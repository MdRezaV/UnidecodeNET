#!/usr/bin/env python3
"""Convert unidecode x###.py data files into C# static classes.

Usage:
    python convert_to_csharp.py [unidecode_dir] [-o output_dir]

Default source directory: unidecode
Default output directory: UnidecodeTables

Example:
    python convert_to_csharp.py unidecode -o src/Unidecode/Tables
"""

import os
import re
import sys
import argparse
import importlib.util


def csharp_escape(s: str) -> str:
    """Escape a Python string for use inside a C# double-quoted string literal."""
    s = s.replace('\\', '\\\\')
    s = s.replace('"', '\\"')
    s = s.replace('\n', '\\n')
    s = s.replace('\r', '\\r')
    s = s.replace('\t', '\\t')
    s = s.replace('\0', '\\0')
    return s


def load_data_module(path: str):
    """Load a Python module from a file path and return its `data` attribute."""
    spec = importlib.util.spec_from_file_location("_conv_data_module", path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Could not load module spec for {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module.data


def format_entry(value) -> str:
    """Format a single tuple entry as a C# array element."""
    if value is None:
        return "null"
    return '"' + csharp_escape(str(value)) + '"'


def generate_cs_file(section_hex: str, data: tuple) -> str:
    """Generate the content of a C# file for a given section."""
    class_name = "X" + section_hex.upper()

    lines = [
        f"// Auto-generated from unidecode/x{section_hex}.py",
        f"namespace Unidecode.Tables",
        "{",
        f"    public static class {class_name}",
        "    {",
        "        public static readonly string[] Data = new string[]",
        "        {",
    ]

    # 8 entries per line for readability
    chunk_size = 8
    for i in range(0, len(data), chunk_size):
        chunk = data[i:i + chunk_size]
        entries = ", ".join(format_entry(entry) for entry in chunk)
        trailing = "" if (i + chunk_size >= len(data)) else ","
        lines.append(f"            {entries}{trailing}")

    lines.append("        };")
    lines.append("    }")
    lines.append("}")
    lines.append("")  # trailing newline

    return "\n".join(lines)


def collect_section_hexes(directory: str) -> list:
    pattern = re.compile(r"^X([0-9A-F]+)\.cs$")
    sections = []
    for name in sorted(os.listdir(directory)):
        m = pattern.match(name)
        if m:
            sections.append(m.group(1).upper())
    return sections


def generate_registry(sections: list) -> str:
    """Generate a TableRegistry.cs that maps sections to their tables."""
    lines = [
        "// Auto-generated table registry",
        "using System.Collections.Generic;",
        "",
        "namespace Unidecode.Tables",
        "{",
        "    public static class TableRegistry",
        "    {",
        "        private static readonly Dictionary<int, string?>[] _empty = System.Array.Empty<Dictionary<int, string?>[]>();",
        "",
        "        private static readonly Dictionary<int, string[]> Tables = new Dictionary<int, string[]>",
        "        {",
    ]
    for sec in sections:
        lines.append(f"            {{ 0x{sec}, X{sec}.Data }},")
    lines.extend([
        "        };",
        "",
        "        /// <summary>",
        "        /// Returns the transliteration table for the given Unicode section,",
        "        /// or null if no data is available for that section.",
        "        /// </summary>",
        "        public static string[]? GetTable(int section)",
        "        {",
        "            return Tables.TryGetValue(section, out var data) ? data : null;",
        "        }",
        "    }",
        "}",
        "",
    ])
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(
        description="Convert unidecode x###.py data files into C# static classes."
    )
    parser.add_argument(
        "source_dir",
        nargs="?",
        default="unidecode",
        help="Directory containing x###.py files (default: unidecode)",
    )
    parser.add_argument(
        "-o", "--output",
        default="UnidecodeTables",
        help="Output directory for .cs files (default: UnidecodeTables)",
    )
    parser.add_argument(
        "--no-registry",
        action="store_true",
        help="Do not generate TableRegistry.cs",
    )
    args = parser.parse_args()

    source_dir = args.source_dir
    output_dir = args.output

    if not os.path.isdir(source_dir):
        sys.exit(f"Error: source directory '{source_dir}' does not exist.")

    os.makedirs(output_dir, exist_ok=True)

    pattern = re.compile(r"^x([0-9a-fA-F]+)\.py$")
    converted = 0
    errors = 0

    for filename in sorted(os.listdir(source_dir)):
        match = pattern.match(filename)
        if not match:
            continue

        section_hex = match.group(1).lower()
        src_path = os.path.join(source_dir, filename)

        try:
            data = load_data_module(src_path)
        except Exception as e:
            print(f"Warning: failed to load {filename}: {e}", file=sys.stderr)
            errors += 1
            continue

        if not isinstance(data, tuple):
            print(f"Warning: {filename}: 'data' is not a tuple, skipping", file=sys.stderr)
            errors += 1
            continue

        cs_content = generate_cs_file(section_hex, data)
        out_name = f"X{section_hex.upper()}.cs"
        out_path = os.path.join(output_dir, out_name)

        with open(out_path, "w", encoding="utf-8", newline="\n") as f:
            f.write(cs_content)

        converted += 1

    print(f"Converted {converted} file(s) -> {output_dir}/")
    if errors:
        print(f"Skipped {errors} file(s) due to errors.", file=sys.stderr)

    if not args.no_registry:
        sections = collect_section_hexes(output_dir)
        if sections:
            registry_path = os.path.join(output_dir, "TableRegistry.cs")
            with open(registry_path, "w", encoding="utf-8", newline="\n") as f:
                f.write(generate_registry(sections))
            print(f"Generated registry: {registry_path} ({len(sections)} table(s))")

    print()
    print("Wire it up in your C# app:")
    print("    Unidecode.TableLoader = TableRegistry.GetTable;")
    print('    string ascii = Unidecode.Decode("北京");')


if __name__ == "__main__":
    main()