using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minimum
{
    public class Text
    {
        public static bool IsNumeric(string source)
        {
            return Regex.Matches(source, "^[0-9]*$").Count > 0;
        }

        public static string Remove(string source, string regexPattern)
        {
            return Regex.Replace(source, regexPattern, "");            
        }

        public static string RemoveNonNumeric(string source)
        {
            return Regex.Replace(source, "[^0-9]", "");
        }

        public static string LimitNamesFrom(string name, int count)
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

        public static string LimitWordsFrom(string source, int count)
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

        public static string Replace(string source, string match, string replace)
        {
            int location = source.IndexOf(match);
            if (location < 0) { return source; }
            return source.Remove(location, match.Length).Insert(location, replace);
        }

        public static int Occurrences(string source, char match)
        {
            int count = 0;
            foreach (char s in source) { if (s == match) { count++; } }
            return count;
        }

        public static void Format(TextBox textBox, TextCompositionEventArgs e, string pattern)
        {
            if (e.Text.Length > 1) { throw new Exception("Não esperava por isso!"); }

            while(true)
            {
                if (textBox.Text.Length >= pattern.Length)
                {
                    e.Handled = true;
                    return;
                }
                else if (Regex.IsMatch(pattern[textBox.Text.Length].ToString(), "^[0-9]$")) // - Somente números
                {
                    if (Regex.IsMatch(e.Text, "^[0-9]$"))
                    {
                        e.Handled = false;
                        return;
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else if (Regex.IsMatch(pattern[textBox.Text.Length].ToString(), "^[a-zA-Z]$")) // - Somente caracteres
                {
                    if (Regex.IsMatch(e.Text, "^[a-zA-Z]$"))
                    {
                        e.Handled = false;
                        return;
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
                //else if (e.Text == pattern[e.Text.Length].ToString())
                //{
                //    e.Handled = false;
                //    return;
                //}
                else
                {
                    int index = textBox.CaretIndex;
                    textBox.Text = textBox.Text + pattern[textBox.Text.Length];
                    textBox.CaretIndex = index + 1;
                }
            }
        }

        public static void Format(TextBox textBox, DataObjectPastingEventArgs e, string pattern)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));

                for (int i = 0; i < text.Length; i++)
                {
                    while (true)
                    {
                        if (textBox.Text.Length >= pattern.Length)
                        {
                            break;
                        }
                        else if (Regex.IsMatch(pattern[textBox.Text.Length].ToString(), "^[0-9]$")) // - Somente números
                        {
                            if (Regex.IsMatch(text[i].ToString(), "^[0-9]$"))
                            {
                                int index = textBox.CaretIndex;
                                textBox.Text = textBox.Text + text[i];
                                textBox.CaretIndex = index + 1;
                            }
                            break;                            
                        }
                        else if (Regex.IsMatch(pattern[textBox.Text.Length].ToString(), "^[a-zA-Z]$")) // - Somente caracteres
                        {
                            if (Regex.IsMatch(text[i].ToString(), "^[a-zA-Z]$"))
                            {
                                int index = textBox.CaretIndex;
                                textBox.Text = textBox.Text + text[i];
                                textBox.CaretIndex = index + 1;
                            }
                            break;
                        }
                        //else if (text[i].ToString() == pattern[text[i].ToString().Length].ToString())
                        //{
                        //    int index = textBox.CaretIndex;
                        //    textBox.Text = textBox.Text + text[i];
                        //    textBox.CaretIndex = index + 1;

                        //    break;
                        //}
                        else
                        {
                            int index = textBox.CaretIndex;
                            textBox.Text = textBox.Text + pattern[textBox.Text.Length];
                            textBox.CaretIndex = index + 1;
                        }
                    }
                }
            }

            e.CancelCommand();
        }
    }
}