namespace BibliotecaCEITI.Models
{
    public class AttentionItemViewModel
    {
        public int IdImprumut { get; set; }
        public int IdElev { get; set; }
        public int IdExemplar { get; set; }
        public string NumeElev { get; set; } = "";
        public string Clasa { get; set; } = "";
        public string Initiale { get; set; } = "";
        public string TitluCarte { get; set; } = "";
        public string AutorCarte { get; set; } = "";
        public DateTime TermenReturnare { get; set; }
        public int ZileIntarziere { get; set; }
        public string Email { get; set; } = "";
        public string Stare { get; set; } = "activ";
        public bool Selectat { get; set; } = true;

        public bool EsteGrav => ZileIntarziere >= 5;
        public string TermenReturnareText => TermenReturnare.ToString("dd.MM.yyyy");
        public string ZileIntarziereText => $"{ZileIntarziere} zile";
    }
}
