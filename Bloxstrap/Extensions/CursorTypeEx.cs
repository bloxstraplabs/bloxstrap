namespace Bloxstrap.Extensions
{
    static class CursorTypeEx
    {
        public static IReadOnlyCollection<CursorType> Selections => new CursorType[]
        {
            CursorType.Default,
            CursorType.From2013,
            CursorType.From2006
        };
    }
}
