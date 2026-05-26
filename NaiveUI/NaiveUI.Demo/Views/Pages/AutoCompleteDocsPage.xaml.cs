using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class AutoCompleteDocsPage : UserControl
{
    private CancellationTokenSource? remoteSearchCts;

    public AutoCompleteDocsPage()
    {
        InitializeComponent();
        DataContext = new AutoCompleteDocsPageViewModel();
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

    private void HandleValueChanged(object sender, NAutoCompleteValueChangedEventArgs e)
    {
        if (DataContext is AutoCompleteDocsPageViewModel viewModel)
        {
            var isSuggestionDrivenDemo =
                ReferenceEquals(sender, BasicAutoComplete)
                || ReferenceEquals(sender, CompletionAutoComplete)
                || ReferenceEquals(sender, PrefixAutoComplete)
                || ReferenceEquals(sender, TemplateAutoComplete);

            if (ReferenceEquals(sender, BasicAutoComplete))
            {
                viewModel.UpdateBasicOptions(e.NewText);
            }
            else if (ReferenceEquals(sender, CompletionAutoComplete))
            {
                viewModel.UpdateCompletionOptions(e.NewText);
            }
            else if (ReferenceEquals(sender, PrefixAutoComplete))
            {
                viewModel.UpdatePrefixOptions(e.NewText);
            }
            else if (ReferenceEquals(sender, TemplateAutoComplete))
            {
                viewModel.UpdateTemplateOptions(e.NewText);
            }

            if (!isSuggestionDrivenDemo || !Equals(e.OldValue, e.NewValue))
            {
                viewModel.RecordValueChanged(e.OldText, e.NewText, e.OldValue, e.NewValue);
            }
        }
    }

    private void HandleOptionSelected(object sender, NAutoCompleteOptionSelectedEventArgs e)
    {
        if (DataContext is AutoCompleteDocsPageViewModel viewModel)
        {
            var label = e.Option.Label?.ToString() ?? string.Empty;
            viewModel.RecordOptionSelected(label, e.Option.Value);
        }
    }

    private async void HandleRemoteValueChanged(object sender, NAutoCompleteValueChangedEventArgs e)
    {
        if (DataContext is not AutoCompleteDocsPageViewModel viewModel)
        {
            return;
        }

        remoteSearchCts?.Cancel();
        remoteSearchCts?.Dispose();
        remoteSearchCts = new CancellationTokenSource();
        var token = remoteSearchCts.Token;

        viewModel.RemoteLoading = true;

        try
        {
            await Task.Delay(280, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            viewModel.ApplyRemoteResults(e.NewText);
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                viewModel.RemoteLoading = false;
            }
        }
    }

    private void HandleFocusClick(object sender, RoutedEventArgs e)
    {
        DemoFocusAutoComplete.Focus();
        if (DataContext is AutoCompleteDocsPageViewModel viewModel)
        {
            viewModel.RecordFocusAction("Focus()");
        }
    }

    private void HandleBlurClick(object sender, RoutedEventArgs e)
    {
        DemoFocusAutoComplete.Blur();
        if (DataContext is AutoCompleteDocsPageViewModel viewModel)
        {
            viewModel.RecordFocusAction("Blur()");
        }
    }
}
