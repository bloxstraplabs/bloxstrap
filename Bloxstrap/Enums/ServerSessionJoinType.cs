namespace Bloxstrap.Enums
{
    public enum ServerSessionJoinType
    {
        NewGameNoAvailableSlots = 1,
        NewGameSinglePlayer = 2,
        NewGamePrivateGame = 4,
        Specific = 5,
        SpecificPrivateGame = 6,
        MatchMade = 10,
    }
}