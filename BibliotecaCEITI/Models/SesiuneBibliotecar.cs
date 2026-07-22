using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaCEITI
{
    public class SesiuneBibliotecar
    {
        public static int IdBibliotecarCurent { get; set; } = 0;
        public static string NumeBibliotecar { get; set; } = string.Empty;
        public static string RolBibliotecar { get; set; } = string.Empty;
        public static string Email {  get; set; } = string.Empty;
        public static string Telefon {  get; set; } = string.Empty;
        public static DateTime ultimaAutentificare { get; set; } = DateTime.Now;
        public static DateTime ultimaModificare {  get; set; } = DateTime.Now;
        public static DateTime dataCreare {  get; set; } = DateTime.Now;
        public static byte[] PozaProfil { get; set; } = null;
        public static void CurataSesiune()
        {
            IdBibliotecarCurent = 0;
            NumeBibliotecar = string.Empty;
            RolBibliotecar = string.Empty;
            Email = string.Empty;
            Telefon = string.Empty;
            ultimaAutentificare = DateTime.Now;
            ultimaModificare = DateTime.Now;
            dataCreare = DateTime.Now;
            PozaProfil = null;
        }
    }
}
