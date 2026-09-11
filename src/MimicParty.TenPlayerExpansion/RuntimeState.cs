namespace Arribbaa.MimicParty.TenPlayerExpansion;

internal static class RuntimeState
{
    private static readonly object Gate = new();

    public static int DesiredMaxPlayers { get; private set; } = 10;
    public static int PlaybackPhaseValue { get; private set; } = 4;
    public static int CurrentPhase { get; private set; } = -1;
    public static int CurrentPerformerIndex { get; private set; } = -1;
    public static bool RematchScope { get; private set; }

    public static void Configure(int desiredMaxPlayers, int playbackPhaseValue)
    {
        lock (Gate)
        {
            DesiredMaxPlayers = desiredMaxPlayers;
            PlaybackPhaseValue = playbackPhaseValue;
            CurrentPhase = -1;
            CurrentPerformerIndex = -1;
            RematchScope = false;
        }
    }

    public static void SetPhase(int phase)
    {
        lock (Gate)
        {
            CurrentPhase = phase;

            if (phase != PlaybackPhaseValue)
                CurrentPerformerIndex = -1;
        }
    }

    public static void SetCurrentPerformer(int playerIndex)
    {
        lock (Gate)
            CurrentPerformerIndex = playerIndex;
    }

    public static void EnterRematch()
    {
        lock (Gate)
            RematchScope = true;
    }

    public static void ExitRematch()
    {
        lock (Gate)
            RematchScope = false;
    }

    public static void ResetRoundState()
    {
        lock (Gate)
        {
            CurrentPhase = -1;
            CurrentPerformerIndex = -1;
            RematchScope = false;
        }
    }

    public static bool ShouldCloseLiveVoice
    {
        get
        {
            lock (Gate)
                return CurrentPhase == PlaybackPhaseValue && CurrentPerformerIndex >= 0;
        }
    }
}
