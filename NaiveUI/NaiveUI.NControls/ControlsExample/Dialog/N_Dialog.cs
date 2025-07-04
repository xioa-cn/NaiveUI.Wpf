﻿using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NaiveUI.NControls.ControlsExample;


public partial class N_Dialog : ContentControl
{
    public static void Register(string token, FrameworkElement? control)
    {
        if (string.IsNullOrEmpty(token) || control == null) return;
        ContainerDict[token] = control;
    }

    public static void Unregister(string token, FrameworkElement? element)
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

    public static void Unregister(FrameworkElement? element)
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
    public static Task<N_Drawer> ShowAsync(string token, object contorl, bool clickClose = false)
    {
        var drawer = new N_Drawer
        {
            _token = token,
            Content = contorl
        };

        var content = N_Dialog.GetDialogControl(token);

        if (content is null) return Task.FromResult(drawer);

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
                drawer._overlayAdorner = new OverlayAdorner(layer);

                if (clickClose)
                {
                    drawer._overlayAdorner.MouseLeftAction += () =>
                    {
                        N_Dialog.Close(token);
                    };
                }

                drawer._container = container;
                drawer.IsClosed = false;
                layer.Add(drawer._overlayAdorner);
                layer.Add(container);
            }
        }
        DialogDict[token] = drawer;
        return Task.FromResult(drawer);

    }
    private OverlayAdorner? _overlayAdorner;
    private static DependencyObject? GetDialogControl(string token) {
        return ContainerDict.GetValueOrDefault(token);
    }




    public static readonly DependencyProperty IsClosedProperty =
        ElementBase.Property<N_Dialog, bool>(nameof(IsClosedProperty), false);

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

    private void Close(DependencyObject? element) {
        if (element == null || _container == null) return;
        var decorator = VisualHelper.GetChild<AdornerDecorator>(element);
        if (decorator == null) return;
        if (decorator.Child != null)
        {
            decorator.Child.IsEnabled = true;
        }
        var layer = decorator.AdornerLayer;
        layer?.Remove(_container);
        if (_overlayAdorner != null) layer?.Remove(_overlayAdorner);
        IsClosed = true;
    }

}
