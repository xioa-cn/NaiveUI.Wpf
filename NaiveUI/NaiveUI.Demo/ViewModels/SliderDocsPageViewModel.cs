using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class SliderDocsPageViewModel : ViewModelBase
{
    private double basicValue = 38d;
    private double stepValue = 20d;
    private double rangeStart = 20d;
    private double rangeEnd = 78d;
    private double markValue = 50d;
    private double verticalValue = 35d;
    private double reverseValue = 72d;
    private double linkedValue = 44d;
    private bool linkedDisabled;
    private bool linkedShowTooltip = true;
    private bool linkedReverse;
    private string eventText = "等待拖动";

    public SliderDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("禁用", "SectionDisabled"),
            ("步长", "SectionStep"),
            ("轨道粗度", "SectionRailSize"),
            ("范围选择", "SectionRange"),
            ("标记", "SectionMarks"),
            ("只在标记间选择", "SectionStepMark"),
            ("自定义标识和标记", "SectionCustomMark"),
            ("自定义滑块按钮", "SectionCustomThumb"),
            ("提示", "SectionTooltip"),
            ("垂直和反向", "SectionVerticalReverse"),
            ("内部控件联动", "SectionControl"),
            ("事件", "SectionEvent"),
            ("API", "SectionApi"),
            ("Slider Props", "SectionSliderProps"),
            ("Slider Events", "SectionSliderEvents"),
            ("Slider Methods", "SectionSliderMethods"),
            ("Slider Mark", "SectionSliderMark"));

        SliderPropsRows =
        [
            new ApiDocRow { Name = "Value", Type = "double", DefaultValue = "0", Description = "当前值，双向绑定属性。对应 Naive UI 非 range 模式下的 value。" },
            new ApiDocRow { Name = "DefaultValue", Type = "double", DefaultValue = "0", Description = "未显式设置 Value 时使用的初始值，对齐 default-value。" },
            new ApiDocRow { Name = "Range", Type = "bool", DefaultValue = "false", Description = "是否启用范围选择。WPF 版用 RangeStart / RangeEnd 承载 Naive UI 的 number[] 值。" },
            new ApiDocRow { Name = "RangeStart / RangeEnd", Type = "double", DefaultValue = "0", Description = "范围选择的起止值，均支持双向绑定。" },
            new ApiDocRow { Name = "DefaultRangeStart / DefaultRangeEnd", Type = "double", DefaultValue = "0", Description = "范围模式下的非受控初始值，可配合 Reset() 使用。" },
            new ApiDocRow { Name = "Min / Max", Type = "double", DefaultValue = "0 / 100", Description = "最小值和最大值，对齐 Naive UI 的 min / max。" },
            new ApiDocRow { Name = "Step", Type = "double | \"mark\"", DefaultValue = "1", Description = "步长。可写数值，也可写 mark 表示只能在 marks 上取值。" },
            new ApiDocRow { Name = "Marks", Type = "NSliderMarkCollection", DefaultValue = "[]", Description = "刻度标记集合，对齐 Naive UI 的 marks。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用。会同步 IsEnabled，写法贴近 Naive UI 的 disabled。" },
            new ApiDocRow { Name = "Keyboard", Type = "bool", DefaultValue = "true", Description = "是否允许方向键调整数值，对齐 keyboard。" },
            new ApiDocRow { Name = "Tooltip", Type = "bool", DefaultValue = "true", Description = "是否启用提示，对齐 tooltip。" },
            new ApiDocRow { Name = "ShowTooltip", Type = "bool?", DefaultValue = "null", Description = "是否总是显示提示。null 时悬浮或拖动显示，对齐 show-tooltip 的默认语义。" },
            new ApiDocRow { Name = "Placement", Type = "NSliderTooltipPlacement", DefaultValue = "Top", Description = "提示位置。支持 Top、Bottom、Left、Right。" },
            new ApiDocRow { Name = "Vertical", Type = "bool", DefaultValue = "false", Description = "是否垂直显示，对齐 vertical。" },
            new ApiDocRow { Name = "Reverse", Type = "bool", DefaultValue = "false", Description = "是否反向，对齐 reverse。" },
            new ApiDocRow { Name = "FormatString", Type = "string", DefaultValue = "null", Description = ".NET 数值格式串，用于格式化 tooltip，例如 P0、N1。" },
            new ApiDocRow { Name = "TooltipPrefix / TooltipSuffix", Type = "string", DefaultValue = "\"\"", Description = "提示文本前后缀，便于替代 Naive UI 的 format-tooltip 简单场景。" },
            new ApiDocRow { Name = "FormatTooltip", Type = "Func<double, string>", DefaultValue = "null", Description = "自定义提示格式化函数，对齐 Naive UI 的 format-tooltip。" },
            new ApiDocRow { Name = "IndicatorTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "自定义提示浮层内容，DataContext 为当前数值。" },
            new ApiDocRow { Name = "MarkTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "自定义标记标签内容，DataContext 为 NSliderMark。" },
            new ApiDocRow { Name = "MarkDotTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "自定义轨道上的标记点，DataContext 为 NSliderMark。" },
            new ApiDocRow { Name = "ThumbTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "自定义滑块按钮内容，DataContext 为当前滑块值。" },
            new ApiDocRow { Name = "RailSize", Type = "double", DefaultValue = "4", Description = "统一设置水平和垂直轨道粗度，轨道两端圆角会按粗度自动计算。" },
            new ApiDocRow { Name = "RailHeight / RailWidthVertical", Type = "double", DefaultValue = "4", Description = "水平轨道高度和垂直轨道宽度。" },
            new ApiDocRow { Name = "HandleSize", Type = "double", DefaultValue = "14", Description = "滑块按钮尺寸，对齐主题变量 handleSize。" },
            new ApiDocRow { Name = "DotWidth / DotHeight / DotBorderRadius", Type = "double", DefaultValue = "8 / 8 / 4", Description = "标记点尺寸和圆角，对齐 dot 相关主题变量。" },
            new ApiDocRow { Name = "MarkFontSize", Type = "double", DefaultValue = "12", Description = "标记文本字号，对齐 markFontSize。" },
            new ApiDocRow { Name = "RailBrush / RailHoverBrush", Type = "Brush", DefaultValue = "主题填充色", Description = "轨道默认和悬浮颜色。" },
            new ApiDocRow { Name = "RailDisabledBrush", Type = "Brush", DefaultValue = "主题填充色", Description = "禁用状态下的轨道颜色。" },
            new ApiDocRow { Name = "FillBrush / FillHoverBrush", Type = "Brush", DefaultValue = "Primary", Description = "选中进度默认和悬浮颜色。" },
            new ApiDocRow { Name = "FillDisabledBrush", Type = "Brush", DefaultValue = "Primary", Description = "禁用状态下的选中进度颜色。" },
            new ApiDocRow { Name = "HandleBrush", Type = "Brush", DefaultValue = "Theme.Surface.0", Description = "滑块按钮背景色。" },
            new ApiDocRow { Name = "HandleBorderBrush", Type = "Brush", DefaultValue = "Primary", Description = "滑块按钮边框色。" },
            new ApiDocRow { Name = "HandleDisabledBrush / HandleDisabledBorderBrush", Type = "Brush", DefaultValue = "主题色", Description = "禁用状态下的滑块背景和边框色。" },
            new ApiDocRow { Name = "DotBrush / DotActiveBrush", Type = "Brush", DefaultValue = "主题色", Description = "标记点默认和激活背景色。" },
            new ApiDocRow { Name = "DotBorderBrush / DotActiveBorderBrush", Type = "Brush", DefaultValue = "主题色", Description = "标记点默认和激活边框色。" },
            new ApiDocRow { Name = "DotDisabledBrush / DotDisabledBorderBrush", Type = "Brush", DefaultValue = "主题色", Description = "禁用状态下的标记点背景和边框色。" },
            new ApiDocRow { Name = "MarkForeground / MarkDisabledForeground", Type = "Brush", DefaultValue = "主题文本色", Description = "标记文本默认和禁用状态前景色。" },
            new ApiDocRow { Name = "IndicatorBrush / IndicatorForeground", Type = "Brush", DefaultValue = "Tooltip 主题色", Description = "提示浮层背景和文字色。" },
            new ApiDocRow { Name = "IndicatorBorderRadius", Type = "double", DefaultValue = "3", Description = "提示浮层圆角。" },
            new ApiDocRow { Name = "DisabledOpacity", Type = "double", DefaultValue = "0.5", Description = "禁用态透明度，对齐 opacityDisabled 主题变量。" }
        ];

        SliderEventRows =
        [
            new ApiDocRow { Name = "ValueChanged", Type = "RoutedPropertyChangedEventHandler<double>", DefaultValue = "-", Description = "单值模式下值变化事件，对齐 on-update:value。" },
            new ApiDocRow { Name = "RangeValueChanged", Type = "RoutedEventHandler", DefaultValue = "-", Description = "范围模式下起止值变化事件。" },
            new ApiDocRow { Name = "DragStarted", Type = "RoutedEventHandler", DefaultValue = "-", Description = "开始拖动时触发，对齐 on-dragstart。" },
            new ApiDocRow { Name = "DragCompleted", Type = "RoutedEventHandler", DefaultValue = "-", Description = "结束拖动时触发，对齐 on-dragend。" }
        ];

        SliderMethodRows =
        [
            new ApiDocRow { Name = "Reset()", Type = "void", DefaultValue = "-", Description = "恢复到 DefaultValue 或 DefaultRangeStart / DefaultRangeEnd。" },
            new ApiDocRow { Name = "Focus()", Type = "bool", DefaultValue = "-", Description = "继承自 Control，用于键盘操作入口。" }
        ];

        SliderMarkRows =
        [
            new ApiDocRow { Name = "Value", Type = "double", DefaultValue = "0", Description = "标记所在数值位置。" },
            new ApiDocRow { Name = "Label", Type = "object", DefaultValue = "null", Description = "标记文本。当前 Demo 使用字符串，保留 object 便于后续扩展模板内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> SliderPropsRows { get; }

    public IReadOnlyList<ApiDocRow> SliderEventRows { get; }

    public IReadOnlyList<ApiDocRow> SliderMethodRows { get; }

    public IReadOnlyList<ApiDocRow> SliderMarkRows { get; }

    public double BasicValue
    {
        get => basicValue;
        set
        {
            if (basicValue == value)
            {
                return;
            }

            SetProperty(ref basicValue, value);
            OnPropertyChanged(nameof(BasicStatusText));
        }
    }

    public double StepValue
    {
        get => stepValue;
        set
        {
            if (stepValue == value)
            {
                return;
            }

            SetProperty(ref stepValue, value);
            OnPropertyChanged(nameof(StepStatusText));
        }
    }

    public double RangeStart
    {
        get => rangeStart;
        set
        {
            if (rangeStart == value)
            {
                return;
            }

            SetProperty(ref rangeStart, value);
            OnPropertyChanged(nameof(RangeStatusText));
        }
    }

    public double RangeEnd
    {
        get => rangeEnd;
        set
        {
            if (rangeEnd == value)
            {
                return;
            }

            SetProperty(ref rangeEnd, value);
            OnPropertyChanged(nameof(RangeStatusText));
        }
    }

    public double MarkValue
    {
        get => markValue;
        set
        {
            if (markValue == value)
            {
                return;
            }

            SetProperty(ref markValue, value);
            OnPropertyChanged(nameof(MarkStatusText));
        }
    }

    public double VerticalValue
    {
        get => verticalValue;
        set => SetProperty(ref verticalValue, value);
    }

    public double ReverseValue
    {
        get => reverseValue;
        set => SetProperty(ref reverseValue, value);
    }

    public double LinkedValue
    {
        get => linkedValue;
        set
        {
            if (linkedValue == value)
            {
                return;
            }

            SetProperty(ref linkedValue, value);
            OnPropertyChanged(nameof(LinkedValueText));
            OnPropertyChanged(nameof(LinkedStatusText));
        }
    }

    public bool LinkedDisabled
    {
        get => linkedDisabled;
        set
        {
            if (linkedDisabled == value)
            {
                return;
            }

            SetProperty(ref linkedDisabled, value);
            OnPropertyChanged(nameof(LinkedStatusText));
        }
    }

    public bool LinkedShowTooltip
    {
        get => linkedShowTooltip;
        set => SetProperty(ref linkedShowTooltip, value);
    }

    public bool LinkedReverse
    {
        get => linkedReverse;
        set
        {
            if (linkedReverse == value)
            {
                return;
            }

            SetProperty(ref linkedReverse, value);
            OnPropertyChanged(nameof(LinkedStatusText));
        }
    }

    public string EventText
    {
        get => eventText;
        set => SetProperty(ref eventText, value);
    }

    public string BasicStatusText => $"当前值：{BasicValue:0}";

    public string StepStatusText => $"当前值：{StepValue:0}";

    public string RangeStatusText => $"范围：{RangeStart:0} - {RangeEnd:0}";

    public string MarkStatusText => $"当前刻度：{MarkValue:0}";

    public string LinkedValueText => $"{LinkedValue:0}%";

    public string LinkedStatusText => $"当前值：{LinkedValue:0}%，{(LinkedDisabled ? "已禁用" : "可操作")}，{(LinkedReverse ? "反向" : "正向")}";

    public void RecordValueChanged(double oldValue, double newValue)
    {
        EventText = $"值变化：{oldValue:0} -> {newValue:0}";
    }

    public void RecordDragStarted()
    {
        EventText = "开始拖动";
    }

    public void RecordDragCompleted()
    {
        EventText = "结束拖动";
    }
}
