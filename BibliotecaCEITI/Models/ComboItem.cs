using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaCEITI
{
    public class ComboItem
    {
        public int Id { get; set; }
        public string Denumire { get; set; }
        public string Cod { get; set; }
        public override string ToString() => Denumire;
    }
}
