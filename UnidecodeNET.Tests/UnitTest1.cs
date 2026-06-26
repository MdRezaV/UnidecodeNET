using System.Text;
using UnidecodeNET.Tables;

namespace UnidecodeNET.Tests
{
    public class UnidecodeTests
    {
        public UnidecodeTests()
        {
            Unidecode.TableLoader = TableRegistry.GetTable;
        }

        [Fact]
        public void TestAsciiToSelf()
        {
            for (var n = 0; n < 128; n++)
            {
                var t = ((char)n).ToString();
                string r = Unidecode.Decode(t);
                Assert.Equal(t, r);
            }
        }

        [Fact]
        public void TestAllToAscii()
        {
            for (var n = 0; n <= 0x1FFFF; n++)
            {
                if (n is >= 0xD800 and <= 0xDFFF)
                {
                    continue;
                }

                string t = char.ConvertFromUtf32(n);
                string u = Unidecode.Decode(t);
                
                foreach (char c in u)
                {
                    Assert.True(c <= 127, $"Character U+{(int)c:X4} is not ASCII");
                }
            }
        }

        [Fact]
        public void TestSurrogates()
        {
            for (var n = 0xD800; n < 0xE000; n++)
            {
                char[] surrogates = [(char)n];
                var s = new string(surrogates);
                string result = Unidecode.Decode(s);
                Assert.Equal("", result);
            }
        }

        [Fact]
        public void TestSpace()
        {
            for (var n = 0x80; n < 0x10000; n++)
            {
                var t = ((char)n).ToString();
                if (char.IsWhiteSpace(t, 0))
                {
                    string s = Unidecode.Decode(t);
                    Assert.True(string.IsNullOrEmpty(s) || s.Trim().Length == 0 || 
                        s.All(c => char.IsWhiteSpace(c) || c == ' '),
                        $"unidecode(U+{n:X4}) should return empty or ASCII space, got: {s}");
                }
            }
        }

        [Fact]
        public void TestSurrogatePairs()
        {
            string s = char.ConvertFromUtf32(0x1D4E3);
            string result = Unidecode.Decode(s);
            Assert.Equal("T", result);
        }

        [Fact]
        public void TestCircledLatin()
        {
            for (var n = 0; n < 26; n++)
            {
                var a = ((char)('a' + n)).ToString();
                string b = Unidecode.Decode(((char)(0x24D0 + n)).ToString());
                Assert.Equal(a, b);
            }
        }

        [Fact]
        public void TestMathematicalLatin()
        {
            var empty = 0;
            for (var n = 0x1D400; n < 0x1D6A4; n++)
            {
                string a = n % 52 < 26 ? ((char)('A' + n % 26)).ToString() : ((char)('a' + n % 26)).ToString();
                
                string b = Unidecode.Decode(char.ConvertFromUtf32(n));

                if (string.IsNullOrEmpty(b))
                {
                    empty++;
                }
                else
                {
                    Assert.Equal(a, b);
                }
            }

            Assert.Equal(24, empty);
        }

        [Fact]
        public void TestMathematicalDigits()
        {
            for (var n = 0x1D7CE; n < 0x1D800; n++)
            {
                var a = ((char)('0' + (n - 0x1D7CE) % 10)).ToString();
                string b = Unidecode.Decode(char.ConvertFromUtf32(n));
                Assert.Equal(a, b);
            }
        }

        [Fact]
        public void TestSpecific()
        {
            var tests = new (string Input, string Expected)[]
            {
                ("Hello, World!", "Hello, World!"),
                ("'\"\r\n", "'\"\r\n"),
                ("ČŽŠčžš", "CZSczs"),
                ("ア", "a"),
                ("α", "a"),
                ("а", "a"),
                ("château", "chateau"),
                ("viñedos", "vinedos"),
                ("\u5317\u4EB0", "Bei Jing "),
                ("Eﬃcient", "Efficient"),
                ("příliš žluťoučký kůň pěl ďábelské ódy", "prilis zlutoucky kun pel dabelske ody"),
                ("PŘÍLIŠ ŽLUŤOUČKÝ KŮŇ PĚL ĎÁBELSKÉ ÓDY", "PRILIS ZLUTOUCKY KUN PEL DABELSKE ODY"),
                ("\ua500", ""),
                ("\u1eff", ""),
            };

            foreach ((string input, string expected) in tests)
            {
                string result = Unidecode.Decode(input);
                Assert.Equal(expected, result);
                Assert.IsType<string>(result);
            }
        }

