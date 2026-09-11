using System.Reflection;
using HarmonyLib;
using Arribbaa.MimicParty.ModdingCore.Runtime;
using BepInEx.Logging;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

internal sealed class HarmonyRuntimeHooks : IDisposable
{
    private readonly Harmony _harmony;
    private readonly ManualLogSource _log;
    private bool _installed;
    private bool _patchingStarted;

    public HarmonyRuntimeHooks(ManualLogSource log)
    {
        _log = log;
        _harmony = new Harmony(PluginConstants.Guid);
    }

    public void Install()
    {
        if (_installed)
            return;

        Type fusionNetwork = ReflectionResolver.FindUniqueType(
            "FusionNetworkService",
            "get_MaxPlayers",
            "OnConnectRequest");

        Type voiceDirector = ReflectionResolver.FindUniqueType(
            "VoiceDirector",
            "get_Open");

        Type roundController = ReflectionResolver.FindUniqueType(
            "RoundController",
            "SetPhase",
            "PublishCurrentPlayer");

        Type resultsScreen = ReflectionResolver.FindUniqueType(
            "ResultsScreen",
            "LaunchRematch",
            "DropSilentPlayers");

        Type roundPhase = ReflectionResolver.FindUniqueType("RoundPhase");
        if (!roundPhase.IsEnum)
            throw new TypeLoadException($"Resolved type '{roundPhase.FullName}' is not an enum.");

        object playback = Enum.Parse(roundPhase, "Playback", ignoreCase: false);
        int playbackPhaseValue = Convert.ToInt32(playback);

        MethodInfo maxPlayers = ReflectionResolver.FindUniqueMethod(fusionNetwork, "get_MaxPlayers", 0);
        MethodInfo setPhase = ReflectionResolver.FindUniqueMethod(roundController, "SetPhase", 1);
        MethodInfo publishCurrentPlayer = ReflectionResolver.FindUniqueMethod(roundController, "PublishCurrentPlayer", 1);
        MethodInfo voiceOpen = ReflectionResolver.FindUniqueMethod(voiceDirector, "get_Open", 0);
        MethodInfo launchRematch = ReflectionResolver.FindUniqueMethod(resultsScreen, "LaunchRematch", 0);

        MethodInfo dropSilent = resultsScreen
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(m => m.Name == "DropSilentPlayers");

        Type? offlineNetwork = ReflectionResolver.TryFindUniqueType("OfflineNetworkService", "get_MaxPlayers");
        MethodInfo? offlineMaxPlayers = offlineNetwork is null
            ? null
            : ReflectionResolver.FindUniqueMethod(offlineNetwork, "get_MaxPlayers", 0);

        Type? gameState = ReflectionResolver.TryFindUniqueType("GameState", "ResetForRematch");
        MethodInfo? resetForRematch = gameState is null
            ? null
            : ReflectionResolver.FindUniqueMethod(gameState, "ResetForRematch", 0);

        RuntimeState.Configure(RuntimeState.DesiredMaxPlayers, playbackPhaseValue);

        try
        {
            _patchingStarted = true;

            PatchPostfix(maxPlayers, nameof(MaxPlayersPostfix));

            if (offlineMaxPlayers is not null)
                PatchPostfix(offlineMaxPlayers, nameof(MaxPlayersPostfix));

            PatchPostfix(setPhase, nameof(SetPhasePostfix));
            PatchPostfix(publishCurrentPlayer, nameof(PublishCurrentPlayerPostfix));
            PatchPostfix(voiceOpen, nameof(VoiceOpenPostfix));

            PatchPrefix(launchRematch, nameof(LaunchRematchPrefix));
            PatchPostfix(launchRematch, nameof(LaunchRematchPostfix));
            PatchFinalizer(launchRematch, nameof(LaunchRematchFinalizer));
            PatchPrefix(dropSilent, nameof(DropSilentPlayersPrefix));

            if (resetForRematch is not null)
                PatchPostfix(resetForRematch, nameof(ResetRoundStatePostfix));

            _installed = true;
            _patchingStarted = false;
            _log.LogInfo($"Harmony runtime hooks installed. Playback enum value = {playbackPhaseValue}.");
        }
        catch
        {
            try { _harmony.UnpatchSelf(); }
            finally
            {
                _installed = false;
                _patchingStarted = false;
                RuntimeState.ResetRoundState();
            }

            throw;
        }
    }

    private void PatchPrefix(MethodBase target, string patchMethod) =>
        _harmony.Patch(target, prefix: new HarmonyMethod(typeof(HarmonyRuntimeHooks), patchMethod));

    private void PatchPostfix(MethodBase target, string patchMethod) =>
        _harmony.Patch(target, postfix: new HarmonyMethod(typeof(HarmonyRuntimeHooks), patchMethod));

    private void PatchFinalizer(MethodBase target, string patchMethod) =>
        _harmony.Patch(target, finalizer: new HarmonyMethod(typeof(HarmonyRuntimeHooks), patchMethod));

    public void Dispose()
    {
        if (!_installed && !_patchingStarted)
            return;

        try
        {
            _harmony.UnpatchSelf();
        }
        finally
        {
            _installed = false;
            _patchingStarted = false;
            RuntimeState.ResetRoundState();
        }
    }

    private static void MaxPlayersPostfix(ref int __result)
    {
        __result = RuntimeState.DesiredMaxPlayers;
    }

    private static void SetPhasePostfix(object[] __args)
    {
        if (__args.Length == 0 || __args[0] is null)
            return;

        RuntimeState.SetPhase(Convert.ToInt32(__args[0]));
    }

    private static void PublishCurrentPlayerPostfix(object[] __args)
    {
        if (__args.Length == 0 || __args[0] is null)
            return;

        RuntimeState.SetCurrentPerformer(Convert.ToInt32(__args[0]));
    }

    private static void VoiceOpenPostfix(ref bool __result)
    {
        if (__result && RuntimeState.ShouldCloseLiveVoice)
            __result = false;
    }

    private static void LaunchRematchPrefix()
    {
        RuntimeState.EnterRematch();
    }

    private static void LaunchRematchPostfix()
    {
        RuntimeState.ExitRematch();
    }

    private static Exception? LaunchRematchFinalizer(Exception? __exception)
    {
        RuntimeState.ExitRematch();
        return __exception;
    }

    private static bool DropSilentPlayersPrefix()
    {
        return !RuntimeState.RematchScope;
    }

    private static void ResetRoundStatePostfix()
    {
        RuntimeState.ResetRoundState();
    }
}
