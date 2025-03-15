using Bloxstrap.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Exceptions
{
    internal class CustomThemeException : Exception
    {
        /// <summary>
        /// The exception message in English (for logging)
        /// </summary>
        public string EnglishMessage { get; } = null!;

        public CustomThemeException(string translationString)
            : base(Strings.ResourceManager.GetStringSafe(translationString))
        {
            EnglishMessage = Strings.ResourceManager.GetStringSafe(translationString, new CultureInfo("en-GB"));
        }

        public CustomThemeException(Exception innerException, string translationString)
            : base(Strings.ResourceManager.GetStringSafe(translationString), innerException)
        {
            EnglishMessage = Strings.ResourceManager.GetStringSafe(translationString, new CultureInfo("en-GB"));
        }

        public CustomThemeException(string translationString, params object?[] args)
            : base(string.Format(Strings.ResourceManager.GetStringSafe(translationString), args))
        {
            EnglishMessage = string.Format(Strings.ResourceManager.GetStringSafe(translationString, new CultureInfo("en-GB")), args);
        }

        public CustomThemeException(Exception innerException, string translationString, params object?[] args)
            : base(string.Format(Strings.ResourceManager.GetStringSafe(translationString), args), innerException)
        {
            EnglishMessage = string.Format(Strings.ResourceManager.GetStringSafe(translationString, new CultureInfo("en-GB")), args);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(GetType().ToString());

            if (!string.IsNullOrEmpty(Message))
                sb.Append($": {Message}");

            if (!string.IsNullOrEmpty(EnglishMessage) && Message != EnglishMessage)
                sb.Append($" ({EnglishMessage})");

            if (InnerException != null)
                sb.Append($"\r\n ---> {InnerException}\r\n   ");

            if (StackTrace != null)
                sb.Append($"\r\n{StackTrace}");

            return sb.ToString();
        }
    }
}