        [Fact]
        public void TestSpecificWide()
        {
            var tests = new (string Input, string Expected)[]
            {
                (char.ConvertFromUtf32(0x1D5A0), "A"),
                ($"{char.ConvertFromUtf32(0x1D5C4)}{char.ConvertFromUtf32(0x1D5C6)}/{char.ConvertFromUtf32(0x1D5C1)}", "km/h"),
                ($"\u2124{char.ConvertFromUtf32(0x1D552)}{char.ConvertFromUtf32(0x1D55C)}{char.ConvertFromUtf32(0x1D552)}{char.ConvertFromUtf32(0x1D55B)} {char.ConvertFromUtf32(0x1D526)}{char.ConvertFromUtf32(0x1D52A)}{char.ConvertFromUtf32(0x1D51E)} {char.ConvertFromUtf32(0x1D4E4)}{char.ConvertFromUtf32(0x1D4F7)}{char.ConvertFromUtf32(0x1D4F2)}{char.ConvertFromUtf32(0x1D4EC)}{char.ConvertFromUtf32(0x1D4F8)}{char.ConvertFromUtf32(0x1D4ED)}{char.ConvertFromUtf32(0x1D4EE)} {char.ConvertFromUtf32(0x1D4C8)}{char.ConvertFromUtf32(0x1D4C5)}\u212F{char.ConvertFromUtf32(0x1D4B8)}{char.ConvertFromUtf32(0x1D4BE)}{char.ConvertFromUtf32(0x1D4BB)}{char.ConvertFromUtf32(0x1D4BE)}{char.ConvertFromUtf32(0x1D4C0)}{char.ConvertFromUtf32(0x1D4B6)}{char.ConvertFromUtf32(0x1D4B8)}{char.ConvertFromUtf32(0x1D4BE)}{char.ConvertFromUtf32(0x1D4BF)}\u212F {char.ConvertFromUtf32(0x1D59F)}{char.ConvertFromUtf32(0x1D586)} {char.ConvertFromUtf32(0x1D631)}{char.ConvertFromUtf32(0x1D62A)}{char.ConvertFromUtf32(0x1D634)}{char.ConvertFromUtf32(0x1D622)}{char.ConvertFromUtf32(0x1D637)}{char.ConvertFromUtf32(0x1D626)}?!",
                    "Zakaj ima Unicode specifikacije za pisave?!"),
            };

            foreach ((string input, string expected) in tests)
            {
                string result = Unidecode.Decode(input);
                Assert.Equal(expected, result);
                Assert.IsType<string>(result);
            }
        }

