using System.ComponentModel;

public class ElevModel : INotifyPropertyChanged
{
    private bool _areManual;
    public bool AreManual
    {
        get => _areManual;
        set
        {
            if (_areManual != value)
            {
                _areManual = value;
                OnPropertyChanged(nameof(AreManual));
            }
        }
    }

    public string NumeElev { get; set; }
    public string Initiale { get; set; }
    public string AvatarColor { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
