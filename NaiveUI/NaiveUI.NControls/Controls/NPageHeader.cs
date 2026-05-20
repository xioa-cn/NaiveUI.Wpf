using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NPageHeader : ContentControl
{
    private const string BackButtonPartName = "PART_BackButton";
    private static readonly Geometry DefaultBackGeometry = Geometry.Parse("M10.828 12l4.95 4.95-1.414 1.414L8 12l6.364-6.364 1.414 1.414z");

    private ButtonBase? backButton;
    private int backHandlerCount;

    static NPageHeader()
    {
        ElementBase.DefaultStyle<NPageHeader>(DefaultStyleKeyProperty);
    }

    public NPageHeader()
    {
        UpdateResolvedState();
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        ElementBase.Property<NPageHeader, string?>(nameof(TitleProperty), null, OnPageHeaderPropertyChanged);

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly DependencyProperty SubtitleProperty =
        ElementBase.Property<NPageHeader, string?>(nameof(SubtitleProperty), null, OnPageHeaderPropertyChanged);

    public string? Extra
    {
        get => (string?)GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public static readonly DependencyProperty ExtraProperty =
        ElementBase.Property<NPageHeader, string?>(nameof(ExtraProperty), null, OnPageHeaderPropertyChanged);

    public ICommand? OnBack
    {
        get => (ICommand?)GetValue(OnBackProperty);
        set => SetValue(OnBackProperty, value);
    }

    public static readonly DependencyProperty OnBackProperty =
        ElementBase.Property<NPageHeader, ICommand?>(nameof(OnBackProperty), null, OnPageHeaderPropertyChanged);

    public object? BackCommandParameter
    {
        get => GetValue(BackCommandParameterProperty);
        set => SetValue(BackCommandParameterProperty, value);
    }

    public static readonly DependencyProperty BackCommandParameterProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(BackCommandParameterProperty), null);

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public static readonly DependencyProperty HeaderContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(HeaderContentProperty), null, OnPageHeaderPropertyChanged);

    public object? AvatarContent
    {
        get => GetValue(AvatarContentProperty);
        set => SetValue(AvatarContentProperty, value);
    }

    public static readonly DependencyProperty AvatarContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(AvatarContentProperty), null, OnPageHeaderPropertyChanged);

    public object? ExtraContent
    {
        get => GetValue(ExtraContentProperty);
        set => SetValue(ExtraContentProperty, value);
    }

    public static readonly DependencyProperty ExtraContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(ExtraContentProperty), null, OnPageHeaderPropertyChanged);

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public static readonly DependencyProperty FooterContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(FooterContentProperty), null, OnPageHeaderPropertyChanged);

    public object? TitleContent
    {
        get => GetValue(TitleContentProperty);
        set => SetValue(TitleContentProperty, value);
    }

    public static readonly DependencyProperty TitleContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(TitleContentProperty), null, OnPageHeaderPropertyChanged);

    public object? SubtitleContent
    {
        get => GetValue(SubtitleContentProperty);
        set => SetValue(SubtitleContentProperty, value);
    }

    public static readonly DependencyProperty SubtitleContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(SubtitleContentProperty), null, OnPageHeaderPropertyChanged);

    public object? BackContent
    {
        get => GetValue(BackContentProperty);
        set => SetValue(BackContentProperty, value);
    }

    public static readonly DependencyProperty BackContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(BackContentProperty), null, OnPageHeaderPropertyChanged);

    public object? ResolvedTitleContent
    {
        get => GetValue(ResolvedTitleContentProperty);
        private set => SetValue(ResolvedTitleContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedTitleContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(ResolvedTitleContentProperty), null);

    public object? ResolvedSubtitleContent
    {
        get => GetValue(ResolvedSubtitleContentProperty);
        private set => SetValue(ResolvedSubtitleContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedSubtitleContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(ResolvedSubtitleContentProperty), null);

    public object? ResolvedExtraContent
    {
        get => GetValue(ResolvedExtraContentProperty);
        private set => SetValue(ResolvedExtraContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedExtraContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(ResolvedExtraContentProperty), null);

    public object? ResolvedBackContent
    {
        get => GetValue(ResolvedBackContentProperty);
        private set => SetValue(ResolvedBackContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedBackContentProperty =
        ElementBase.Property<NPageHeader, object?>(nameof(ResolvedBackContentProperty), null);

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        private set => SetValue(ShowBackProperty, value);
    }

    public static readonly DependencyProperty ShowBackProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(ShowBackProperty), false);

    public bool HasHeaderSection
    {
        get => (bool)GetValue(HasHeaderSectionProperty);
        private set => SetValue(HasHeaderSectionProperty, value);
    }

    public static readonly DependencyProperty HasHeaderSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasHeaderSectionProperty), false);

    public bool HasAvatarSection
    {
        get => (bool)GetValue(HasAvatarSectionProperty);
        private set => SetValue(HasAvatarSectionProperty, value);
    }

    public static readonly DependencyProperty HasAvatarSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasAvatarSectionProperty), false);

    public bool HasTitleSection
    {
        get => (bool)GetValue(HasTitleSectionProperty);
        private set => SetValue(HasTitleSectionProperty, value);
    }

    public static readonly DependencyProperty HasTitleSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasTitleSectionProperty), false);

    public bool HasSubtitleSection
    {
        get => (bool)GetValue(HasSubtitleSectionProperty);
        private set => SetValue(HasSubtitleSectionProperty, value);
    }

    public static readonly DependencyProperty HasSubtitleSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasSubtitleSectionProperty), false);

    public bool HasExtraSection
    {
        get => (bool)GetValue(HasExtraSectionProperty);
        private set => SetValue(HasExtraSectionProperty, value);
    }

    public static readonly DependencyProperty HasExtraSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasExtraSectionProperty), false);

    public bool HasMainSection
    {
        get => (bool)GetValue(HasMainSectionProperty);
        private set => SetValue(HasMainSectionProperty, value);
    }

    public static readonly DependencyProperty HasMainSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasMainSectionProperty), false);

    public bool HasDefaultSection
    {
        get => (bool)GetValue(HasDefaultSectionProperty);
        private set => SetValue(HasDefaultSectionProperty, value);
    }

    public static readonly DependencyProperty HasDefaultSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasDefaultSectionProperty), false);

    public bool HasFooterSection
    {
        get => (bool)GetValue(HasFooterSectionProperty);
        private set => SetValue(HasFooterSectionProperty, value);
    }

    public static readonly DependencyProperty HasFooterSectionProperty =
        ElementBase.Property<NPageHeader, bool>(nameof(HasFooterSectionProperty), false);

    public static readonly RoutedEvent BackEvent =
        ElementBase.RoutedEvent<NPageHeader, RoutedEventHandler>(nameof(BackEvent));

    public event RoutedEventHandler Back
    {
        add
        {
            AddHandler(BackEvent, value);
            backHandlerCount++;
            UpdateResolvedState();
        }
        remove
        {
            RemoveHandler(BackEvent, value);
            backHandlerCount = Math.Max(0, backHandlerCount - 1);
            UpdateResolvedState();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (backButton is not null)
        {
            backButton.Click -= HandleBackButtonClick;
        }

        backButton = GetTemplateChild(BackButtonPartName) as ButtonBase;
        if (backButton is not null)
        {
            backButton.Click += HandleBackButtonClick;
        }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateResolvedState();
    }

    private static void OnPageHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NPageHeader pageHeader)
        {
            pageHeader.UpdateResolvedState();
        }
    }

    private void HandleBackButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(BackEvent, this));
    }

    private void UpdateResolvedState()
    {
        ResolvedTitleContent = ResolvePreferredContent(TitleContent, Title);
        ResolvedSubtitleContent = ResolvePreferredContent(SubtitleContent, Subtitle);
        ResolvedExtraContent = ResolvePreferredContent(ExtraContent, Extra);
        ShowBack = OnBack is not null || backHandlerCount > 0;
        ResolvedBackContent = BackContent ?? CreateDefaultBackContent();

        HasHeaderSection = HasMeaningfulContent(HeaderContent);
        HasAvatarSection = HasMeaningfulContent(AvatarContent);
        HasTitleSection = HasMeaningfulContent(ResolvedTitleContent);
        HasSubtitleSection = HasMeaningfulContent(ResolvedSubtitleContent);
        HasExtraSection = HasMeaningfulContent(ResolvedExtraContent);
        HasMainSection = ShowBack || HasAvatarSection || HasTitleSection || HasSubtitleSection || HasExtraSection;
        HasDefaultSection = HasMeaningfulContent(Content);
        HasFooterSection = HasMeaningfulContent(FooterContent);
    }

    private static object? ResolvePreferredContent(object? content, string? fallbackText)
    {
        if (HasMeaningfulContent(content))
        {
            return content;
        }

        return string.IsNullOrWhiteSpace(fallbackText) ? null : fallbackText;
    }

    private static bool HasMeaningfulContent(object? content)
    {
        return content switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }

    private static object CreateDefaultBackContent()
    {
        return new NIcon
        {
            Size = 22d,
            Data = DefaultBackGeometry.CloneCurrentValue()
        };
    }
}
