using System.Windows;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample;

public class N_Tag : System.Windows.Controls.Control {

    public CornerRadius CornerRadius {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<N_Tag, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(1));
    
    public Grade Grade {
        get { return (Grade)GetValue(GradeProperty); }
        set { SetValue(GradeProperty, value); }
    }

    public static readonly DependencyProperty GradeProperty =
        ElementBase.Property<N_Tag, Grade>(nameof(GradeProperty), Grade.Default);

    public string Text {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
        ElementBase.Property<N_Tag, string>(nameof(TextProperty), "N_Tag");
}