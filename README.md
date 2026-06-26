# UnidecodeNET

**ASCII transliterations of Unicode text in .NET**

[![License](https://img.shields.io/badge/license-GPL--2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4.svg)](https://dotnet.microsoft.com/)

---

It often happens that you have text data in Unicode, but you need to represent it in ASCII. For example:

- Integrating with legacy code that doesn't support Unicode
- Ease of entry of non-Roman names on a US keyboard
- Constructing ASCII machine identifiers from human-readable Unicode strings (e.g. URL slugs)

> **Unidecode is not a replacement for fully supporting Unicode in your program.** There are a number of caveats that come with its use, especially when its output is directly visible to users. Please read the rest of this README before using UnidecodeNET in your project.

In most cases you could represent Unicode characters as `???` or `\u15BA\u15A0\u1610`, to mention two extremes. But that's nearly useless to someone who actually wants to read what the text says.

**UnidecodeNET provides a middle road:** the `Decode()` method takes a Unicode string and tries to represent it in ASCII characters (the universally displayable characters between `0x00` and `0x7F`), where the compromises taken when mapping between two character sets are chosen to be near what a human with a US keyboard would choose.

The quality of the resulting ASCII representation varies. For languages of Western origin it should be between perfect and good. On the other hand, transliteration of languages like Chinese, Japanese, or Korean is a very complex issue and this library does not even attempt to address it — it draws the line at context-free character-by-character mapping.

**A good rule of thumb:** the further the script you are transliterating is from the Latin alphabet, the worse the transliteration will be.

Generally UnidecodeNET produces better results than simply stripping accents from characters (which can be done with .NET's `String.Normalize()`). It is based on hand-tuned character mappings that also contain ASCII approximations for symbols and non-Latin alphabets.

This is a **C#/.NET port** of the [Unidecode](https://github.com/avian2/unidecode) Python library by Tomaž Šolc, which itself is a port of the [`Text::Unidecode`](https://metacpan.org/pod/Text::Unidecode) Perl module by Sean M. Burke.

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Error Handling](#error-handling)
- [Advanced Usage](#advanced-usage)
- [Performance](#performance)
- [Frequently Asked Questions](#frequently-asked-questions)
- [Credits](#credits)
- [License](#license)

---

## Installation

Clone the repository and add the project to your solution:

```bash
git clone https://github.com/MdRezaV/UnidecodeNET.git
```

Then reference the project from your solution:

```bash
dotnet add reference path/to/UnidecodeNET/UnidecodeNET.csproj
```

### Requirements

- **.NET 6.0** or later (uses `System.Text.Rune` for proper surrogate pair handling)
- Works with .NET Core, .NET 5+, and .NET 6+

---

## Quick Start

```csharp
using Unidecode;

string ascii = Unidecode.Decode("kožušček");
// => "kozuscek"

string speed = Unidecode.Decode("30 \U0001d5c4\U0001d5c6/\U0001d5c1");
// => "30 km/h"

string city = Unidecode.Decode("\u5317\u4EB0");
// => "Bei Jing "

string czech = Unidecode.Decode("příliš žluťoučký kůň pěl ďábelské ódy");
// => "prilis zlutoucky kun pel dabelske ody"
```

---

## API Reference

### `Unidecode.Decode(string input, ErrorHandling errors = ErrorHandling.Ignore, string replaceStr = "?")`

Transliterates a Unicode string into ASCII.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `input` | `string` | *required* | The Unicode string to transliterate |
| `errors` | `ErrorHandling` | `Ignore` | How to handle characters without a transliteration |
| `replaceStr` | `string` | `"?"` | Replacement string when `errors = Replace` |

**Returns:** an ASCII `string`.

### `Unidecode.DecodeCodepoint(int codepoint)`

Transliterates a single Unicode codepoint.

**Returns:** the ASCII replacement `string`, or `null` if no replacement exists.

### `Unidecode.ClearCache()`

Clears the in-memory transliteration table cache.

### `Unidecode.TableLoader`

A `Func<int, string[]?>` delegate used to load transliteration tables by section number. Defaults to `TableRegistry.GetTable`. Override this to implement custom loading strategies (lazy disk I/O, embedded resources, etc.).

---

## Error Handling

The `errors` parameter controls what happens when Unidecode encounters a character that is **not present** in its transliteration tables.

### `ErrorHandling.Ignore` (default)

Unknown characters are silently dropped:

```csharp
Unidecode.Decode("test \ue000 test");
// => "test  test"
```

### `ErrorHandling.Strict`

Throws an `UnidecodeException` containing the index of the offending character:

```csharp
try
{
    Unidecode.Decode("\ue000", ErrorHandling.Strict);
}
catch (UnidecodeException ex)
{
    Console.WriteLine($"Failed at index {ex.Index}: {ex.Message}");
}
// => Failed at index 0: No replacement found for character U+E000 in position 0
```

### `ErrorHandling.Replace`

Substitutes unknown characters with `replaceStr` (default `"?"`):

```csharp
Unidecode.Decode("test \ue000 test", ErrorHandling.Replace);
// => "test ? test"

Unidecode.Decode("test \ue000 test", ErrorHandling.Replace, "[?] ");
// => "test [?]  test"
```

### `ErrorHandling.Preserve`

Keeps the original character. **Note:** the returned string may no longer be ASCII-encodable.

```csharp
Unidecode.Decode("test \ue000 test", ErrorHandling.Preserve);
// => "test \ue000 test"
```

---

## Advanced Usage

### Custom Table Loader

For applications where you want to lazily load transliteration tables (e.g. from disk or embedded resources) instead of eagerly loading them all:

```csharp
Unidecode.TableLoader = section =>
{
    // Your custom loading logic
    string path = $"Tables/X{section:X3}.bin";
    return File.Exists(path) ? LoadTable(path) : null;
};
```

### Processing Large Text

For streaming or large-text processing, split the input into manageable chunks:

```csharp
foreach (var line in File.ReadLines("unicode.txt"))
{
    string ascii = Unidecode.Decode(line);
    Console.WriteLine(ascii);
}
```

### URL Slug Generation

```csharp
string title = "Un été à Paris — 2024";
string slug = Unidecode.Decode(title)
    .ToLowerInvariant()
    .Replace(" ", "-")
    .Trim('-');
// => "un-ete-a-paris-2024"
```

---

## Performance

UnidecodeNET is optimized for typical .NET workloads:

- **Fast path for ASCII input:** if the input is already ASCII-only, the string is returned immediately without table lookups.
- **Concurrent caching:** transliteration tables are cached in a thread-safe `ConcurrentDictionary`, so each table is loaded only once.
- **Efficient surrogate handling:** uses `string.EnumerateRunes()` for correct and fast handling of non-BMP characters.

For performance-critical scenarios, reuse the same `Unidecode` instance across calls — the cached tables will be shared.

---

## Frequently Asked Questions

### German umlauts are transliterated "incorrectly"

Latin letters **a**, **o**, and **u** with diaeresis are transliterated as `a`, `o`, `u` — **not** according to German rules (`ae`, `oe`, `ue`). This is intentional and inherited from the original Unidecode.

**Rationale:** these letters are used in languages other than German (for example, Finnish and Turkish). German text transliterated without the extra `e` is much more readable than other languages transliterated using German rules.

**Workaround:** do your own replacements before passing the string to `Decode()`:

```csharp
string input = "Über";
input = input.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
             .Replace("Ä", "Ae").Replace("Ö", "Oe").Replace("Ü", "Ue")
             .Replace("ß", "ss");
string ascii = Unidecode.Decode(input);
// => "Ueber"
```

### Japanese Kanji is transliterated as Chinese

Same as with Latin letters with accents: Unicode encodes **letters**, not letters in a certain language or their meaning. For certain characters used in both Japanese and Chinese, Unidecode chose Chinese transliterations.

If you need Japanese-specific transliteration, consider using other libraries that do language-specific transliteration (e.g. Unihandecode).

### Unidecode should support localization

Language-specific transliteration is a complicated problem and beyond the scope of this library. Consider other libraries such as [Unihandecode](https://github.com/miurahr/unihandecode).

### Unidecode should automatically detect the language

Language detection is a completely separate problem and beyond the scope of this library.

### Unidecode produces completely wrong results

The strings you are passing to Unidecode have likely been wrongly decoded somewhere in your program. For example, you might be reading UTF-8 bytes with a Latin-1 decoder. Inspect your strings with `BitConverter.ToString(Encoding.UTF8.GetBytes(input))` to diagnose encoding issues.

### I've upgraded and now some URLs on my website return 404

Occasionally, new versions of Unidecode are released with improvements to the transliteration tables. **You cannot rely on `Decode()` output being stable across versions.** If you use it to generate URL slugs, either:

1. Generate the slug once and store it in the database, **or**
2. Pin your dependency to a specific version.

---

## Credits

- **Original transliteration tables:** Sean M. Burke ([`Text::Unidecode`](https://metacpan.org/pod/Text::Unidecode))
- **Python port:** Tomaž Šolc ([Unidecode](https://github.com/avian2/unidecode))
- **C#/.NET port:** [MdRezaV](https://github.com/MdRezaV)

---

## License

This project is licensed under the **GNU General Public License v2.0** — see the [LICENSE](LICENSE) file for details.

Original character transliteration tables:
Copyright © 2001 Sean M. Burke, all rights reserved.

Python code and later additions:
Copyright © 2025 Tomaž Šolc.

---

## Links

- **GitHub:** [https://github.com/MdRezaV/UnidecodeNET](https://github.com/MdRezaV/UnidecodeNET)
- **Issues:** [https://github.com/MdRezaV/UnidecodeNET/issues](https://github.com/MdRezaV/UnidecodeNET/issues)
- **Original Unidecode (Python):** [https://github.com/avian2/unidecode](https://github.com/avian2/unidecode)
