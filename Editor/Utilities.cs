using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace JesseStiller.PhlayerTool {
    internal static class Utilities {
        private static readonly char[] invalidPathChars = {
            '"', '<', '>', '|', char.MinValue, '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', '\f',
            '\r', '\x000E', '\x000F', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019',
            '\x001A', '\x001B', '\x001C', '\x001D', '\x001E', '\x001F', '*', '?'
        };
        private static readonly StringBuilder sb = new StringBuilder();
        internal static string ConvertToValidIdentifier(string s) {
            sb.Length = 0;
            for(int c = 0; c < s.Length; c++) {
                if(IsCharValidForIdentifier(s[c])) {
                    sb.Append(s[c]);
                } else {
                    sb.Append('_');
                }
            }
            return sb.ToString();
        }

        internal static bool IsCharValidForIdentifier(char c) {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            return (uc >= UnicodeCategory.UppercaseLetter && uc <= UnicodeCategory.SpacingCombiningMark) || uc == UnicodeCategory.DecimalDigitNumber ||
                uc == UnicodeCategory.LetterNumber || uc == UnicodeCategory.Format || uc == UnicodeCategory.ConnectorPunctuation;
        }

        internal static bool IsDirectoryPathCharacterValid(char c) {
            foreach(char c2 in invalidPathChars) {
                if(c2 == c) return false;
            }
            return true;
        }

        internal static string ConvertToValidDirectoryPath(string s) {
            sb.Length = 0;
            for(int c = 0; c < s.Length; c++) {
                if(IsDirectoryPathCharacterValid(s[c])) {
                    sb.Append(s[c]);
                } else {
                    sb.Append('_');
                }
            }
            return sb.ToString();
        }

        internal static string GetLocalPathFromAbsolutePath(string absolutePath) {
            if(absolutePath.Length <= Application.dataPath.Length) return "";
            return absolutePath.Substring(Application.dataPath.Length + 1);
        }
    }
}
