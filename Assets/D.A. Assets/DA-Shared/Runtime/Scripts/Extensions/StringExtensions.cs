﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace DA_Assets.Extensions
{
    public static class StringExtensions
    {
        public static string RemovePathExtension(this string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
            {
                return Path.Combine(directory, fileNameWithoutExtension).Replace('\\', '/');
            }

            return fileNameWithoutExtension;
        }

        public static bool IsValidEmail(this string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsSpecialNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string cleanName = Regex.Replace(value, @"[\%\#\$\€\£\₴\¥\₹\₩\₪\₦\฿\₫\₹\₺\₱\₽]", "").Trim();
            return cleanName.TryParseFloat(out float _);
        }

        const string phonePattern = @"^\+?[0-9\s\-\(\)]+$";
        public static bool IsPhoneNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) 
                return false;

            return Regex.IsMatch(value, phonePattern);
        }

        public static string MakeCharLower(this string str, int charIndex)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (charIndex < 0 || charIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex));

            char[] strArray = str.ToCharArray();
            strArray[charIndex] = char.ToLower(strArray[charIndex]);
            return new string(strArray);
        }

        public static string MakeCharUpper(this string str, int charIndex)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (charIndex < 0 || charIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(charIndex));

            char[] strArray = str.ToCharArray();
            strArray[charIndex] = char.ToUpper(strArray[charIndex]);
            return new string(strArray);
        }

        public static string MakeFirstCharUpper(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// https://stackoverflow.com/a/11161232
        /// </summary>
        public static long GetNumberOnly(this string str)
        {
            string result = Regex.Replace(str, @"[^\d]", "");

            if (long.TryParse(result, out long res))
            {
                return res;
            }

            return 0;
        }

        public static string ReplaceNotNumbers(this string str, char @char)
        {
            char[] chars = str.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsDigit(chars[i]))
                {
                    chars[i] = @char;
                }
            }
            return new string(chars);
        }

        public static string RemoveRepeats(this string str, char @char)
        {
            if (str.IsEmpty())
                return str;

            return Regex.Replace(str, $"{@char}+", $"{@char}");
        }

        public static string RemoveRepeatsForNonAlphanumericCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Regex.Replace(str, @"([^a-zA-Z0-9])\1+", "$1");
        }

        public static float GetSizeMB(this string myString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(myString);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                long sizeInBytes = stream.Length;
                float sizeInMegabytes = (float)sizeInBytes / (1024 * 1024);

                sizeInMegabytes = (float)Math.Floor(sizeInMegabytes * 100) / 100;
                return sizeInMegabytes;
            }
        }

        public static string AddWithNewLine(this object source)
        {
            string result = "";
            result += Environment.NewLine;
            result += source;
            return result;
        }

        public static bool IsLastChar(this string str, Func<char, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }
            char lastChar = str.Last();
            //UnityEngine.Debug.LogError($"IsLastChar | {str} | {str.Last()} | {condition(lastChar)}");

            return condition(lastChar);
        }

        /// <summary>
        /// https://stackoverflow.com/a/448225
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllUpper(this string input)
        {
            input = input.RemoveNonLettersAndNonNumbers("");

            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsUpper(input[i]))
                    return false;
            }

            return true;
        }

        public static string RemoveNonNumbers(this string str, string replace)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return Regex.Replace(str, "[^0-9]", replace);
        }

        public static string RemoveNonLettersAndNonNumbers(this string str, string replace)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return Regex.Replace(str, "[^a-zA-Z0-9]", replace);
        }

        public static bool IsLetterOrNumber(this char c) => Regex.IsMatch(c.ToString(), "[a-zA-Z0-9]");

        /// <summary>
        /// https://stackoverflow.com/a/63055998
        /// </summary>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            if (str.Length < 2)
            {
                return str.ToLowerInvariant();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(str[0]));

            for (int i = 1; i < str.Length; ++i)
            {
                char c = str[i];

                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string ToUpperSnakeCase(this string str)
        {
            string snakeCase = ToSnakeCase(str);

            if (snakeCase.IsEmpty())
            {
                return snakeCase;
            }

            return snakeCase.ToUpper();
        }

        /// <summary>
        /// https://stackoverflow.com/a/46095771
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(str, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }

        public static string ToPascalSnakeCase(this string original)
        {
            string normalized = Regex.Replace(original, "[^-_a-zA-Z0-9]", " ");

            normalized = Regex.Replace(normalized, "([a-z])([A-Z])", "$1 $2");
            normalized = Regex.Replace(normalized, "([0-9])([a-zA-Z])", "$1 $2");
            normalized = Regex.Replace(normalized, "([a-zA-Z])([0-9])", "$1 $2");
            normalized = normalized.Replace("-", " ").Replace("_", " ");

            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            string[] words = normalized
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()))
                .ToArray();

            return string.Join("_", words);
        }

        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Truncates a string to a specific character only if the string really needs to be truncated. Does not cause an exception.
        /// https://stackoverflow.com/a/2776689
        /// </summary>
        public static string SubstringSafe(this string value, int maxLength)
        {
            return value?.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        /// <summary>
        /// Removes all HTML tags from string.
        /// <para><see href="https://stackoverflow.com/a/18154046"/></para>
        /// </summary>
        public static string RemoveHTML(this string text)
        {
            return Regex.Replace(text, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Removing string between two strings.
        /// <para><see href="https://stackoverflow.com/q/51891661"/></para>
        /// </summary>
        public static string RemoveBetween(this string text, string startTag, string endTag)
        {
            Regex regex = new Regex(string.Format("{0}(.*?){1}", Regex.Escape(startTag), Regex.Escape(endTag)), RegexOptions.RightToLeft);
            string result = regex.Replace(text, startTag + endTag);
            return result;
        }

        /// <summary>
        /// Get part of string between two strings.
        /// <para><see href="https://stackoverflow.com/a/17252672"/></para>
        /// </summary>
        public static string GetBetween(this string text, string startTag, string endTag)
        {
            int pFrom = text.IndexOf(startTag) + startTag.Length;
            int pTo = text.LastIndexOf(endTag);
            string result = text.Substring(pFrom, pTo - pFrom);
            return result;
        }

        /// <summary>
        /// Simplified syntax for splitting string by string
        /// </summary>
        public static string[] SplitByString(this string text, string separator)
        {
            return text.Split(new string[] { separator }, StringSplitOptions.None);
        }
    }
}