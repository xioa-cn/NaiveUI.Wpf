using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NaiveUI.Demo.ViewModels;

public sealed class ComponentSidebarItemViewModel : ViewModelBase
{
    private bool isSelected;
    
    public string Key { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public string? BadgeText { get; init; }

    public bool HasBadge => !string.IsNullOrWhiteSpace(BadgeText);

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}
