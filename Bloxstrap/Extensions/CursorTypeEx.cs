using System.Collections.Generic;

using Bloxstrap.Enums;

namespace Bloxstrap.Extensions
{
    static class CursorTypeEx
    {
        public static IReadOnlyDictionary<string, CursorType> Selections => new Dictionary<string, CursorType>
        {
            { "Default", CursorType.Default },
            { "Before 2022", CursorType.From2013 },
            { "Before 2013", CursorType.From2006 },
        };
    }
}
