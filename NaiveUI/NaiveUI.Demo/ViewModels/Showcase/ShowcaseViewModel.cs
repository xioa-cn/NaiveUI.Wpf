using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NaiveUI.Demo.ViewModels.Showcase;

internal class ShowcaseViewModel : INotifyPropertyChanged
{
    private string searchText = "naive ui for wpf";
    private string email = "hello@naiveui.wpf";
    private string commandText = "dotnet add package NaiveUI.NControls";
    private bool notificationsEnabled = true;
    private bool animationEnabled = false;

    public ShowcaseViewModel()
    {
        AvatarSources = new ObservableCollection<string>
        {
            "/Assets/Header.jpg",
            "/Assets/Header.jpg",
            "/Assets/Header.jpg",
            "/Assets/Header.jpg"
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> AvatarSources { get; }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string CommandText
    {
        get => commandText;
        set => SetProperty(ref commandText, value);
    }

    public bool NotificationsEnabled
    {
        get => notificationsEnabled;
        set => SetProperty(ref notificationsEnabled, value);
    }

    public bool AnimationEnabled
    {
        get => animationEnabled;
        set => SetProperty(ref animationEnabled, value);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
