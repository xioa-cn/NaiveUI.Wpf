using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class WatermarkDocsPageViewModel : ViewModelBase
{
    private bool showFullscreenWatermark;
    private bool showImageWatermark;
    private string customText = "水印\n自定义水印";
    private double customFontSize = 16d;
    private double customRotate = -15d;
    private string customFontColorText = "rgba(128, 128, 128, .3)";
    private bool customCross = true;
    private string customFontStyleKey = "Normal";
    private int customFontWeightValue = 400;
    private double customGlobalRotate;
    private double customLineHeight = 16d;
    private double customMarkHeight = 128d;
    private double customMarkWidth = 192d;
    private double customXGap;
    private double customYGap;
    private double customXOffset = 12d;
    private double customYOffset = 28d;
    private int customZIndex = 10;
    private string customTextAlignKey = "Left";

    public WatermarkDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基本用法", "SectionBasic"),
            ("全屏幕", "SectionFullscreen"),
            ("图片", "SectionImage"),
            ("多行文本", "SectionMultiline"),
            ("自定义水印", "SectionCustom"),
            ("API", "SectionApi"),
            ("Watermark Props", "SectionWatermarkProps"),
            ("Watermark Slots", "SectionWatermarkSlots"));

        WatermarkPropsRows =
        [
            new ApiDocRow { Name = "Text", Type = "string", DefaultValue = "null", Description = "WPF 版用于映射 Naive UI 的 content 属性，以避免与 Content 默认插槽冲突。支持使用 \\n 分隔多行文本。" },
            new ApiDocRow { Name = "Cross", Type = "bool", DefaultValue = "false", Description = "是否跨越边界显示。" },
            new ApiDocRow { Name = "Debug", Type = "bool", DefaultValue = "false", Description = "是否显示调试边框。" },
            new ApiDocRow { Name = "FontSize", Type = "double", DefaultValue = "14", Description = "字体大小。复用 WPF 原生 FontSize。" },
            new ApiDocRow { Name = "FontFamily", Type = "FontFamily", DefaultValue = "inherit", Description = "字体族。复用 WPF 原生 FontFamily。" },
            new ApiDocRow { Name = "FontStyle", Type = "FontStyle", DefaultValue = "Normal", Description = "字体风格。复用 WPF 原生 FontStyle，支持 Normal、Italic、Oblique。" },
            new ApiDocRow { Name = "FontVariant", Type = "string", DefaultValue = "\"\"", Description = "字型语义占位，便于与 Naive UI API 对齐。当前 WPF 版保留该属性但不额外渲染变体。" },
            new ApiDocRow { Name = "FontWeight", Type = "FontWeight", DefaultValue = "Normal", Description = "字重。复用 WPF 原生 FontWeight。" },
            new ApiDocRow { Name = "FontStretch", Type = "FontStretch", DefaultValue = "Normal", Description = "字体拉伸。复用 WPF 原生 FontStretch。" },
            new ApiDocRow { Name = "FontColor", Type = "Brush", DefaultValue = "rgba(128, 128, 128, .3)", Description = "字体颜色。支持直接传入 SolidColorBrush 或 XAML 颜色字符串。" },
            new ApiDocRow { Name = "Fullscreen", Type = "bool", DefaultValue = "false", Description = "是否展示全屏水印。WPF 版通过 adorner 覆盖顶层窗口内容。" },
            new ApiDocRow { Name = "GlobalRotate", Type = "double", DefaultValue = "0", Description = "水印整体的旋转角度。" },
            new ApiDocRow { Name = "LineHeight", Type = "double", DefaultValue = "14", Description = "多行文本行高。" },
            new ApiDocRow { Name = "MarkHeight", Type = "double", DefaultValue = "32", Description = "单个水印块高度。对应 Naive UI 的 height。" },
            new ApiDocRow { Name = "Image", Type = "string", DefaultValue = "null", Description = "图片路径。支持 pack URI、本地路径或 HTTP/HTTPS 地址。" },
            new ApiDocRow { Name = "ImageHeight", Type = "double", DefaultValue = "NaN", Description = "图片高度。未设置时按图片原始比例推导。" },
            new ApiDocRow { Name = "ImageOpacity", Type = "double", DefaultValue = "1", Description = "图片不透明度。" },
            new ApiDocRow { Name = "ImageWidth", Type = "double", DefaultValue = "NaN", Description = "图片宽度。未设置时按图片原始比例推导。" },
            new ApiDocRow { Name = "Rotate", Type = "double", DefaultValue = "0", Description = "单个水印块内部内容的旋转角度。" },
            new ApiDocRow { Name = "Selectable", Type = "bool", DefaultValue = "true", Description = "是否允许被覆盖内容保持可选中语义。WPF 版保留该属性用于行为对齐。" },
            new ApiDocRow { Name = "TextAlign", Type = "TextAlignment", DefaultValue = "Left", Description = "多行文本对齐方式。支持 Left、Center、Right。" },
            new ApiDocRow { Name = "MarkWidth", Type = "double", DefaultValue = "32", Description = "单个水印块宽度。对应 Naive UI 的 width。" },
            new ApiDocRow { Name = "XGap", Type = "double", DefaultValue = "0", Description = "X 轴间隔。" },
            new ApiDocRow { Name = "XOffset", Type = "double", DefaultValue = "0", Description = "X 轴偏移。" },
            new ApiDocRow { Name = "YGap", Type = "double", DefaultValue = "0", Description = "Y 轴间隔。" },
            new ApiDocRow { Name = "YOffset", Type = "double", DefaultValue = "0", Description = "Y 轴偏移。" },
            new ApiDocRow { Name = "ZIndex", Type = "int", DefaultValue = "10", Description = "水印层级。" }
        ];

        WatermarkSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "被水印覆盖的内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> WatermarkPropsRows { get; }

    public IReadOnlyList<ApiDocRow> WatermarkSlotsRows { get; }

    public string WatermarkImageUri => "pack://application:,,,/NaiveUI.Demo;component/Assets/Header.jpg";

    public string MultilineText => "Naive UI\n有点意思...";

    public bool ShowFullscreenWatermark
    {
        get => showFullscreenWatermark;
        set => SetProperty(ref showFullscreenWatermark, value);
    }

    public bool ShowImageWatermark
    {
        get => showImageWatermark;
        set => SetProperty(ref showImageWatermark, value);
    }

    public string CustomText
    {
        get => customText;
        set => SetProperty(ref customText, value);
    }

    public double CustomFontSize
    {
        get => customFontSize;
        set => SetProperty(ref customFontSize, value);
    }

    public double CustomRotate
    {
        get => customRotate;
        set => SetProperty(ref customRotate, value);
    }

    public string CustomFontColorText
    {
        get => customFontColorText;
        set
        {
            SetProperty(ref customFontColorText, value);
            OnPropertyChanged(nameof(CustomFontColorBrush));
        }
    }

    public Brush CustomFontColorBrush => TryParseBrush(CustomFontColorText);

    public bool CustomCross
    {
        get => customCross;
        set => SetProperty(ref customCross, value);
    }

    public string CustomFontStyleKey
    {
        get => customFontStyleKey;
        set
        {
            SetProperty(ref customFontStyleKey, value);
            OnPropertyChanged(nameof(CustomFontStyle));
        }
    }

    public FontStyle CustomFontStyle => CustomFontStyleKey switch
    {
        "Italic" => FontStyles.Italic,
        "Oblique" => FontStyles.Oblique,
        _ => FontStyles.Normal
    };

    public int CustomFontWeightValue
    {
        get => customFontWeightValue;
        set
        {
            SetProperty(ref customFontWeightValue, value);
            OnPropertyChanged(nameof(CustomFontWeight));
        }
    }

    public FontWeight CustomFontWeight => FontWeight.FromOpenTypeWeight(Math.Clamp(CustomFontWeightValue, 100, 900));

    public double CustomGlobalRotate
    {
        get => customGlobalRotate;
        set => SetProperty(ref customGlobalRotate, value);
    }

    public double CustomLineHeight
    {
        get => customLineHeight;
        set => SetProperty(ref customLineHeight, value);
    }

    public double CustomMarkHeight
    {
        get => customMarkHeight;
        set => SetProperty(ref customMarkHeight, value);
    }

    public double CustomMarkWidth
    {
        get => customMarkWidth;
        set => SetProperty(ref customMarkWidth, value);
    }

    public double CustomXGap
    {
        get => customXGap;
        set => SetProperty(ref customXGap, value);
    }

    public double CustomYGap
    {
        get => customYGap;
        set => SetProperty(ref customYGap, value);
    }

    public double CustomXOffset
    {
        get => customXOffset;
        set => SetProperty(ref customXOffset, value);
    }

    public double CustomYOffset
    {
        get => customYOffset;
        set => SetProperty(ref customYOffset, value);
    }

    public int CustomZIndex
    {
        get => customZIndex;
        set => SetProperty(ref customZIndex, value);
    }

    public string CustomTextAlignKey
    {
        get => customTextAlignKey;
        set
        {
            SetProperty(ref customTextAlignKey, value);
            OnPropertyChanged(nameof(CustomTextAlign));
        }
    }

    public TextAlignment CustomTextAlign => CustomTextAlignKey switch
    {
        "Center" => TextAlignment.Center,
        "Right" => TextAlignment.Right,
        _ => TextAlignment.Left
    };

    private static Brush TryParseBrush(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new SolidColorBrush(Color.FromArgb(77, 128, 128, 128));
            }

            if (text.Trim().StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
            {
                var normalized = text.Trim()[5..^1];
                var segments = normalized.Split(',');
                if (segments.Length == 4
                    && byte.TryParse(segments[0].Trim(), out var r)
                    && byte.TryParse(segments[1].Trim(), out var g)
                    && byte.TryParse(segments[2].Trim(), out var b)
                    && double.TryParse(segments[3].Trim(), out var alpha))
                {
                    return new SolidColorBrush(Color.FromArgb(
                        (byte)Math.Clamp((int)Math.Round(alpha * 255d), 0, 255),
                        r,
                        g,
                        b));
                }
            }

            return (Brush)new BrushConverter().ConvertFromString(text)!;
        }
        catch
        {
            return new SolidColorBrush(Color.FromArgb(77, 128, 128, 128));
        }
    }
}
