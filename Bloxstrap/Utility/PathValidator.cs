using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Utility
{
    internal static class PathValidator
    {
        public enum ValidationResult
        {
            Ok,
            IllegalCharacter,
            ReservedFileName,
            ReservedDirectoryName
        }

        private static readonly string[] _reservedNames = new string[]
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9"
        };

        private static readonly char[] _directorySeperatorDelimiters = new char[]
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        private static readonly char[] _invalidPathChars = GetInvalidPathChars();

        public static char[] GetInvalidPathChars()
        {
            char[] invalids = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
            char[] otherInvalids = Path.GetInvalidPathChars();

            char[] result = new char[invalids.Length + otherInvalids.Length];
            invalids.CopyTo(result, 0);
            otherInvalids.CopyTo(result, invalids.Length);

            return result;
        }

        public static ValidationResult IsFileNameValid(string fileName)
        {
            if (fileName.IndexOfAny(_invalidPathChars) != -1)
                return ValidationResult.IllegalCharacter;

            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            if (_reservedNames.Contains(fileNameNoExt))
                return ValidationResult.ReservedFileName;

            return ValidationResult.Ok;
        }

        public static ValidationResult IsPathValid(string path)
        {
            string? pathRoot = Path.GetPathRoot(path);
            string pathNoRoot = pathRoot != null ? path[pathRoot.Length..] : path;

            string[] pathParts = pathNoRoot.Split(_directorySeperatorDelimiters);

            foreach (var part in pathParts)
            {
                if (part.IndexOfAny(_invalidPathChars) != -1)
                    return ValidationResult.IllegalCharacter;

                if (_reservedNames.Contains(part))
                    return ValidationResult.ReservedDirectoryName;
            }

            string fileName = Path.GetFileName(path);
            if (fileName.IndexOfAny(_invalidPathChars) != -1)
                return ValidationResult.IllegalCharacter;

            string fileNameNoExt = Path.GetFileNameWithoutExtension(path).ToUpperInvariant();
            if (_reservedNames.Contains(fileNameNoExt))
                return ValidationResult.ReservedFileName;

            return ValidationResult.Ok;
        }
    }
}
