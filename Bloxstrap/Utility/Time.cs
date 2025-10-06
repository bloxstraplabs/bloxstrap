using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Utility
{
    internal static class Time
    {
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            Func<Tuple<int, string>, string> tupleFormatter = t => $"{t.Item1} {t.Item2}{(t.Item1 == 1 ? string.Empty : "s")}";
            var components = new List<Tuple<int, string>>
            {
                Tuple.Create((int) timeSpan.TotalDays, "day"),
                Tuple.Create(timeSpan.Hours, "hour"),
                Tuple.Create(timeSpan.Minutes, "minute")
            };

            components.RemoveAll(i => i.Item1 == 0);

            string extra = "";

            if (components.Count > 1)
            {
                var finalComponent = components[components.Count - 1];
                components.RemoveAt(components.Count - 1);
                extra = $" and {tupleFormatter(finalComponent)}";
            }

            return $"{string.Join(", ", components.Select(tupleFormatter))}{extra}";
        }
    }
}
