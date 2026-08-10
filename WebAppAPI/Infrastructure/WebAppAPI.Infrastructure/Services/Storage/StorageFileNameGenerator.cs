using System.Globalization;
using System.Text;

namespace WebAppAPI.Infrastructure.Services.Storage
{
    public sealed class StorageFileNameGenerator
    {
        public IEnumerable<string> GenerateCandidates(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            string safeFileName = Path.GetFileName(fileName.Trim());

            string extension = Path.GetExtension(safeFileName);
            string originalStem = Path.GetFileNameWithoutExtension(safeFileName);

            string normalizedStem = NormalizeStem(originalStem);

            if (string.IsNullOrEmpty(normalizedStem))
                normalizedStem = "invalid-name";

            yield return $"{normalizedStem}{extension}";

            string baseStem = normalizedStem;
            long suffix = 0;

            if (TryGetNumericSuffix(normalizedStem, out string parsedBaseStem, out long parsedSuffix))
            {
                baseStem = parsedBaseStem;
                suffix = parsedSuffix;
            }

            while (true)
            {
                suffix = checked(suffix + 1);

                yield return string.Create(CultureInfo.InvariantCulture, $"{baseStem}-{suffix}{extension}");
            }
        }

        private static string NormalizeStem(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            StringBuilder builder = new(value.Length);
            bool previousWasSeparator = false;

            foreach (char rawCharacter in value)
            {
                char character = NormalizeCharacter(rawCharacter);

                if (IsAsciiLetterOrDigit(character))
                {
                    builder.Append(character);
                    previousWasSeparator = false;
                    continue;
                }

                if (character == '-' || char.IsWhiteSpace(character))
                {
                    if (builder.Length > 0 && !previousWasSeparator)
                    {
                        builder.Append('-');
                        previousWasSeparator = true;
                    }
                }
            }

            if (builder.Length > 0 && builder[^1] == '-')
                builder.Length--;

            return builder.ToString();
        }

        private static char NormalizeCharacter(char character)
        {
            return character switch
            {
                'ç' or 'Ç' => 'c',
                'ğ' or 'Ğ' => 'g',
                'ı' or 'I' or 'İ' => 'i',
                'ş' or 'Ş' => 's',
                'ü' or 'Ü' => 'u',
                'ö' or 'Ö' => 'o',
                >= 'A' and <= 'Z' => (char)(character + ('a' - 'A')),
                _ => character
            };
        }

        private static bool IsAsciiLetterOrDigit(char character)
            => character is >= 'a' and <= 'z' or >= '0' and <= '9';

        private static bool TryGetNumericSuffix(string stem, out string baseStem, out long suffix)
        {
            baseStem = stem;
            suffix = 0;

            int dashIndex = stem.LastIndexOf('-');

            if (dashIndex <= 0 || dashIndex == stem.Length - 1)
                return false;

            ReadOnlySpan<char> suffixSpan = stem.AsSpan(dashIndex + 1);

            // "file-1" is numeric, but "file-0" and "file-01" are not.
            if (suffixSpan[0] == '0')
                return false;

            if (!long.TryParse(suffixSpan, NumberStyles.None, CultureInfo.InvariantCulture, out suffix))
                return false;

            baseStem = stem[..dashIndex];

            return true;
        }
    }
}