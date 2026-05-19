using System;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NDividerTitlePlacement
{
    Left,
    Center,
    Right
}

public class NDivider : ContentControl
{
    static NDivider()
    {
        ElementBase.DefaultStyle<NDivider>(DefaultStyleKeyProperty);
    }

    public NDivider()
    {
        UpdateTitleState();
        UpdateResolvedVerticalHeight();
        UpdateResolvedLineThickness();
    }

    public bool Dashed
    {
        get => (bool)GetValue(DashedProperty);
        set => SetValue(DashedProperty, value);
    }

    public static readonly DependencyProperty DashedProperty =
        ElementBase.Property<NDivider, bool>(nameof(DashedProperty), false);

    public bool Vertical
    {
        get => (bool)GetValue(VerticalProperty);
        set => SetValue(VerticalProperty, value);
    }

    public static readonly DependencyProperty VerticalProperty =
        ElementBase.Property<NDivider, bool>(nameof(VerticalProperty), false);

    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public static readonly DependencyProperty LineThicknessProperty =
        ElementBase.Property<NDivider, double>(nameof(LineThicknessProperty), 1d);

    public NDividerTitlePlacement TitlePlacement
    {
        get => (NDividerTitlePlacement)GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    public static readonly DependencyProperty TitlePlacementProperty =
        ElementBase.Property<NDivider, NDividerTitlePlacement>(nameof(TitlePlacementProperty), NDividerTitlePlacement.Center);

    public bool HasTitle
    {
        get => (bool)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    public static readonly DependencyProperty HasTitleProperty =
        ElementBase.Property<NDivider, bool>(nameof(HasTitleProperty), false);

    public object? ResolvedTitleContent
    {
        get => GetValue(ResolvedTitleContentProperty);
        private set => SetValue(ResolvedTitleContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedTitleContentProperty =
        ElementBase.Property<NDivider, object?>(nameof(ResolvedTitleContentProperty), null);

    public double ResolvedVerticalHeight
    {
        get => (double)GetValue(ResolvedVerticalHeightProperty);
        private set => SetValue(ResolvedVerticalHeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedVerticalHeightProperty =
        ElementBase.Property<NDivider, double>(nameof(ResolvedVerticalHeightProperty), 16d);

    public double ResolvedLineThickness
    {
        get => (double)GetValue(ResolvedLineThicknessProperty);
        private set => SetValue(ResolvedLineThicknessProperty, value);
    }

    public static readonly DependencyProperty ResolvedLineThicknessProperty =
        ElementBase.Property<NDivider, double>(nameof(ResolvedLineThicknessProperty), 1d);

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateTitleState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == HeightProperty || e.Property == FontSizeProperty)
        {
            UpdateResolvedVerticalHeight();
        }

        if (e.Property == LineThicknessProperty)
        {
            UpdateResolvedLineThickness();
        }
    }

    private void UpdateTitleState()
    {
        ResolvedTitleContent = IsMeaningfulContent(Content) ? Content : null;
        HasTitle = ResolvedTitleContent is not null;
    }

    private void UpdateResolvedVerticalHeight()
    {
        var explicitHeight = Height;
        ResolvedVerticalHeight = !double.IsNaN(explicitHeight) && explicitHeight > 0d
            ? explicitHeight
            : Math.Max(1d, FontSize);
    }

    private void UpdateResolvedLineThickness()
    {
        ResolvedLineThickness = !double.IsNaN(LineThickness) && LineThickness > 0d
            ? LineThickness
            : 1d;
    }

    private static bool IsMeaningfulContent(object? value)
    {
        return value switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }
}
