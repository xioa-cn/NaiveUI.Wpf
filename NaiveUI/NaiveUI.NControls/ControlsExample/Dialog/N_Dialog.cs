using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NaiveUI.NControls.ControlsExample;


public partial class N_Dialog : ContentControl
{
    public static void Register(string token, FrameworkElement control)
    {
        if (string.IsNullOrEmpty(token) || control == null) return;
        ContainerDict[token] = control;
    }

    public static void Unregister(string token, FrameworkElement element)
    {
        if (string.IsNullOrEmpty(token) || element == null) return;

        if (ContainerDict.ContainsKey(token))
        {
            if (ReferenceEquals(ContainerDict[token], element))
            {
                ContainerDict.Remove(token);
            }
        }
    }

    public static void Unregister(FrameworkElement element)
    {
        if (element == null) return;
        var first = ContainerDict.FirstOrDefault(item => ReferenceEquals(element, item.Value));
        if (!string.IsNullOrEmpty(first.Key))
        {
            ContainerDict.Remove(first.Key);
        }
    }

    public static void Unregister(string token)
    {
        if (string.IsNullOrEmpty(token)) return;

        if (ContainerDict.ContainsKey(token))
        {
            ContainerDict.Remove(token);
        }
    }

    private AdornerContainer? _container;
    private string _token;
    private static readonly Dictionary<string, FrameworkElement> ContainerDict = new();
    private static readonly Dictionary<string, N_Drawer> DialogDict = new();
    public static async Task<N_Drawer> ShowAsync(string token, object contorl)
    {
        var drawer = new N_Drawer
        {
            _token = token,
            Content = contorl
        };

        var content = N_Dialog.GetDialogControl(token);

        if (content is null) return drawer;

        var decorator = VisualHelper.GetChild<DialogContainer>(content);

        if (decorator != null)
        {
            if (decorator.Child != null)
            {
                decorator.Child.IsEnabled = false;
            }

            var layer = decorator.AdornerLayer;
            if (layer != null)
            {
                var container = new AdornerContainer(layer)
                {
                    Child = drawer
                };
                drawer.overlayAdorner = new OverlayAdorner(layer);
                drawer._container = container;
                drawer.IsClosed = false;
                layer.Add(drawer.overlayAdorner);
                layer.Add(container);
            }
        }
        DialogDict[token] = drawer;
        return drawer;

    }
    private OverlayAdorner? overlayAdorner;
    private static DependencyObject? GetDialogControl(string token)
    {
        if (ContainerDict.TryGetValue(token, out var element))
        {
            return element;
        }
        return null;
    }

    public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
      nameof(IsClosed), typeof(bool), typeof(N_Drawer), new PropertyMetadata(ValueBoxes.FalseBox));

    public bool IsClosed
    {
        get => (bool)GetValue(IsClosedProperty);
        internal set => SetValue(IsClosedProperty, ValueBoxes.BooleanBox(value));
    }
    public static void Close(string token)
    {
        if (DialogDict.TryGetValue(token, out N_Drawer? dialog))
        {
            dialog?.Close();
        }
    }

    public void Close()
    {
        if (string.IsNullOrEmpty(_token))
        {
            Close(WindowHelper.GetActiveWindow());
        }
        else if (ContainerDict.TryGetValue(_token, out var element))
        {
            Close(element);
            DialogDict.Remove(_token);
        }
    }

    public async Task GetResultAsync()
    {
        await Task.Run(() =>
        {
            var wait = true;
            while (wait)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    if (this.IsClosed)
                    {
                        wait = false;
                    }
                });
                Thread.Sleep(500);
            }
        });
    }

    private void Close(DependencyObject? element)
    {
        if (element != null && _container != null)
        {
            var decorator = VisualHelper.GetChild<AdornerDecorator>(element);
            if (decorator != null)
            {
                if (decorator.Child != null)
                {
                    decorator.Child.IsEnabled = true;
                }
                var layer = decorator.AdornerLayer;
                layer?.Remove(_container);
                layer?.Remove(overlayAdorner);
                IsClosed = true;
            }
        }
    }

}
