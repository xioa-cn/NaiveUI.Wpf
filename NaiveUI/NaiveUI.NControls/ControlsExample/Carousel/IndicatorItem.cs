using System.ComponentModel;
using System.Windows.Input;

namespace NaiveUI.NControls.ControlsExample;

public class IndicatorItem : INotifyPropertyChanged {
    public int Index { get; set; }

    private bool _isSelected;

    public bool IsSelected {
        get { return _isSelected; }
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    
    public ICommand SelectSlideCommand { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}