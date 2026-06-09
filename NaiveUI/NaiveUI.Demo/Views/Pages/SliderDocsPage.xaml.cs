using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class SliderDocsPage : UserControl
{
    public SliderDocsPage()
    {
        InitializeComponent();
        DataContext = new SliderDocsPageViewModel();
    }

    private void HandleOutlineItemInvoked(object? sender, NMenuItemInvokedEventArgs e)
    {
        if (e.Value is not string targetName)
        {
            return;
        }

        if (FindName(targetName) is FrameworkElement target)
        {
            target.BringIntoView();
        }
    }

    private void HandleEventSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DataContext is SliderDocsPageViewModel viewModel)
        {
            viewModel.RecordValueChanged(e.OldValue, e.NewValue);
        }
    }

    private void HandleEventSliderDragStarted(object sender, RoutedEventArgs e)
    {
        if (DataContext is SliderDocsPageViewModel viewModel)
        {
            viewModel.RecordDragStarted();
        }
    }

    private void HandleEventSliderDragCompleted(object sender, RoutedEventArgs e)
    {
        if (DataContext is SliderDocsPageViewModel viewModel)
        {
            viewModel.RecordDragCompleted();
        }
    }

    private void HandleResetEventSliderClick(object sender, RoutedEventArgs e)
    {
        EventSlider.Reset();
    }

    private void HandleResetLinkedSliderClick(object sender, RoutedEventArgs e)
    {
        LinkedSlider.Reset();
    }
}
