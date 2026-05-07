using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for CheckInBorrow.xaml
    /// </summary>
    /// 

    // Test load items into borrows container
    public class ActiveBorrow { 
        public string BookTitle { get; set; }
        public DateTime DueDate { get; set; }
    }

    public partial class CheckInBorrow : UserControl
    {
        public CheckInBorrow()
        {
            InitializeComponent();
            LoadStudentData();
        }

        private void LoadStudentData()
        {
            
            List<ActiveBorrow> loans = new List<ActiveBorrow>
            {
                new ActiveBorrow { BookTitle = "Morometii - Marin Preda", DueDate = DateTime.Now.AddDays(4) },
                new ActiveBorrow { BookTitle = "Culegere Matematică Bac", DueDate = DateTime.Now.AddDays(-1) }
            };
        }
        }
}
