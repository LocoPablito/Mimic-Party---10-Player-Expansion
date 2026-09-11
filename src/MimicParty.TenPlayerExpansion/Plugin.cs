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
            "Maximum lobby size. Supported range: 6-10. Photon room capacity is 10.");

        int maxPlayers = Math.Clamp(maxPlayersConfig.Value, 6, 10);
        if (maxPlayers != maxPlayersConfig.Value)
        {
            Log.LogWarning(
                $"Configured MaxPlayers={maxPlayersConfig.Value} is outside the supported range. Using {maxPlayers}.");
        }

        RuntimeState.Configure(maxPlayers, playbackPhaseValue: 4);

        Log.LogInfo($"{PluginConstants.Name} v{PluginConstants.Version} by arribbaa starting.");
        Log.LogInfo($"Requested maximum players: {maxPlayers}");
        Log.LogInfo($"Detected GameAssembly: {CoreApi.Build.GameAssemblySha256}");
        Log.LogInfo($"Detected metadata:     {CoreApi.Build.MetadataSha256}");

        try
        {
            _harmonyHooks = new HarmonyRuntimeHooks(Log);
            _harmonyHooks.Install();

            _capacityPatches = NativeCapacityPatches.Create(maxPlayers);
            _capacityPatches.Apply();

            CoreApi.Mods.Register(PluginConstants.Guid, PluginConstants.Name, PluginConstants.Version);

            Log.LogInfo(
                $"{PluginConstants.Name} is active: {maxPlayers} players, performance voice isolation, rematch keep-lobby.");
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
