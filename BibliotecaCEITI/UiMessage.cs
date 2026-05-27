using System.Windows;
using System.Windows.Media;

namespace BibliotecaCEITI
{
    public class UiMessage
    {
        public string MessageText { get; set; }
        public bool IsUser { get; set; }
        public ImageSource GeneratedImage { get; set; }

        public Visibility ImageVisibility => GeneratedImage != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TextVisibility => !string.IsNullOrEmpty(MessageText) ? Visibility.Visible : Visibility.Collapsed;

        public HorizontalAlignment Alignment => IsUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        public CornerRadius BubbleRadius => IsUser ? new CornerRadius(15, 15, 0, 15) : new CornerRadius(15, 15, 15, 0);
        public Thickness BorderThickness => IsUser ? new Thickness(0) : new Thickness(1);

        public Brush BackgroundBrush => IsUser
            ? (Brush)Application.Current.TryFindResource("PrimaryEmerald")
            : (Brush)Application.Current.TryFindResource("ControlBackground");

        public Brush TextBrush => IsUser
            ? Brushes.White
            : (Brush)Application.Current.TryFindResource("TextPrimary");

        public Brush BorderBrush => IsUser
            ? Brushes.Transparent
            : (Brush)Application.Current.TryFindResource("BorderBrushLight");

        public UiMessage(string text, bool isUser)
        {
            MessageText = text;
            IsUser = isUser;
            GeneratedImage = null;
        }

        public UiMessage(ImageSource image, bool isUser)
        {
            MessageText = null;
            IsUser = isUser;
            GeneratedImage = image;
        }
    }
}