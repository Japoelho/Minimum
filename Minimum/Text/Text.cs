using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Minimum
{
    public class Text
    {
        public static string Remove(string source, string regexPattern)
        {
            return Regex.Replace(source, regexPattern, "");            
        }

        public static string NumericOnly(string source)
        {
            return Regex.Replace(source, "[^0-9]", "");
        }

        public static string PickCountNames(string name, int count)
        {
            IList<string> names = new List<string>();

            string[] n = name.Split(' ');
            string t = null;
            for (int i = 0; i < n.Length; i++)
            {
                t += n[i];
                string s = n[i].ToLower();
                if (s == "da" || s == "de" || s == "do")
                {
                    t = n[i] + " ";
                }
                else
                {
                    names.Add(t.Trim());
                    t = null;
                }
                //if (n[i].Length > 2 || n[i].IndexOf('.') > -1)
            }

            if (count > names.Count) { return name; }

            string result = null;
            for (int i = 0; i < (count + 1) / 2; i++)
            {
                result += result == null ? names[i] : " " + names[i];
            }

            for (int i = names.Count - (count / 2); i < names.Count; i++)
            {
                result += result == null ? names[i] : " " + names[i];
            }

            return result;
        }

        public static string PickCountString(string source, int count)
        {
            string[] names = source.Split(' ');

            if (count > names.Length) { return source; }

            string result = null;
            for (int i = 0; i < (count + 1) / 2; i++)
            {
                result += result == null ? names[i].Trim() : " " + names[i].Trim();
            }

            for (int i = names.Length - (count / 2); i < names.Length; i++)
            {
                result += result == null ? names[i].Trim() : " " + names[i].Trim();
            }

            return result;
        }

        public static string Capitalize(string source)
        {
            string result = null;

            string[] s = source.ToLower().Split(' ');
            for (int i = 0; i < s.Length; i++)
            {
                s[i] = Char.ToUpper(s[i][0]) + s[i].Substring(1);

                result += result == null ? s[i] : " " + s[i];
            }

            return result;
        }
    }
}
