using System.ComponentModel;

public class ElevModel : INotifyPropertyChanged
{
    private bool _areManual;

    public bool AreManual
    {
        get { return _areManual; }
        set
        {
            if (_areManual != value)
            {
                _areManual = value;
            }
        }
    }

    public int Id { get; set; }
    public string NumeElev { get; set; }
    public string Initiale { get; set; }
    public string AvatarColor { get; set; }
 
    public string Telefon { get; set; }
    public string Email { get; set; }
    public string Grupa { get; set; }
 
    public ElevModel(int id, string numeElev, string initiale, string avatarColor, bool areManual)
    {
        Id = id;
        NumeElev = numeElev;
        Initiale = initiale;
        AvatarColor = avatarColor;
        AreManual = areManual;
    }

    public ElevModel(int id, string numeElev, string initiale, string avatarColor, string telefon, string email, string grupa)
    {
        Id = id;
        NumeElev = numeElev;
        Initiale = initiale;
        AvatarColor = avatarColor;
        Telefon = telefon;
        Email = email;
        Grupa = grupa;
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
