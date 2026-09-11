using Arribbaa.MimicParty.ModdingCore;
using Arribbaa.MimicParty.ModdingCore.Native;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

internal static class NativeCapacityPatches
{
    // 2026-09-11 later Steam update: the previous four inline "5 player" constants
    // were refactored into one shared helper. For this exact verified build, patch
    // only the helper's base value. Its stock return is 4/5 depending on mode; using
    // desiredMaxPlayers - 1 preserves that one-player delta and yields the configured
    // value on the normal online path.
    private const string DynamicCapacityBuildSha256 =
        "44bbc82bdae73c1c86559a1f091ee9c7a3ae510a02c2d83f16b686ecdd9c8b11";

    private const string DynamicCapacityHelper =
        "33 C0 83 F9 01 0F 95 C0 83 C0 04 C3";

    // Legacy runtime signatures used by the earlier supported September builds.
    private const string HostCapacityGate =
        "48 8B 43 40 48 85 C0 0F 84 ?? ?? ?? ?? 83 78 18 05 " +
        "0F 8D ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 83 B9 E4 00 00 00 00";

    private const string ConnectStatusMax =
        "48 8D 54 24 30 48 8B D8 C7 44 24 30 05 00 00 00 " +
        "E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 45 33 C9 4C 8B C0 48 8B D3";

    private const string FullRoomMax =
        "48 8D 54 24 30 48 8B D8 C7 44 24 30 05 00 00 00 " +
        "E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B D0";

    private const string RoomListMax =
        "48 8D 4C 24 60 48 89 44 24 38 0F 57 C0 8B 44 24 5C 4C 8B CD " +
        "44 88 64 24 30 4D 8B C5 C7 44 24 28 05 00 00 00 48 8B D7 89 44 24 20";

    public static PatchTransaction Create(int desiredMaxPlayers)
    {
        if (desiredMaxPlayers is < 6 or > 10)
            throw new ArgumentOutOfRangeException(nameof(desiredMaxPlayers), "Supported player count is 6-10.");

        byte replacement = checked((byte)desiredMaxPlayers);

        if (string.Equals(
                CoreApi.Build.GameAssemblySha256,
                DynamicCapacityBuildSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            byte dynamicBase = checked((byte)(desiredMaxPlayers - 1));

            return CoreApi.CreatePatchTransaction(PluginConstants.Guid)
                .Add(
                    "Shared dynamic capacity helper",
                    DynamicCapacityHelper,
                    patchOffset: 10,
                    expected: new byte[] { 0x04 },
                    replacement: new byte[] { dynamicBase });
        }

        return CoreApi.CreatePatchTransaction(PluginConstants.Guid)
            .Add(
                "Host connection capacity",
                HostCapacityGate,
                patchOffset: 16,
                expected: new byte[] { 0x05 },
                replacement: new byte[] { replacement })
            .Add(
                "Connect status maximum",
                ConnectStatusMax,
                patchOffset: 12,
                expected: new byte[] { 0x05 },
                replacement: new byte[] { replacement })
            .Add(
                "Full-room maximum",
                FullRoomMax,
                patchOffset: 12,
                expected: new byte[] { 0x05 },
                replacement: new byte[] { replacement })
            .Add(
                "Room-list maximum",
                RoomListMax,
                patchOffset: 32,
                expected: new byte[] { 0x05 },
                replacement: new byte[] { replacement });
    }
}
