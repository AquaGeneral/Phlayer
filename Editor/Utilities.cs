using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace JesseStiller.PhLayerTool {
    internal static class Utilities {
        private readonly static StringBuilder sb = new StringBuilder();
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
    }
}
