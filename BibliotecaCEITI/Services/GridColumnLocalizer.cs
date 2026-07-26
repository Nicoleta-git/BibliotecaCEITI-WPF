using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BibliotecaCEITI
{
    public static class GridColumnLocalizer
    {
        private static readonly Dictionary<string, string> HeaderKeys = new()
        {
            { "Titlu", "Books_ColTitle" },
            { "Autor", "Books_ColAuthor" },
            { "Stare", "Books_ColStatus" },
            { "Elev", "Students_ColName" },
            { "Telefon", "Students_ColPhone" },
            { "Grupa", "Students_ColGroup" },
            { "Preț", "Grid_Price" },
            { "Chirie", "Grid_Rent" },
            { "Carte", "Grid_Book" },
            { "Termen", "Grid_DueDate" },
            { "Data_împrumut", "Grid_LoanDate" },
            { "Data împrumut", "Grid_LoanDate" },
            { "Data_returnare", "Grid_ReturnDate" },
            { "Data returnare", "Grid_ReturnDate" },
        };

        public static void Localize(DataGridAutoGeneratingColumnEventArgs e)
        {
            string original = e.Column.Header?.ToString() ?? "";
            if (HeaderKeys.TryGetValue(original, out string key))
            {
                e.Column.Header = Application.Current.FindResource(key);
            }
        }
    }
}
