using Arribbaa.MimicParty.ModdingCore;
using Arribbaa.MimicParty.ModdingCore.Native;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

internal static class NativeCapacityPatches
{
    // 2026-09-11 later Steam update: the previous four inline capacity constants
    // were refactored into one shared mode helper. Stock behavior returns 5 for
    // Classic and 4 for Versus. Patch the helper's return path itself so the
    // configured capacity applies to BOTH modes. With MaxPlayers=10 this gives
    // Classic 10-player rooms and a 10-player Versus room for 5v5 testing.
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
            // Original bytes at +5:
            //   0F 95 C0       setne al
            //   83 C0 04       add eax, 4
            //   C3             ret
            //
            // Replacement:
            //   B8 xx 00 00 00 mov eax, desiredMaxPlayers
            //   C3             ret
            //   90             nop
            //
            // The earlier xor/cmp instructions remain harmless. The result no longer
            // differs by mode, so Classic and Versus both advertise/use the configured
            // total room capacity.
            return CoreApi.CreatePatchTransaction(PluginConstants.Guid)
                .Add(
                    "Shared Classic/Versus capacity helper",
                    DynamicCapacityHelper,
                    patchOffset: 5,
                    expected: new byte[] { 0x0F, 0x95, 0xC0, 0x83, 0xC0, 0x04, 0xC3 },
                    replacement: new byte[] { 0xB8, replacement, 0x00, 0x00, 0x00, 0xC3, 0x90 });
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