        [Fact]
        public void TestWordPressRemoveAccents()
        {
            var wpRemoveAccents = new Dictionary<int[], string>
            {
                // Decompositions for Latin-1 Supplement
                { [194, 170], "a" }, { [194, 186], "o" },
                { [195, 128], "A" }, { [195, 129], "A" },
                { [195, 130], "A" }, { [195, 131], "A" },
                { [195, 133], "A" },
                { [195, 134], "AE" }, { [195, 135], "C" },
                { [195, 136], "E" }, { [195, 137], "E" },
                { [195, 138], "E" }, { [195, 139], "E" },
                { [195, 140], "I" }, { [195, 141], "I" },
                { [195, 142], "I" }, { [195, 143], "I" },
                { [195, 144], "D" }, { [195, 145], "N" },
                { [195, 146], "O" }, { [195, 147], "O" },
                { [195, 148], "O" }, { [195, 149], "O" },
                { [195, 153], "U" },
                { [195, 154], "U" }, { [195, 155], "U" },
                { [195, 157], "Y" },
                { [195, 160], "a" }, { [195, 161], "a" },
                { [195, 162], "a" }, { [195, 163], "a" },
                { [195, 165], "a" },
                { [195, 166], "ae" }, { [195, 167], "c" },
                { [195, 168], "e" }, { [195, 169], "e" },
                { [195, 170], "e" }, { [195, 171], "e" },
                { [195, 172], "i" }, { [195, 173], "i" },
                { [195, 174], "i" }, { [195, 175], "i" },
                { [195, 176], "d" }, { [195, 177], "n" },
                { [195, 178], "o" }, { [195, 179], "o" },
                { [195, 180], "o" }, { [195, 181], "o" },
                { [195, 184], "o" },
                { [195, 185], "u" }, { [195, 186], "u" },
                { [195, 187], "u" },
                { [195, 189], "y" }, { [195, 190], "th" },
                { [195, 191], "y" }, { [195, 152], "O" },
                // Decompositions for Latin Extended-A
                { [196, 128], "A" }, { [196, 129], "a" },
                { [196, 130], "A" }, { [196, 131], "a" },
                { [196, 132], "A" }, { [196, 133], "a" },
                { [196, 134], "C" }, { [196, 135], "c" },
                { [196, 136], "C" }, { [196, 137], "c" },
                { [196, 138], "C" }, { [196, 139], "c" },
                { [196, 140], "C" }, { [196, 141], "c" },
                { [196, 142], "D" }, { [196, 143], "d" },
                { [196, 144], "D" }, { [196, 145], "d" },
                { [196, 146], "E" }, { [196, 147], "e" },
                { [196, 148], "E" }, { [196, 149], "e" },
                { [196, 150], "E" }, { [196, 151], "e" },
                { [196, 152], "E" }, { [196, 153], "e" },
                { [196, 154], "E" }, { [196, 155], "e" },
                { [196, 156], "G" }, { [196, 157], "g" },
                { [196, 158], "G" }, { [196, 159], "g" },
                { [196, 160], "G" }, { [196, 161], "g" },
                { [196, 162], "G" }, { [196, 163], "g" },
                { [196, 164], "H" }, { [196, 165], "h" },
                { [196, 166], "H" }, { [196, 167], "h" },
                { [196, 168], "I" }, { [196, 169], "i" },
                { [196, 170], "I" }, { [196, 171], "i" },
                { [196, 172], "I" }, { [196, 173], "i" },
                { [196, 174], "I" }, { [196, 175], "i" },
                { [196, 176], "I" }, { [196, 177], "i" },
                { [196, 178], "IJ" }, { [196, 179], "ij" },
                { [196, 180], "J" }, { [196, 181], "j" },
                { [196, 182], "K" }, { [196, 183], "k" },
                { [196, 184], "k" }, { [196, 185], "L" },
                { [196, 186], "l" }, { [196, 187], "L" },
                { [196, 188], "l" }, { [196, 189], "L" },
                { [196, 190], "l" }, { [196, 191], "L" },
                { [197, 128], "l" }, { [197, 129], "L" },
                { [197, 130], "l" }, { [197, 131], "N" },
                { [197, 132], "n" }, { [197, 133], "N" },
                { [197, 134], "n" }, { [197, 135], "N" },
                { [197, 136], "n" },
                { [197, 140], "O" }, { [197, 141], "o" },
                { [197, 142], "O" }, { [197, 143], "o" },
                { [197, 144], "O" }, { [197, 145], "o" },
                { [197, 146], "OE" }, { [197, 147], "oe" },
                { [197, 148], "R" }, { [197, 149], "r" },
                { [197, 150], "R" }, { [197, 151], "r" },
                { [197, 152], "R" }, { [197, 153], "r" },
                { [197, 154], "S" }, { [197, 155], "s" },
                { [197, 156], "S" }, { [197, 157], "s" },
                { [197, 158], "S" }, { [197, 159], "s" },
                { [197, 160], "S" }, { [197, 161], "s" },
                { [197, 162], "T" }, { [197, 163], "t" },
                { [197, 164], "T" }, { [197, 165], "t" },
                { [197, 166], "T" }, { [197, 167], "t" },
                { [197, 168], "U" }, { [197, 169], "u" },
                { [197, 170], "U" }, { [197, 171], "u" },
                { [197, 172], "U" }, { [197, 173], "u" },
                { [197, 174], "U" }, { [197, 175], "u" },
                { [197, 176], "U" }, { [197, 177], "u" },
                { [197, 178], "U" }, { [197, 179], "u" },
                { [197, 180], "W" }, { [197, 181], "w" },
                { [197, 182], "Y" }, { [197, 183], "y" },
                { [197, 184], "Y" }, { [197, 185], "Z" },
                { [197, 186], "z" }, { [197, 187], "Z" },
                { [197, 188], "z" }, { [197, 189], "Z" },
                { [197, 190], "z" }, { [197, 191], "s" },
                // Decompositions for Latin Extended-B
                { [200, 152], "S" }, { [200, 153], "s" },
                { [200, 154], "T" }, { [200, 155], "t" },

                // Vowels with diacritic (Vietnamese) - unmarked
                { [198, 160], "O" }, { [198, 161], "o" },
                { [198, 175], "U" }, { [198, 176], "u" },
                // grave accent
                { [225, 186, 166], "A" }, { [225, 186, 167], "a" },
                { [225, 186, 176], "A" }, { [225, 186, 177], "a" },
                { [225, 187, 128], "E" }, { [225, 187, 129], "e" },
                { [225, 187, 146], "O" }, { [225, 187, 147], "o" },
                { [225, 187, 156], "O" }, { [225, 187, 157], "o" },
                { [225, 187, 170], "U" }, { [225, 187, 171], "u" },
                { [225, 187, 178], "Y" }, { [225, 187, 179], "y" },
                // hook
                { [225, 186, 162], "A" }, { [225, 186, 163], "a" },
                { [225, 186, 168], "A" }, { [225, 186, 169], "a" },
                { [225, 186, 178], "A" }, { [225, 186, 179], "a" },
                { [225, 186, 186], "E" }, { [225, 186, 187], "e" },
                { [225, 187, 130], "E" }, { [225, 187, 131], "e" },
                { [225, 187, 136], "I" }, { [225, 187, 137], "i" },
                { [225, 187, 142], "O" }, { [225, 187, 143], "o" },
                { [225, 187, 148], "O" }, { [225, 187, 149], "o" },
                { [225, 187, 158], "O" }, { [225, 187, 159], "o" },
                { [225, 187, 166], "U" }, { [225, 187, 167], "u" },
                { [225, 187, 172], "U" }, { [225, 187, 173], "u" },
                { [225, 187, 182], "Y" }, { [225, 187, 183], "y" },
                // tilde
                { [225, 186, 170], "A" }, { [225, 186, 171], "a" },
                { [225, 186, 180], "A" }, { [225, 186, 181], "a" },
                { [225, 186, 188], "E" }, { [225, 186, 189], "e" },
                { [225, 187, 132], "E" }, { [225, 187, 133], "e" },
                { [225, 187, 150], "O" }, { [225, 187, 151], "o" },
                { [225, 187, 160], "O" }, { [225, 187, 161], "o" },
                { [225, 187, 174], "U" }, { [225, 187, 175], "u" },
                { [225, 187, 184], "Y" }, { [225, 187, 185], "y" },
                // acute accent
                { [225, 186, 164], "A" }, { [225, 186, 165], "a" },
                { [225, 186, 174], "A" }, { [225, 186, 175], "a" },
                { [225, 186, 190], "E" }, { [225, 186, 191], "e" },
                { [225, 187, 144], "O" }, { [225, 187, 145], "o" },
                { [225, 187, 154], "O" }, { [225, 187, 155], "o" },
                { [225, 187, 168], "U" }, { [225, 187, 169], "u" },
                // dot below
                { [225, 186, 160], "A" }, { [225, 186, 161], "a" },
                { [225, 186, 172], "A" }, { [225, 186, 173], "a" },
                { [225, 186, 182], "A" }, { [225, 186, 183], "a" },
                { [225, 186, 184], "E" }, { [225, 186, 185], "e" },
                { [225, 187, 134], "E" }, { [225, 187, 135], "e" },
                { [225, 187, 138], "I" }, { [225, 187, 139], "i" },
                { [225, 187, 140], "O" }, { [225, 187, 141], "o" },
                { [225, 187, 152], "O" }, { [225, 187, 153], "o" },
                { [225, 187, 162], "O" }, { [225, 187, 163], "o" },
                { [225, 187, 164], "U" }, { [225, 187, 165], "u" },
                { [225, 187, 176], "U" }, { [225, 187, 177], "u" },
                { [225, 187, 180], "Y" }, { [225, 187, 181], "y" },
                // Vowels with diacritic (Chinese, Hanyu Pinyin)
                { [201, 145], "a" },
                // macron
                { [199, 149], "U" }, { [199, 150], "u" },
                // acute accent
                { [199, 151], "U" }, { [199, 152], "u" },
                // caron
                { [199, 141], "A" }, { [199, 142], "a" },
                { [199, 143], "I" }, { [199, 144], "i" },
                { [199, 145], "O" }, { [199, 146], "o" },
                { [199, 147], "U" }, { [199, 148], "u" },
                { [199, 153], "U" }, { [199, 154], "u" },
                // grave accent
                { [199, 155], "U" }, { [199, 156], "u" },

                { [195, 132], "A" },
                { [195, 150], "O" },
                { [195, 156], "U" },
                { [195, 164], "a" },
                { [195, 182], "o" },
                { [195, 188], "u" },
            };
            
            foreach (KeyValuePair<int[], string> kvp in wpRemoveAccents)
            {
                var bytes = new byte[kvp.Key.Length];
                for (var i = 0; i < kvp.Key.Length; i++)
                {
                    bytes[i] = (byte)kvp.Key[i];
                }
                string input = Encoding.UTF8.GetString(bytes);
                string output = Unidecode.Decode(input);
                Assert.Equal(kvp.Value, output);
            }
        }

