using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NCardSize
{
    Small,
    Medium,
    Large
}

[TemplatePart(Name = RootBorderPartName, Type = typeof(Border))]
public class NCard : ContentControl
{
    private const string RootBorderPartName = "PART_RootBorder";

    private Border? rootBorder;
    private DropShadowEffect? shadowEffect;
    private TranslateTransform? rootTransform;
    private bool hoverHandlersAttached;

    static NCard()
    {
        ElementBase.DefaultStyle<NCard>(DefaultStyleKeyProperty);
    }

    public NCard()
    {
        Loaded += (_, _) => UpdateHoverVisualState();
        SizeChanged += (_, _) => UpdateResolvedMetrics();
        UpdateResolvedMetrics();
        UpdateSectionState();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        ElementBase.Property<NCard, string>(nameof(TitleProperty), string.Empty, OnCardStructurePropertyChanged);

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        ElementBase.Property<NCard, object?>(nameof(HeaderProperty), null, OnCardStructurePropertyChanged);

    public object? HeaderExtra
    {
        get => GetValue(HeaderExtraProperty);
        set => SetValue(HeaderExtraProperty, value);
    }

    public static readonly DependencyProperty HeaderExtraProperty =
        ElementBase.Property<NCard, object?>(nameof(HeaderExtraProperty), null, OnCardStructurePropertyChanged);

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        ElementBase.Property<NCard, object?>(nameof(FooterProperty), null, OnCardStructurePropertyChanged);

    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    public static readonly DependencyProperty ActionProperty =
        ElementBase.Property<NCard, object?>(nameof(ActionProperty), null, OnCardStructurePropertyChanged);

    public object? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    public static readonly DependencyProperty CoverProperty =
        ElementBase.Property<NCard, object?>(nameof(CoverProperty), null, OnCardStructurePropertyChanged);

    public bool Bordered
    {
        get => (bool)GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public static readonly DependencyProperty BorderedProperty =
        ElementBase.Property<NCard, bool>(nameof(BorderedProperty), true);

    public bool Hoverable
    {
        get => (bool)GetValue(HoverableProperty);
        set => SetValue(HoverableProperty, value);
    }

    public static readonly DependencyProperty HoverableProperty =
        ElementBase.Property<NCard, bool>(nameof(HoverableProperty), false, OnCardHoverablePropertyChanged);

    public bool Embedded
    {
        get => (bool)GetValue(EmbeddedProperty);
        set => SetValue(EmbeddedProperty, value);
    }

    public static readonly DependencyProperty EmbeddedProperty =
        ElementBase.Property<NCard, bool>(nameof(EmbeddedProperty), false);

    public bool Segmented
    {
        get => (bool)GetValue(SegmentedProperty);
        set => SetValue(SegmentedProperty, value);
    }

    public static readonly DependencyProperty SegmentedProperty =
        ElementBase.Property<NCard, bool>(nameof(SegmentedProperty), false);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        ElementBase.Property<NCard, bool>(nameof(IsLoadingProperty), false);

    public NCardSize Size
    {
        get => (NCardSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NCard, NCardSize>(nameof(SizeProperty), NCardSize.Medium, OnCardLayoutPropertyChanged);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NCard, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(3));

    public Thickness ResolvedHeaderPadding
    {
        get => (Thickness)GetValue(ResolvedHeaderPaddingProperty);
        private set => SetValue(ResolvedHeaderPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedHeaderPaddingProperty =
        ElementBase.Property<NCard, Thickness>(nameof(ResolvedHeaderPaddingProperty), new Thickness(24, 18, 24, 0));

    public Thickness ResolvedContentPadding
    {
        get => (Thickness)GetValue(ResolvedContentPaddingProperty);
        private set => SetValue(ResolvedContentPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentPaddingProperty =
        ElementBase.Property<NCard, Thickness>(nameof(ResolvedContentPaddingProperty), new Thickness(24));

    public Thickness ResolvedFooterPadding
    {
        get => (Thickness)GetValue(ResolvedFooterPaddingProperty);
        private set => SetValue(ResolvedFooterPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedFooterPaddingProperty =
        ElementBase.Property<NCard, Thickness>(nameof(ResolvedFooterPaddingProperty), new Thickness(24, 16, 24, 18));

    public Thickness ResolvedActionPadding
    {
        get => (Thickness)GetValue(ResolvedActionPaddingProperty);
        private set => SetValue(ResolvedActionPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedActionPaddingProperty =
        ElementBase.Property<NCard, Thickness>(nameof(ResolvedActionPaddingProperty), new Thickness(24, 12, 24, 12));

    public double ResolvedTitleFontSize
    {
        get => (double)GetValue(ResolvedTitleFontSizeProperty);
        private set => SetValue(ResolvedTitleFontSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedTitleFontSizeProperty =
        ElementBase.Property<NCard, double>(nameof(ResolvedTitleFontSizeProperty), 18d);

    public bool HasHeader
    {
        get => (bool)GetValue(HasHeaderProperty);
        private set => SetValue(HasHeaderProperty, value);
    }

    public static readonly DependencyProperty HasHeaderProperty =
        ElementBase.Property<NCard, bool>(nameof(HasHeaderProperty), false);

    public bool HasCustomHeader
    {
        get => (bool)GetValue(HasCustomHeaderProperty);
        private set => SetValue(HasCustomHeaderProperty, value);
    }

    public static readonly DependencyProperty HasCustomHeaderProperty =
        ElementBase.Property<NCard, bool>(nameof(HasCustomHeaderProperty), false);

    public bool HasTitle
    {
        get => (bool)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    public static readonly DependencyProperty HasTitleProperty =
        ElementBase.Property<NCard, bool>(nameof(HasTitleProperty), false);

    public bool HasHeaderExtra
    {
        get => (bool)GetValue(HasHeaderExtraProperty);
        private set => SetValue(HasHeaderExtraProperty, value);
    }

    public static readonly DependencyProperty HasHeaderExtraProperty =
        ElementBase.Property<NCard, bool>(nameof(HasHeaderExtraProperty), false);

    public bool HasFooter
    {
        get => (bool)GetValue(HasFooterProperty);
        private set => SetValue(HasFooterProperty, value);
    }

    public static readonly DependencyProperty HasFooterProperty =
        ElementBase.Property<NCard, bool>(nameof(HasFooterProperty), false);

    public bool HasAction
    {
        get => (bool)GetValue(HasActionProperty);
        private set => SetValue(HasActionProperty, value);
    }

    public static readonly DependencyProperty HasActionProperty =
        ElementBase.Property<NCard, bool>(nameof(HasActionProperty), false);

    public bool HasCover
    {
        get => (bool)GetValue(HasCoverProperty);
        private set => SetValue(HasCoverProperty, value);
    }

    public static readonly DependencyProperty HasCoverProperty =
        ElementBase.Property<NCard, bool>(nameof(HasCoverProperty), false);

    public override void OnApplyTemplate()
    {
        DetachHoverHandlers();

        base.OnApplyTemplate();

        rootBorder = GetTemplateChild(RootBorderPartName) as Border;
        ApplyHoverInfrastructure();
        UpdateHoverVisualState();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateSectionState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == BackgroundProperty || e.Property == BorderBrushProperty || e.Property == EmbeddedProperty || e.Property == BorderedProperty)
        {
            UpdateHoverVisualState();
        }
    }

    private static void OnCardLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NCard card)
        {
            card.UpdateResolvedMetrics();
        }
    }

    private static void OnCardStructurePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NCard card)
        {
            card.UpdateSectionState();
        }
    }

    private static void OnCardHoverablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NCard card)
        {
            card.ApplyHoverInfrastructure();
            card.UpdateHoverVisualState();
        }
    }

    private void UpdateResolvedMetrics()
    {
        switch (Size)
        {
            case NCardSize.Small:
                ResolvedHeaderPadding = new Thickness(16, 14, 16, 0);
                ResolvedContentPadding = new Thickness(16);
                ResolvedFooterPadding = new Thickness(16, 12, 16, 14);
                ResolvedActionPadding = new Thickness(16, 10, 16, 10);
                ResolvedTitleFontSize = 16d;
                break;
            case NCardSize.Large:
                ResolvedHeaderPadding = new Thickness(28, 22, 28, 0);
                ResolvedContentPadding = new Thickness(28, 24, 28, 24);
                ResolvedFooterPadding = new Thickness(28, 18, 28, 22);
                ResolvedActionPadding = new Thickness(28, 14, 28, 14);
                ResolvedTitleFontSize = 20d;
                break;
            default:
                ResolvedHeaderPadding = new Thickness(24, 18, 24, 0);
                ResolvedContentPadding = new Thickness(24);
                ResolvedFooterPadding = new Thickness(24, 16, 24, 18);
                ResolvedActionPadding = new Thickness(24, 12, 24, 12);
                ResolvedTitleFontSize = 18d;
                break;
        }
    }

    private void UpdateSectionState()
    {
        HasCustomHeader = IsMeaningfulContent(Header);
        HasTitle = !HasCustomHeader && !string.IsNullOrWhiteSpace(Title);
        HasHeaderExtra = IsMeaningfulContent(HeaderExtra);
        HasHeader = HasCustomHeader || HasTitle || HasHeaderExtra;
        HasFooter = IsMeaningfulContent(Footer);
        HasAction = IsMeaningfulContent(Action);
        HasCover = IsMeaningfulContent(Cover);
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

    private void ApplyHoverInfrastructure()
    {
        if (rootBorder is null)
        {
            return;
        }

        if (!Hoverable)
        {
            RemoveHoverInfrastructure();
            return;
        }

        if (hoverHandlersAttached)
        {
            return;
        }

        shadowEffect = new DropShadowEffect
        {
            BlurRadius = 18,
            Direction = 270,
            ShadowDepth = 4,
            Color = Color.FromArgb(28, 15, 23, 42),
            Opacity = 0
        };
        rootBorder.Effect = shadowEffect;

        rootTransform = new TranslateTransform();
        rootBorder.RenderTransform = rootTransform;
        rootBorder.RenderTransformOrigin = new Point(0.5, 0.5);

        rootBorder.MouseEnter += HandleBorderMouseEnter;
        rootBorder.MouseLeave += HandleBorderMouseLeave;
        hoverHandlersAttached = true;
    }

    private void RemoveHoverInfrastructure()
    {
        if (rootBorder is null)
        {
            return;
        }

        rootBorder.Effect = null;
        rootBorder.RenderTransform = null;
        DetachHoverHandlers();
        shadowEffect = null;
        rootTransform = null;
    }

    private void DetachHoverHandlers()
    {
        if (rootBorder is null || !hoverHandlersAttached)
        {
            return;
        }

        rootBorder.MouseEnter -= HandleBorderMouseEnter;
        rootBorder.MouseLeave -= HandleBorderMouseLeave;
        hoverHandlersAttached = false;
    }

    private void HandleBorderMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AnimateHoverState(true);
    }

    private void HandleBorderMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AnimateHoverState(false);
    }

    private void UpdateHoverVisualState()
    {
        if (!Hoverable || rootBorder is null)
        {
            return;
        }

        rootBorder.BorderBrush = BorderBrush;

        if (IsMouseOver)
        {
            AnimateHoverState(true);
        }
        else
        {
            AnimateHoverState(false);
        }
    }

    private void AnimateHoverState(bool isHovered)
    {
        if (!Hoverable || rootBorder is null || shadowEffect is null || rootTransform is null)
        {
            return;
        }

        if (isHovered)
        {
            if (Application.Current?.TryFindResource("Theme.Border.Strong.Brush") is Brush strongBorderBrush)
            {
                rootBorder.BorderBrush = strongBorderBrush;
            }
        }
        else
        {
            rootBorder.BorderBrush = BorderBrush;
        }

        shadowEffect.BeginAnimation(
            DropShadowEffect.OpacityProperty,
            new DoubleAnimation
            {
                To = isHovered ? 1d : 0d,
                Duration = TimeSpan.FromMilliseconds(180)
            });

        rootTransform.BeginAnimation(
            TranslateTransform.YProperty,
            new DoubleAnimation
            {
                To = isHovered ? -2d : 0d,
                Duration = TimeSpan.FromMilliseconds(180)
            });
    }
}
