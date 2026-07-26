using System;
using System.Windows;

namespace BibliotecaCEITI
{
    public static class LanguageService
    {
        public static string CurrentLanguage { get; private set; } = "en";

        public static void SetLanguage(string languageCode)
        {
            string path = languageCode switch
            {
                "ro" => "Languages/Strings.ro.xaml",
                "ru" => "Languages/Strings.ru.xaml",
                _ => "Languages/Strings.en.xaml"
            };

            var newDict = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            for (int i = 0; i < dictionaries.Count; i++)
            {
                string sourceStr = dictionaries[i].Source?.OriginalString ?? "";
                if (sourceStr.Contains("Strings."))
                {
                    dictionaries[i] = newDict;
                    CurrentLanguage = languageCode;
                    return;
                }
            }

            dictionaries.Add(newDict);
            CurrentLanguage = languageCode;
        }
    }
}
