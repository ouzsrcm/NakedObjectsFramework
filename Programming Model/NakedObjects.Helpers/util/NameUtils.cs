// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using System.Linq;
using System.Text;
using NakedObjects.Resources;

namespace NakedObjects.Util {
    /// <summary>
    /// Utility methods for manipulating type-name strings.  The Naked Objects framework makes extensive
    /// use of these utils, but they are provided within the NakedObjects.Helpers
    /// assembly to permit optional use within domain code.
    /// </summary>
    public static class NameUtils {
        private const char space = ' ';

        /// <summary>
        ///     Return a lower case, non-spaced version of the specified name
        /// </summary>
        public static string SimpleName(string name) {
            var sb = new StringBuilder(name.Length);
            foreach (char ch in name) {
                if (!Char.IsWhiteSpace(ch)) {
                    sb.Append(Char.ToLower(ch));
                }
            }
            return sb.ToString();
        }


        /// <summary>
        ///     Returns a word spaced version of the specified name, so there are spaces between the words, where each
        ///     word starts with a capital letter. E.g., "NextAvailableDate" is returned as "Next Available Date".
        /// </summary>
        public static string NaturalName(string name) {
            int length = name.Length;

            if (length <= 1) {
                return name.ToUpper(); // ensure first character is upper case
            }

            var naturalName = new StringBuilder(length);

            char character = Char.ToUpper(name[0]); // ensure first character is upper case
            naturalName.Append(character);
            char nextCharacter = name[1];

            for (int pos = 2; pos < length; pos++) {
                char previousCharacter = character;
                character = nextCharacter;
                nextCharacter = name[pos];

                if (!Char.IsWhiteSpace(previousCharacter)) {
                    if (Char.IsUpper(character) && !Char.IsUpper(previousCharacter)) {
                        naturalName.Append(space);
                    }
                    if (Char.IsUpper(character) && Char.IsLower(nextCharacter) && Char.IsUpper(previousCharacter)) {
                        naturalName.Append(space);
                    }
                    if (Char.IsDigit(character) && !Char.IsDigit(previousCharacter)) {
                        naturalName.Append(space);
                    }
                }
                naturalName.Append(character);
            }
            naturalName.Append(nextCharacter);
            return naturalName.ToString();
        }

        public static string PluralName(string name) {
            string pluralName;
            if (name.EndsWith("y")) {
                pluralName = name.Substring(0, (name.Length - 1) - (0)) + "ies";
            }
            else if (name.EndsWith("s") || name.EndsWith("x")) {
                pluralName = name + "es";
            }
            else {
                pluralName = name + 's';
            }
            return pluralName;
        }

        public static string CapitalizeName(string name) {
            return Char.ToUpper(name[0]) + name.Substring(1);
        }

        private static bool IsStartOfNewWord(char c, char previousChar) {
            return char.IsUpper(c) || char.IsDigit(c) && !char.IsDigit(previousChar);
        }

        public static string MakeTitle(string name) {
            int pos = 0;

            // find first upper case character
            while ((pos < name.Length) && char.IsLower(name[pos])) {
                pos++;
            }

            if (pos == name.Length) {
                return ProgrammingModel.InvalidName;
            }
            var s = new StringBuilder(name.Length - pos); // remove is/get/set
            for (int j = pos; j < name.Length; j++) {
                // process english name - add spaces
                if ((j > pos) && IsStartOfNewWord(name[j], name[j - 1])) {
                    s.Append(' ');
                }
                s.Append(name[j]);
            }
            return s.ToString();
        }

        public static string[] NaturalNames(Type typeOfEnum) {
            return Enum.GetNames(typeOfEnum).Select(NaturalName).ToArray();
        }
    }
}