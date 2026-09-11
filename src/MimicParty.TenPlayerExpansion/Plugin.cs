using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Arribbaa.MimicParty.ModdingCore;
using Arribbaa.MimicParty.ModdingCore.Native;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

[BepInPlugin(PluginConstants.Guid, PluginConstants.Name, PluginConstants.Version)]
[BepInProcess(PluginConstants.ProcessName)]
[BepInDependency(CoreConstants.Guid, ">=1.0.0")]
public sealed class Plugin : BasePlugin
{
    private HarmonyRuntimeHooks? _harmonyHooks;
    private PatchTransaction? _capacityPatches;

    public override void Load()
    {
        ConfigEntry<int> maxPlayersConfig = Config.Bind(
            "Multiplayer",
            "MaxPlayers",
            10,
            "Maximum total lobby size for Classic and Versus. Supported range: 6-10. In Versus, players choose RED/BLUE themselves; the mod only raises the total room capacity.");

        int maxPlayers = Math.Clamp(maxPlayersConfig.Value, 6, 10);
        if (maxPlayers != maxPlayersConfig.Value)
        {
            Log.LogWarning(
                $"Configured MaxPlayers={maxPlayersConfig.Value} is outside the supported range. Using {maxPlayers}.");
        }

        RuntimeState.Configure(maxPlayers, playbackPhaseValue: 4);

        Log.LogInfo($"{PluginConstants.Name} v{PluginConstants.Version} by arribbaa starting.");
        Log.LogInfo($"Requested maximum players for Classic + Versus: {maxPlayers}");
        Log.LogInfo("Versus team distribution is not forced; players can switch RED/BLUE themselves up to the total room cap.");
        Log.LogInfo($"Detected GameAssembly: {CoreApi.Build.GameAssemblySha256}");
        Log.LogInfo($"Detected metadata:     {CoreApi.Build.MetadataSha256}");

        try
        {
            _harmonyHooks = new HarmonyRuntimeHooks(Log);
            _harmonyHooks.Install();

            _capacityPatches = NativeCapacityPatches.Create(maxPlayers);
            IReadOnlyList<RuntimePatchHandle> capacityHandles = _capacityPatches.Apply();
            NativeCapacityPatches.VerifyRuntime(maxPlayers, capacityHandles, Log);

            CoreApi.Mods.Register(PluginConstants.Guid, PluginConstants.Name, PluginConstants.Version);

            Log.LogInfo(
                $"{PluginConstants.Name} is active: {maxPlayers} total players in Classic/Versus, free RED/BLUE team choice in Versus, performance voice isolation, rematch keep-lobby.");
        }
        catch
        {
            try { _capacityPatches?.Dispose(); } catch { }
            try { _harmonyHooks?.Dispose(); } catch { }

            _capacityPatches = null;
            _harmonyHooks = null;
            throw;
        }
    }

    public override bool Unload()
    {
        try
        {
            _capacityPatches?.Dispose();
            _harmonyHooks?.Dispose();
        }
        finally
        {
            CoreApi.Mods.Unregister(PluginConstants.Guid);
            _capacityPatches = null;
            _harmonyHooks = null;
        }

        return true;
    }
}