        [Fact]
        public void TestUnicodeTextConverter()
        {
            var lower = new[]
            {
                "\uFF54\uFF48\uFF45 \uFF51\uFF55\uFF49\uFF43\uFF4B \uFF42\uFF52\uFF4F\uFF57\uFF4E \uFF46\uFF4F\uFF58 \uFF4A\uFF55\uFF4D\uFF50\uFF53 \uFF4F\uFF56\uFF45\uFF52 \uFF54\uFF48\uFF45 \uFF4C\uFF41\uFF5A\uFF59 \uFF44\uFF4F\uFF47 \uFF11\uFF12\uFF13\uFF14\uFF15\uFF16\uFF17\uFF18\uFF19\uFF10",
                $"{char.ConvertFromUtf32(0x1D565)}{char.ConvertFromUtf32(0x1D559)}{char.ConvertFromUtf32(0x1D556)} {char.ConvertFromUtf32(0x1D562)}{char.ConvertFromUtf32(0x1D566)}{char.ConvertFromUtf32(0x1D55A)}{char.ConvertFromUtf32(0x1D554)}{char.ConvertFromUtf32(0x1D55C)} {char.ConvertFromUtf32(0x1D553)}{char.ConvertFromUtf32(0x1D563)}{char.ConvertFromUtf32(0x1D560)}{char.ConvertFromUtf32(0x1D568)}{char.ConvertFromUtf32(0x1D55F)} {char.ConvertFromUtf32(0x1D557)}{char.ConvertFromUtf32(0x1D560)}{char.ConvertFromUtf32(0x1D569)} {char.ConvertFromUtf32(0x1D55B)}{char.ConvertFromUtf32(0x1D566)}{char.ConvertFromUtf32(0x1D55E)}{char.ConvertFromUtf32(0x1D561)}{char.ConvertFromUtf32(0x1D564)} {char.ConvertFromUtf32(0x1D560)}{char.ConvertFromUtf32(0x1D567)}{char.ConvertFromUtf32(0x1D556)}{char.ConvertFromUtf32(0x1D563)} {char.ConvertFromUtf32(0x1D565)}{char.ConvertFromUtf32(0x1D559)}{char.ConvertFromUtf32(0x1D556)} {char.ConvertFromUtf32(0x1D55D)}{char.ConvertFromUtf32(0x1D552)}{char.ConvertFromUtf32(0x1D56B)}{char.ConvertFromUtf32(0x1D56A)} {char.ConvertFromUtf32(0x1D555)}{char.ConvertFromUtf32(0x1D560)}{char.ConvertFromUtf32(0x1D558)} {char.ConvertFromUtf32(0x1D7D9)}{char.ConvertFromUtf32(0x1D7DA)}{char.ConvertFromUtf32(0x1D7DB)}{char.ConvertFromUtf32(0x1D7DC)}{char.ConvertFromUtf32(0x1D7DD)}{char.ConvertFromUtf32(0x1D7DE)}{char.ConvertFromUtf32(0x1D7DF)}{char.ConvertFromUtf32(0x1D7E0)}{char.ConvertFromUtf32(0x1D7E1)}{char.ConvertFromUtf32(0x1D7D8)}",
                $"{char.ConvertFromUtf32(0x1D42D)}{char.ConvertFromUtf32(0x1D421)}{char.ConvertFromUtf32(0x1D41E)} {char.ConvertFromUtf32(0x1D42A)}{char.ConvertFromUtf32(0x1D42E)}{char.ConvertFromUtf32(0x1D422)}{char.ConvertFromUtf32(0x1D41C)}{char.ConvertFromUtf32(0x1D424)} {char.ConvertFromUtf32(0x1D41B)}{char.ConvertFromUtf32(0x1D42B)}{char.ConvertFromUtf32(0x1D428)}{char.ConvertFromUtf32(0x1D430)}{char.ConvertFromUtf32(0x1D427)} {char.ConvertFromUtf32(0x1D41F)}{char.ConvertFromUtf32(0x1D428)}{char.ConvertFromUtf32(0x1D431)} {char.ConvertFromUtf32(0x1D423)}{char.ConvertFromUtf32(0x1D42E)}{char.ConvertFromUtf32(0x1D426)}{char.ConvertFromUtf32(0x1D429)}{char.ConvertFromUtf32(0x1D42C)} {char.ConvertFromUtf32(0x1D428)}{char.ConvertFromUtf32(0x1D42F)}{char.ConvertFromUtf32(0x1D41E)}{char.ConvertFromUtf32(0x1D42B)} {char.ConvertFromUtf32(0x1D42D)}{char.ConvertFromUtf32(0x1D421)}{char.ConvertFromUtf32(0x1D41E)} {char.ConvertFromUtf32(0x1D425)}{char.ConvertFromUtf32(0x1D41A)}{char.ConvertFromUtf32(0x1D433)}{char.ConvertFromUtf32(0x1D432)} {char.ConvertFromUtf32(0x1D41D)}{char.ConvertFromUtf32(0x1D428)}{char.ConvertFromUtf32(0x1D420)} {char.ConvertFromUtf32(0x1D7CF)}{char.ConvertFromUtf32(0x1D7D0)}{char.ConvertFromUtf32(0x1D7D1)}{char.ConvertFromUtf32(0x1D7D2)}{char.ConvertFromUtf32(0x1D7D3)}{char.ConvertFromUtf32(0x1D7D4)}{char.ConvertFromUtf32(0x1D7D5)}{char.ConvertFromUtf32(0x1D7D6)}{char.ConvertFromUtf32(0x1D7D7)}{char.ConvertFromUtf32(0x1D7CE)}",
                $"{char.ConvertFromUtf32(0x1D495)}{char.ConvertFromUtf32(0x1D489)}{char.ConvertFromUtf32(0x1D486)} {char.ConvertFromUtf32(0x1D492)}{char.ConvertFromUtf32(0x1D496)}{char.ConvertFromUtf32(0x1D48A)}{char.ConvertFromUtf32(0x1D484)}{char.ConvertFromUtf32(0x1D48C)} {char.ConvertFromUtf32(0x1D483)}{char.ConvertFromUtf32(0x1D493)}{char.ConvertFromUtf32(0x1D490)}{char.ConvertFromUtf32(0x1D498)}{char.ConvertFromUtf32(0x1D48F)} {char.ConvertFromUtf32(0x1D487)}{char.ConvertFromUtf32(0x1D490)}{char.ConvertFromUtf32(0x1D499)} {char.ConvertFromUtf32(0x1D48B)}{char.ConvertFromUtf32(0x1D496)}{char.ConvertFromUtf32(0x1D48E)}{char.ConvertFromUtf32(0x1D491)}{char.ConvertFromUtf32(0x1D494)} {char.ConvertFromUtf32(0x1D490)}{char.ConvertFromUtf32(0x1D497)}{char.ConvertFromUtf32(0x1D486)}{char.ConvertFromUtf32(0x1D493)} {char.ConvertFromUtf32(0x1D495)}{char.ConvertFromUtf32(0x1D489)}{char.ConvertFromUtf32(0x1D486)} {char.ConvertFromUtf32(0x1D48D)}{char.ConvertFromUtf32(0x1D482)}{char.ConvertFromUtf32(0x1D49B)}{char.ConvertFromUtf32(0x1D49A)} {char.ConvertFromUtf32(0x1D485)}{char.ConvertFromUtf32(0x1D490)}{char.ConvertFromUtf32(0x1D488)} 1234567890",
                $"{char.ConvertFromUtf32(0x1D4FD)}{char.ConvertFromUtf32(0x1D4F1)}{char.ConvertFromUtf32(0x1D4EE)} {char.ConvertFromUtf32(0x1D4FA)}{char.ConvertFromUtf32(0x1D4FE)}{char.ConvertFromUtf32(0x1D4F2)}{char.ConvertFromUtf32(0x1D4EC)}{char.ConvertFromUtf32(0x1D4F4)} {char.ConvertFromUtf32(0x1D4EB)}{char.ConvertFromUtf32(0x1D4FB)}{char.ConvertFromUtf32(0x1D4F8)}{char.ConvertFromUtf32(0x1D500)}{char.ConvertFromUtf32(0x1D4F7)} {char.ConvertFromUtf32(0x1D4EF)}{char.ConvertFromUtf32(0x1D4F8)}{char.ConvertFromUtf32(0x1D501)} {char.ConvertFromUtf32(0x1D4F3)}{char.ConvertFromUtf32(0x1D4FE)}{char.ConvertFromUtf32(0x1D4F6)}{char.ConvertFromUtf32(0x1D4F9)}{char.ConvertFromUtf32(0x1D4FC)} {char.ConvertFromUtf32(0x1D4F8)}{char.ConvertFromUtf32(0x1D4FF)}{char.ConvertFromUtf32(0x1D4EE)}{char.ConvertFromUtf32(0x1D4FB)} {char.ConvertFromUtf32(0x1D4FD)}{char.ConvertFromUtf32(0x1D4F1)}{char.ConvertFromUtf32(0x1D4EE)} {char.ConvertFromUtf32(0x1D4F5)}{char.ConvertFromUtf32(0x1D4EA)}{char.ConvertFromUtf32(0x1D503)}{char.ConvertFromUtf32(0x1D502)} {char.ConvertFromUtf32(0x1D4ED)}{char.ConvertFromUtf32(0x1D4F8)}{char.ConvertFromUtf32(0x1D4F0)} 1234567890",
                $"{char.ConvertFromUtf32(0x1D599)}{char.ConvertFromUtf32(0x1D58D)}{char.ConvertFromUtf32(0x1D58A)} {char.ConvertFromUtf32(0x1D596)}{char.ConvertFromUtf32(0x1D59A)}{char.ConvertFromUtf32(0x1D58E)}{char.ConvertFromUtf32(0x1D588)}{char.ConvertFromUtf32(0x1D590)} {char.ConvertFromUtf32(0x1D587)}{char.ConvertFromUtf32(0x1D597)}{char.ConvertFromUtf32(0x1D594)}{char.ConvertFromUtf32(0x1D59C)}{char.ConvertFromUtf32(0x1D593)} {char.ConvertFromUtf32(0x1D58B)}{char.ConvertFromUtf32(0x1D594)}{char.ConvertFromUtf32(0x1D59D)} {char.ConvertFromUtf32(0x1D58F)}{char.ConvertFromUtf32(0x1D59A)}{char.ConvertFromUtf32(0x1D592)}{char.ConvertFromUtf32(0x1D595)}{char.ConvertFromUtf32(0x1D598)} {char.ConvertFromUtf32(0x1D594)}{char.ConvertFromUtf32(0x1D59B)}{char.ConvertFromUtf32(0x1D58A)}{char.ConvertFromUtf32(0x1D597)} {char.ConvertFromUtf32(0x1D599)}{char.ConvertFromUtf32(0x1D58D)}{char.ConvertFromUtf32(0x1D58A)} {char.ConvertFromUtf32(0x1D591)}{char.ConvertFromUtf32(0x1D586)}{char.ConvertFromUtf32(0x1D59F)}{char.ConvertFromUtf32(0x1D59E)} {char.ConvertFromUtf32(0x1D589)}{char.ConvertFromUtf32(0x1D594)}{char.ConvertFromUtf32(0x1D58C)} 1234567890",
            };

            foreach (string s in lower)
            {
                string o = Unidecode.Decode(s);
                Assert.Equal("the quick brown fox jumps over the lazy dog 1234567890", o);
            }

            var upper = new[]
            {
                "\uFF34\uFF28\uFF25 \uFF31\uFF35\uFF29\uFF23\uFF2B \uFF22\uFF32\uFF2F\uFF37\uFF2E \uFF26\uFF2F\uFF38 \uFF2A\uFF35\uFF2D\uFF30\uFF33 \uFF2F\uFF36\uFF25\uFF32 \uFF34\uFF28\uFF25 \uFF2C\uFF21\uFF3A\uFF39 \uFF24\uFF2F\uFF27 \uFF11\uFF12\uFF13\uFF14\uFF15\uFF16\uFF17\uFF18\uFF19\uFF10",
                $"{char.ConvertFromUtf32(0x1D54B)}\u210D{char.ConvertFromUtf32(0x1D53C)} \u211A{char.ConvertFromUtf32(0x1D54C)}{char.ConvertFromUtf32(0x1D540)}\u2102{char.ConvertFromUtf32(0x1D542)} {char.ConvertFromUtf32(0x1D539)}\u211D{char.ConvertFromUtf32(0x1D546)}{char.ConvertFromUtf32(0x1D54E)}\u2115 {char.ConvertFromUtf32(0x1D53D)}{char.ConvertFromUtf32(0x1D546)}{char.ConvertFromUtf32(0x1D54F)} {char.ConvertFromUtf32(0x1D541)}{char.ConvertFromUtf32(0x1D54C)}{char.ConvertFromUtf32(0x1D544)}\u2119{char.ConvertFromUtf32(0x1D54A)} {char.ConvertFromUtf32(0x1D546)}{char.ConvertFromUtf32(0x1D54D)}{char.ConvertFromUtf32(0x1D53C)}\u211D {char.ConvertFromUtf32(0x1D54B)}\u210D{char.ConvertFromUtf32(0x1D53C)} {char.ConvertFromUtf32(0x1D543)}{char.ConvertFromUtf32(0x1D538)}\u2124{char.ConvertFromUtf32(0x1D550)} {char.ConvertFromUtf32(0x1D53B)}{char.ConvertFromUtf32(0x1D546)}{char.ConvertFromUtf32(0x1D53E)} {char.ConvertFromUtf32(0x1D7D9)}{char.ConvertFromUtf32(0x1D7DA)}{char.ConvertFromUtf32(0x1D7DB)}{char.ConvertFromUtf32(0x1D7DC)}{char.ConvertFromUtf32(0x1D7DD)}{char.ConvertFromUtf32(0x1D7DE)}{char.ConvertFromUtf32(0x1D7DF)}{char.ConvertFromUtf32(0x1D7E0)}{char.ConvertFromUtf32(0x1D7E1)}{char.ConvertFromUtf32(0x1D7D8)}",
                $"{char.ConvertFromUtf32(0x1D413)}{char.ConvertFromUtf32(0x1D407)}{char.ConvertFromUtf32(0x1D404)} {char.ConvertFromUtf32(0x1D410)}{char.ConvertFromUtf32(0x1D414)}{char.ConvertFromUtf32(0x1D408)}{char.ConvertFromUtf32(0x1D402)}{char.ConvertFromUtf32(0x1D40A)} {char.ConvertFromUtf32(0x1D401)}{char.ConvertFromUtf32(0x1D411)}{char.ConvertFromUtf32(0x1D40E)}{char.ConvertFromUtf32(0x1D416)}{char.ConvertFromUtf32(0x1D40D)} {char.ConvertFromUtf32(0x1D405)}{char.ConvertFromUtf32(0x1D40E)}{char.ConvertFromUtf32(0x1D417)} {char.ConvertFromUtf32(0x1D409)}{char.ConvertFromUtf32(0x1D414)}{char.ConvertFromUtf32(0x1D40C)}{char.ConvertFromUtf32(0x1D40F)}{char.ConvertFromUtf32(0x1D412)} {char.ConvertFromUtf32(0x1D40E)}{char.ConvertFromUtf32(0x1D415)}{char.ConvertFromUtf32(0x1D404)}{char.ConvertFromUtf32(0x1D411)} {char.ConvertFromUtf32(0x1D413)}{char.ConvertFromUtf32(0x1D407)}{char.ConvertFromUtf32(0x1D404)} {char.ConvertFromUtf32(0x1D40B)}{char.ConvertFromUtf32(0x1D400)}{char.ConvertFromUtf32(0x1D419)}{char.ConvertFromUtf32(0x1D418)} {char.ConvertFromUtf32(0x1D403)}{char.ConvertFromUtf32(0x1D40E)}{char.ConvertFromUtf32(0x1D406)} {char.ConvertFromUtf32(0x1D7CF)}{char.ConvertFromUtf32(0x1D7D0)}{char.ConvertFromUtf32(0x1D7D1)}{char.ConvertFromUtf32(0x1D7D2)}{char.ConvertFromUtf32(0x1D7D3)}{char.ConvertFromUtf32(0x1D7D4)}{char.ConvertFromUtf32(0x1D7D5)}{char.ConvertFromUtf32(0x1D7D6)}{char.ConvertFromUtf32(0x1D7D7)}{char.ConvertFromUtf32(0x1D7CE)}",
                $"{char.ConvertFromUtf32(0x1D47B)}{char.ConvertFromUtf32(0x1D46F)}{char.ConvertFromUtf32(0x1D46C)} {char.ConvertFromUtf32(0x1D478)}{char.ConvertFromUtf32(0x1D47C)}{char.ConvertFromUtf32(0x1D470)}{char.ConvertFromUtf32(0x1D46A)}{char.ConvertFromUtf32(0x1D472)} {char.ConvertFromUtf32(0x1D469)}{char.ConvertFromUtf32(0x1D479)}{char.ConvertFromUtf32(0x1D476)}{char.ConvertFromUtf32(0x1D47E)}{char.ConvertFromUtf32(0x1D475)} {char.ConvertFromUtf32(0x1D46D)}{char.ConvertFromUtf32(0x1D476)}{char.ConvertFromUtf32(0x1D47F)} {char.ConvertFromUtf32(0x1D471)}{char.ConvertFromUtf32(0x1D47C)}{char.ConvertFromUtf32(0x1D474)}{char.ConvertFromUtf32(0x1D477)}{char.ConvertFromUtf32(0x1D47A)} {char.ConvertFromUtf32(0x1D476)}{char.ConvertFromUtf32(0x1D47D)}{char.ConvertFromUtf32(0x1D46C)}{char.ConvertFromUtf32(0x1D479)} {char.ConvertFromUtf32(0x1D47B)}{char.ConvertFromUtf32(0x1D46F)}{char.ConvertFromUtf32(0x1D46C)} {char.ConvertFromUtf32(0x1D473)}{char.ConvertFromUtf32(0x1D468)}{char.ConvertFromUtf32(0x1D481)}{char.ConvertFromUtf32(0x1D480)} {char.ConvertFromUtf32(0x1D46B)}{char.ConvertFromUtf32(0x1D476)}{char.ConvertFromUtf32(0x1D46E)} 1234567890",
                $"{char.ConvertFromUtf32(0x1D4E3)}{char.ConvertFromUtf32(0x1D4D7)}{char.ConvertFromUtf32(0x1D4D4)} {char.ConvertFromUtf32(0x1D4E0)}{char.ConvertFromUtf32(0x1D4E4)}{char.ConvertFromUtf32(0x1D4D8)}{char.ConvertFromUtf32(0x1D4D2)}{char.ConvertFromUtf32(0x1D4DA)} {char.ConvertFromUtf32(0x1D4D1)}{char.ConvertFromUtf32(0x1D4E1)}{char.ConvertFromUtf32(0x1D4DE)}{char.ConvertFromUtf32(0x1D4E6)}{char.ConvertFromUtf32(0x1D4DD)} {char.ConvertFromUtf32(0x1D4D5)}{char.ConvertFromUtf32(0x1D4DE)}{char.ConvertFromUtf32(0x1D4E7)} {char.ConvertFromUtf32(0x1D4D9)}{char.ConvertFromUtf32(0x1D4E4)}{char.ConvertFromUtf32(0x1D4DC)}{char.ConvertFromUtf32(0x1D4DF)}{char.ConvertFromUtf32(0x1D4E2)} {char.ConvertFromUtf32(0x1D4DE)}{char.ConvertFromUtf32(0x1D4E5)}{char.ConvertFromUtf32(0x1D4D4)}{char.ConvertFromUtf32(0x1D4E1)} {char.ConvertFromUtf32(0x1D4E3)}{char.ConvertFromUtf32(0x1D4D7)}{char.ConvertFromUtf32(0x1D4D4)} {char.ConvertFromUtf32(0x1D4DB)}{char.ConvertFromUtf32(0x1D4D0)}{char.ConvertFromUtf32(0x1D4E9)}{char.ConvertFromUtf32(0x1D4E8)} {char.ConvertFromUtf32(0x1D4D3)}{char.ConvertFromUtf32(0x1D4DE)}{char.ConvertFromUtf32(0x1D4D6)} 1234567890",
                $"{char.ConvertFromUtf32(0x1D57F)}{char.ConvertFromUtf32(0x1D573)}{char.ConvertFromUtf32(0x1D570)} {char.ConvertFromUtf32(0x1D57C)}{char.ConvertFromUtf32(0x1D580)}{char.ConvertFromUtf32(0x1D574)}{char.ConvertFromUtf32(0x1D56E)}{char.ConvertFromUtf32(0x1D576)} {char.ConvertFromUtf32(0x1D56D)}{char.ConvertFromUtf32(0x1D57D)}{char.ConvertFromUtf32(0x1D57A)}{char.ConvertFromUtf32(0x1D582)}{char.ConvertFromUtf32(0x1D579)} {char.ConvertFromUtf32(0x1D571)}{char.ConvertFromUtf32(0x1D57A)}{char.ConvertFromUtf32(0x1D583)} {char.ConvertFromUtf32(0x1D575)}{char.ConvertFromUtf32(0x1D580)}{char.ConvertFromUtf32(0x1D578)}{char.ConvertFromUtf32(0x1D57B)}{char.ConvertFromUtf32(0x1D57E)} {char.ConvertFromUtf32(0x1D57A)}{char.ConvertFromUtf32(0x1D581)}{char.ConvertFromUtf32(0x1D570)}{char.ConvertFromUtf32(0x1D57D)} {char.ConvertFromUtf32(0x1D57F)}{char.ConvertFromUtf32(0x1D573)}{char.ConvertFromUtf32(0x1D570)} {char.ConvertFromUtf32(0x1D577)}{char.ConvertFromUtf32(0x1D56C)}{char.ConvertFromUtf32(0x1D585)}{char.ConvertFromUtf32(0x1D584)} {char.ConvertFromUtf32(0x1D56F)}{char.ConvertFromUtf32(0x1D57A)}{char.ConvertFromUtf32(0x1D572)} 1234567890",
            };

            foreach (string s in upper)
            {
                string o = Unidecode.Decode(s);
                Assert.Equal("THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 1234567890", o);
            }
        }

