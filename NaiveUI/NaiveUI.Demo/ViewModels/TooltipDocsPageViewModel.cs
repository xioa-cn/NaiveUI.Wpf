using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public sealed class TooltipDocsPageViewModel : ViewModelBase
{
    private string interactiveStatusText = "最近一次交互：尚未触发";
    private bool manualTooltipVisible;

    public TooltipDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础", "SectionBasic"),
            ("触发方式", "SectionTrigger"),
            ("弹出位置", "SectionPlacement"),
            ("交互内容", "SectionInteractive"),
            ("外观与尺寸", "SectionAppearance"),
            ("API", "SectionApi"),
            ("Tooltip Props", "SectionTooltipProps"),
            ("Tooltip Events", "SectionTooltipEvents"),
            ("Tooltip Methods", "SectionTooltipMethods"),
            ("Tooltip Slots", "SectionTooltipSlots"));

        InteractiveActionCommand = new RelayCommand<object?>(HandleInteractiveAction);

        TooltipPropsRows =
        [
            new ApiDocRow { Name = "TooltipContent", Type = "object", DefaultValue = "null", Description = "弹层内容。支持字符串、绑定数据或任意 WPF 视觉元素。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "触发 Tooltip 的内容，对应 Naive UI 的 trigger slot。" },
            new ApiDocRow { Name = "Trigger", Type = "Hover | Click | Focus | Manual", DefaultValue = "Hover", Description = "触发方式。Hover 适合普通提示，Click 适合交互内容，Manual 适合外部完全控制。" },
            new ApiDocRow { Name = "Placement", Type = "NTooltipPlacement", DefaultValue = "Top", Description = "弹出位置。支持上下左右以及 Start / End 对齐变体。" },
            new ApiDocRow { Name = "Show", Type = "bool", DefaultValue = "false", Description = "当前是否显示 Tooltip。支持 TwoWay 绑定，适合 Manual 模式。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用 Tooltip 自身。" },
            new ApiDocRow { Name = "ShowArrow", Type = "bool", DefaultValue = "true", Description = "是否显示箭头。" },
            new ApiDocRow { Name = "Raw", Type = "bool", DefaultValue = "false", Description = "启用后关闭默认气泡壳层，只渲染 TooltipContent，方便完全自定义。" },
            new ApiDocRow { Name = "KeepAliveOnHover", Type = "bool", DefaultValue = "true", Description = "Hover 模式下，鼠标移入弹层后是否继续保持显示。" },
            new ApiDocRow { Name = "OpenDelay", Type = "int", DefaultValue = "100", Description = "Hover 模式下的打开延时，单位毫秒。" },
            new ApiDocRow { Name = "CloseDelay", Type = "int", DefaultValue = "100", Description = "关闭延时，单位毫秒。" },
            new ApiDocRow { Name = "Distance", Type = "double", DefaultValue = "8", Description = "弹层与触发器之间的额外距离。" },
            new ApiDocRow { Name = "Overlap", Type = "bool", DefaultValue = "false", Description = "是否与触发器贴合显示。启用后 Distance 会被忽略。" },
            new ApiDocRow { Name = "ArrowPointToCenter", Type = "bool", DefaultValue = "false", Description = "在 Start / End 变体下，让箭头尽量指向触发器中心。" },
            new ApiDocRow { Name = "HorizontalOffset", Type = "double", DefaultValue = "0", Description = "水平方向的额外偏移。" },
            new ApiDocRow { Name = "VerticalOffset", Type = "double", DefaultValue = "0", Description = "垂直方向的额外偏移。" },
            new ApiDocRow { Name = "PopupMinWidth", Type = "double", DefaultValue = "0", Description = "弹层最小宽度。" },
            new ApiDocRow { Name = "PopupMaxWidth", Type = "double", DefaultValue = "320", Description = "弹层最大宽度。" },
            new ApiDocRow { Name = "StretchToTriggerWidth", Type = "bool", DefaultValue = "false", Description = "是否让弹层宽度跟随触发器宽度。" },
            new ApiDocRow { Name = "ArrowSize", Type = "double", DefaultValue = "8", Description = "箭头尺寸。" },
            new ApiDocRow { Name = "ArrowEdgeMargin", Type = "double", DefaultValue = "12", Description = "Start / End 变体下箭头距离边缘的最小间距。" },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "6", Description = "默认气泡圆角。" },
            new ApiDocRow { Name = "Background / Foreground / BorderBrush / BorderThickness / Padding", Type = "Control 继承属性", DefaultValue = "主题默认值", Description = "用于调整默认气泡的背景、文字、边框和内边距。" },
            new ApiDocRow { Name = "FontSize / FontFamily / FontWeight", Type = "Control 继承属性", DefaultValue = "13 / 继承 / 继承", Description = "控制 Tooltip 内部文本的排版表现。" },
            
        ];

        TooltipEventsRows =
        [
            new ApiDocRow { Name = "Opened", Type = "event EventHandler", DefaultValue = "-", Description = "弹层打开后触发。" },
            new ApiDocRow { Name = "Closed", Type = "event EventHandler", DefaultValue = "-", Description = "弹层关闭后触发。" }
        ];

        TooltipMethodsRows =
        [
            new ApiDocRow { Name = "Open()", Type = "void", DefaultValue = "-", Description = "以代码方式打开 Tooltip。" },
            new ApiDocRow { Name = "Close()", Type = "void", DefaultValue = "-", Description = "以代码方式关闭 Tooltip。" }
        ];

        TooltipSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "-", Description = "默认内容槽，负责渲染触发元素。" },
            new ApiDocRow { Name = "TooltipContent", Type = "object", DefaultValue = "-", Description = "弹层内容槽，对应 Naive UI 的默认 Tooltip 内容区域。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> TooltipPropsRows { get; }

    public IReadOnlyList<ApiDocRow> TooltipEventsRows { get; }

    public IReadOnlyList<ApiDocRow> TooltipMethodsRows { get; }

    public IReadOnlyList<ApiDocRow> TooltipSlotsRows { get; }

    public ICommand InteractiveActionCommand { get; }

    public bool ManualTooltipVisible
    {
        get => manualTooltipVisible;
        set => SetProperty(ref manualTooltipVisible, value);
    }

    public string InteractiveStatusText
    {
        get => interactiveStatusText;
        set => SetProperty(ref interactiveStatusText, value);
    }

    private void HandleInteractiveAction(object? parameter)
    {
        var message = parameter?.ToString();
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "执行了一次 Tooltip 操作";
        }

        InteractiveStatusText = $"最近一次交互：{message}";
        NElMessage.Info(message);
    }
}
