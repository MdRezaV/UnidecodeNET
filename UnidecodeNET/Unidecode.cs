using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using UnidecodeNET.Tables;

namespace UnidecodeNET
{
    public enum ErrorHandling
    {
        Ignore,
        Strict,
        Replace,
        Preserve
    }

    public class UnidecodeException(string message, int index = -1) : Exception(message)
    {
        public int Index { get; } = index;
    }

    public static class Unidecode
    {
        private static readonly ConcurrentDictionary<int, string[]?> Cache = new();

        private static Func<int, string[]?> _tableLoader = TableRegistry.GetTable;

        /// <summary>
        /// Gets or sets the function used to load transliteration tables by section number.
        /// Defaults to <see cref="TableRegistry.GetTable"/>.
        /// </summary>
        public static Func<int, string[]?> TableLoader
        {
            get => _tableLoader;
            set => _tableLoader = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Transliterates a Unicode string into ASCII.
        /// </summary>
        public static string Decode(string input, ErrorHandling errors = ErrorHandling.Ignore, string replaceStr = "?")
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (IsAscii(input))
            {
                return input;
            }

            var sb = new StringBuilder(input.Length);
            var index = 0;

            foreach (Rune rune in input.EnumerateRunes())
            {
                int codepoint = rune.Value;
                string? replacement = GetReplacement(codepoint);

                if (replacement == null)
                {
                    switch (errors)
                    {
                        case ErrorHandling.Ignore:
                            break;
                        case ErrorHandling.Strict:
                            throw new UnidecodeException(
                                $"No replacement found for character U+{codepoint:X4} in position {index}",
                                index);
                        case ErrorHandling.Replace:
                            sb.Append(replaceStr);
                            break;
                        case ErrorHandling.Preserve:
                            sb.Append(rune.ToString());
                            break;
                        default:
                            throw new UnidecodeException($"Invalid value for errors parameter: {errors}");
                    }
                }
                else
                {
                    sb.Append(replacement);
                }

                index += rune.Utf16SequenceLength;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transliterates a single Unicode codepoint. Returns null if no replacement exists.
        /// </summary>
        public static string? DecodeCodepoint(int codepoint)
        {
            return GetReplacement(codepoint);
        }

        /// <summary>
        /// Clears the cached transliteration tables.
        /// </summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }

        private static string? GetReplacement(int codepoint)
        {
            switch (codepoint)
            {
                case < 0x80:
                    return ((char)codepoint).ToString();
                case > 0xEFFFF:
                case >= 0xD800 and <= 0xDFFF:
                    return null;
            }

            int section = codepoint >> 8;
            int position = codepoint & 0xFF;

            if (!Cache.TryGetValue(section, out string[]? table))
            {
                table = _tableLoader(section);
                Cache[section] = table;
            }

            if (table != null && position < table.Length)
            {
                return table[position];
            }

            return null;
        }

        private static bool IsAscii(string s)
        {
            return s.All(c => c <= 0x7F);
        }
    }
}