        [Fact]
        public void TestEnclosedAlphanumerics()
        {
            Assert.Equal("aA20(20)20.20100", Unidecode.Decode("ⓐⒶ⑳⒇⒛⓴⓾⓿"));
        }

        [Fact]
        public void TestErrorsIgnore()
        {
            string o = Unidecode.Decode($"test {char.ConvertFromUtf32(0xF0000)} test");
            Assert.Equal("test  test", o);
        }

        [Fact]
        public void TestErrorsReplace()
        {
            string o = Unidecode.Decode($"test {char.ConvertFromUtf32(0xF0000)} test", ErrorHandling.Replace);
            Assert.Equal("test ? test", o);
        }

        [Fact]
        public void TestErrorsReplaceStr()
        {
            string o = Unidecode.Decode($"test {char.ConvertFromUtf32(0xF0000)} test", ErrorHandling.Replace, "[?] ");
            Assert.Equal("test [?]  test", o);
        }

        [Fact]
        public void TestErrorsStrict()
        {
            var ex = Assert.Throws<UnidecodeException>(() =>
                Unidecode.Decode($"test {char.ConvertFromUtf32(0xF0000)} test", ErrorHandling.Strict));
            Assert.Equal(5, ex.Index);
        }

        [Fact]
        public void TestErrorsPreserve()
        {
            var s = $"test {char.ConvertFromUtf32(0xF0000)} test";
            string o = Unidecode.Decode(s, ErrorHandling.Preserve);
            Assert.Equal(s, o);
        }

        [Fact]
        public void TestErrorsInvalid()
        {
            Assert.Throws<UnidecodeException>(() =>
                Unidecode.Decode($"test {char.ConvertFromUtf32(0xF0000)} test", (ErrorHandling)999));
        }

        [Fact]
        public void TestDegree()
        {
            Assert.Equal(Unidecode.Decode("\u2109"), Unidecode.Decode("\u00B0F"));
            Assert.Equal(Unidecode.Decode("\u2103"), Unidecode.Decode("\u00B0C"));
        }
    }
}