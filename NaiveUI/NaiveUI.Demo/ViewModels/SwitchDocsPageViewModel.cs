using System.Collections.Generic;
using System.Threading.Tasks;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class SwitchDocsPageViewModel : ViewModelBase
{
    private bool basicValue = true;
    private bool contentValue = true;
    private bool asyncLoadingValue;
    private bool asyncLoading;
    private bool eventValue = true;
    private string eventStatusText = "最近一次触发：暂无";
    private string customValue = "关闭";
    private bool roundValue = true;
    private bool squareValue;
    private bool colorValue = true;
    private bool iconValue = true;
    private bool iconOnlyValue;

    public SwitchDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("尺寸", "SectionSize"),
            ("内容", "SectionContent"),
            ("加载中", "SectionLoading"),
            ("事件", "SectionEvent"),
            ("自定义选中值", "SectionCustomValue"),
            ("形状", "SectionShape"),
            ("颜色", "SectionColor"),
            ("图标", "SectionIcon"),
            ("API", "SectionApi"),
            ("Switch 属性", "SectionSwitchProps"),
            ("Switch 事件", "SectionSwitchEvents"),
            ("Switch 内容属性", "SectionSwitchContentProps"));

        SwitchPropsRows =
        [
            new ApiDocRow { Name = "Value", Type = "object", DefaultValue = "null", Description = "当前值。默认使用 bool，也支持配合 CheckedValue / UncheckedValue 绑定为字符串、数字或任意对象。" },
            new ApiDocRow { Name = "DefaultValue", Type = "object", DefaultValue = "null", Description = "非受控初始值。未显式绑定 Value 时，控件首次加载会以它作为初始状态。" },
            new ApiDocRow { Name = "CheckedValue", Type = "object", DefaultValue = "true", Description = "选中时写回到 Value 的值，对齐 Naive UI 的 checked-value。" },
            new ApiDocRow { Name = "UncheckedValue", Type = "object", DefaultValue = "false", Description = "未选中时写回到 Value 的值，对齐 Naive UI 的 unchecked-value。" },
            new ApiDocRow { Name = "IsChecked", Type = "bool", DefaultValue = "false", Description = "继承自 ToggleButton 的标准布尔状态属性。与 Value 双向同步，适合纯 WPF 布尔绑定场景。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用。与 IsEnabled 双向同步，写法更接近 Naive UI 的 disabled。" },
            new ApiDocRow { Name = "Loading", Type = "bool", DefaultValue = "false", Description = "是否进入加载态。加载时会阻止再次切换，并在按钮内显示旋转指示器。" },
            new ApiDocRow { Name = "Command", Type = "ICommand", DefaultValue = "null", Description = "继承自 ButtonBase 的命令属性。适合在 MVVM 中直接绑定命令，而不依赖代码后置事件。" },
            new ApiDocRow { Name = "CommandParameter", Type = "object", DefaultValue = "null", Description = "命令参数。会随着点击一并传入绑定的 Command。" },
            new ApiDocRow { Name = "CommandTarget", Type = "IInputElement", DefaultValue = "null", Description = "命令目标。需要将 RoutedCommand 路由到特定元素时可以使用它。" },
            new ApiDocRow { Name = "Size", Type = "NControlSize", DefaultValue = "Medium", Description = "尺寸。支持 Tiny、Small、Medium、Large。" },
            new ApiDocRow { Name = "TrackWidth / TrackHeight / ThumbSize", Type = "double", DefaultValue = "NaN", Description = "手动指定轨道宽度、轨道高度和按钮尺寸。未设置时按 Size 和字体自动计算。" },
            new ApiDocRow { Name = "Round", Type = "bool", DefaultValue = "true", Description = "是否使用圆角轨道和圆形按钮。关闭后可得到接近官方 round=false 的方形开关。" },
            new ApiDocRow { Name = "RailRadius / ThumbRadius", Type = "double", DefaultValue = "NaN", Description = "手动指定轨道圆角和按钮圆角。设为 0 可得到直角风格；未设置时按 Round 和尺寸自动推导。" },
            new ApiDocRow { Name = "RubberBand", Type = "bool", DefaultValue = "true", Description = "按下时是否启用按钮轻微弹性形变，模拟 Naive UI 的 rubber-band 反馈。" },
            new ApiDocRow { Name = "ContentFontSize / ContentFontFamily / ContentFontWeight", Type = "double / FontFamily / FontWeight", DefaultValue = "自动 / null / SemiBold", Description = "轨道内文案的字号、字体和字重。适合对齐业务品牌字形或强调不同视觉层级。" },
            new ApiDocRow { Name = "CheckedRailBrush", Type = "Brush", DefaultValue = "主题主色", Description = "选中状态下轨道背景色。" },
            new ApiDocRow { Name = "UncheckedRailBrush", Type = "Brush", DefaultValue = "主题边框强色", Description = "未选中状态下轨道背景色。" },
            new ApiDocRow { Name = "CheckedRailHoverBrush / UncheckedRailHoverBrush", Type = "Brush", DefaultValue = "null", Description = "悬停时轨道颜色覆盖。未设置时使用主题 hover 色。" },
            new ApiDocRow { Name = "CheckedRailPressedBrush / UncheckedRailPressedBrush", Type = "Brush", DefaultValue = "null", Description = "按下时轨道颜色覆盖。未设置时使用主题 pressed 色。" },
            new ApiDocRow { Name = "ThumbBrush", Type = "Brush", DefaultValue = "null", Description = "滑块按钮的通用背景色。未设置状态专属颜色时，两种状态都会使用它。" },
            new ApiDocRow { Name = "CheckedThumbBrush / UncheckedThumbBrush", Type = "Brush", DefaultValue = "null", Description = "滑块按钮在选中和未选中状态下的背景色，语义上比 ButtonBrush 更直观。" },
            new ApiDocRow { Name = "CheckedButtonBrush / UncheckedButtonBrush", Type = "Brush", DefaultValue = "白色", Description = "兼容旧命名的滑块按钮背景色属性，仍然可用；优先级低于 CheckedThumbBrush / UncheckedThumbBrush。" },
            new ApiDocRow { Name = "CheckedContentBrush / UncheckedContentBrush", Type = "Brush", DefaultValue = "主题前景", Description = "轨道内文案颜色。" },
            new ApiDocRow { Name = "CheckedIconBrush / UncheckedIconBrush", Type = "Brush", DefaultValue = "主题色 / 次级文字色", Description = "按钮内图标颜色。" },
            new ApiDocRow { Name = "FocusRingBrush", Type = "Brush", DefaultValue = "主题计算值", Description = "键盘聚焦时外圈描边颜色。" },
            new ApiDocRow { Name = "LoadingBrush", Type = "Brush", DefaultValue = "跟随图标颜色", Description = "加载指示器描边颜色。" },
            new ApiDocRow { Name = "LoadingStrokeThickness", Type = "double", DefaultValue = "2", Description = "加载指示器线宽。" },
            new ApiDocRow { Name = "LoadingScale", Type = "double", DefaultValue = "1", Description = "加载指示器缩放倍率。" },
            new ApiDocRow { Name = "Width / MinWidth / FontSize", Type = "继承属性", DefaultValue = "主题默认值", Description = "可继续使用 WPF 原生尺寸与字体属性微调轨道宽度、文本大小和整体视觉密度。" }
        ];

        SwitchEventRows =
        [
            new ApiDocRow { Name = "ValueChanged", Type = "RoutedPropertyChangedEventHandler<object>", DefaultValue = "-", Description = "值变化事件，对齐 Naive UI 的 on-update:value。参数中同时提供旧值和新值。" },
            new ApiDocRow { Name = "Checked", Type = "RoutedEventHandler", DefaultValue = "-", Description = "继承自 ToggleButton，切换到选中状态时触发。" },
            new ApiDocRow { Name = "Unchecked", Type = "RoutedEventHandler", DefaultValue = "-", Description = "继承自 ToggleButton，切换到未选中状态时触发。" },
            new ApiDocRow { Name = "Click", Type = "RoutedEventHandler", DefaultValue = "-", Description = "继承自 ButtonBase，用户触发点击时触发。若只关心交互动作而非最终值，可继续使用它。" }
        ];

        SwitchContentPropRows =
        [
            new ApiDocRow { Name = "CheckedContent", Type = "object", DefaultValue = "null", Description = "选中状态显示在轨道内的内容，对齐 Naive UI 的 checked 插槽。" },
            new ApiDocRow { Name = "UncheckedContent", Type = "object", DefaultValue = "null", Description = "未选中状态显示在轨道内的内容，对齐 Naive UI 的 unchecked 插槽。" },
            new ApiDocRow { Name = "IconContent", Type = "object", DefaultValue = "null", Description = "按钮内的默认图标内容。未设置状态专属图标时会使用它，可直接传入 NIcon。" },
            new ApiDocRow { Name = "CheckedIconContent", Type = "object", DefaultValue = "null", Description = "选中状态的按钮图标内容，对齐 Naive UI 的 checked-icon 插槽。推荐直接传 NIcon + Data(path)。" },
            new ApiDocRow { Name = "UncheckedIconContent", Type = "object", DefaultValue = "null", Description = "未选中状态的按钮图标内容，对齐 Naive UI 的 unchecked-icon 插槽。推荐直接传 NIcon + Data(path)。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> SwitchPropsRows { get; }

    public IReadOnlyList<ApiDocRow> SwitchEventRows { get; }

    public IReadOnlyList<ApiDocRow> SwitchContentPropRows { get; }

    public bool BasicValue
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

    public bool ContentValue
    {
        get => contentValue;
        set
        {
            if (contentValue == value)
            {
                return;
            }

            SetProperty(ref contentValue, value);
            OnPropertyChanged(nameof(ContentStatusText));
        }
    }

    public bool AsyncLoadingValue
    {
        get => asyncLoadingValue;
        set
        {
            if (asyncLoadingValue == value)
            {
                return;
            }

            SetProperty(ref asyncLoadingValue, value);
            OnPropertyChanged(nameof(AsyncLoadingStatusText));
        }
    }

    public bool AsyncLoading
    {
        get => asyncLoading;
        set
        {
            if (asyncLoading == value)
            {
                return;
            }

            SetProperty(ref asyncLoading, value);
            OnPropertyChanged(nameof(AsyncLoadingStatusText));
        }
    }

    public bool EventValue
    {
        get => eventValue;
        set => SetProperty(ref eventValue, value);
    }

    public string EventStatusText
    {
        get => eventStatusText;
        private set => SetProperty(ref eventStatusText, value);
    }

    public string CustomValue
    {
        get => customValue;
        set
        {
            if (customValue == value)
            {
                return;
            }

            SetProperty(ref customValue, value);
            OnPropertyChanged(nameof(CustomValueStatusText));
        }
    }

    public bool RoundValue
    {
        get => roundValue;
        set => SetProperty(ref roundValue, value);
    }

    public bool SquareValue
    {
        get => squareValue;
        set => SetProperty(ref squareValue, value);
    }

    public bool ColorValue
    {
        get => colorValue;
        set => SetProperty(ref colorValue, value);
    }

    public bool IconValue
    {
        get => iconValue;
        set => SetProperty(ref iconValue, value);
    }

    public bool IconOnlyValue
    {
        get => iconOnlyValue;
        set => SetProperty(ref iconOnlyValue, value);
    }

    public string CustomValueStatusText => $"当前 Value = \"{CustomValue}\"";

    public string BasicStatusText => $"当前状态：{(BasicValue ? "开启" : "关闭")}";

    public string ContentStatusText => $"当前状态：{(ContentValue ? "开启" : "关闭")}";

    public string AsyncLoadingStatusText
        => AsyncLoading
            ? "正在提交中，开关会在请求完成后再真正切换。"
            : $"当前状态：{(AsyncLoadingValue ? "开启" : "关闭")}";

    public void RecordEventChange(object? oldValue, object? newValue)
    {
        EventStatusText = $"最近一次触发：{FormatValue(oldValue)} -> {FormatValue(newValue)}";
    }

    public void BeginAsyncLoadingChange(object? oldValue)
    {
        AsyncLoading = true;
        AsyncLoadingValue = oldValue is bool oldBool && oldBool;
    }

    public async Task CompleteAsyncLoadingChangeAsync(object? newValue)
    {
        await Task.Delay(1800);
        AsyncLoadingValue = newValue is bool newBool && newBool;
        AsyncLoading = false;
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            bool boolValue => boolValue ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }
}
