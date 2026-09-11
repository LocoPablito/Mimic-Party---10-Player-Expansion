using Arribbaa.MimicParty.ModdingCore;
using Arribbaa.MimicParty.ModdingCore.Native;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

internal static class NativeCapacityPatches
{
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
