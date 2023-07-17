namespace Bloxstrap.Extensions
{
    static class CursorTypeEx
    {
        public static IReadOnlyDictionary<string, CursorType> Selections => new Dictionary<string, CursorType>
        {
            { "Default", CursorType.Default },
            { "2013 (Angular)", CursorType.From2013 },
            { "2006 (Cartoony)", CursorType.From2006 },
        };
    }
}
