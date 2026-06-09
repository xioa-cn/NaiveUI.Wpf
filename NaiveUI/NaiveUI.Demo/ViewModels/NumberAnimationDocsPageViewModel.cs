using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed partial class NumberAnimationDocsPageViewModel : ViewModelBase
{
    private double dynamicFrom = 0d;
    private double dynamicTo = 120000d;
    private bool manualActive;
    private string eventText = "等待中";

    public NumberAnimationDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("精度", "SectionPrecision"),
            ("手动播放", "SectionManual"),
            ("动态数值", "SectionDynamic"),
            ("格式化", "SectionFormat"),
            ("API", "SectionApi"),
            ("NumberAnimation Props", "SectionNumberAnimationProps"),
            ("NumberAnimation Methods", "SectionNumberAnimationMethods"),
            ("NumberAnimation Events", "SectionNumberAnimationEvents"));

        NumberAnimationPropsRows =
        [
            new ApiDocRow { Name = "From", Type = "double", DefaultValue = "0", Description = "动画起始数值，对齐 Naive UI 的 from。" },
            new ApiDocRow { Name = "To", Type = "double", DefaultValue = "0", Description = "动画目标数值，对齐 Naive UI 的 to。" },
            new ApiDocRow { Name = "Active", Type = "bool", DefaultValue = "true", Description = "是否在加载或输入变化时自动播放动画。" },
            new ApiDocRow { Name = "Duration", Type = "double", DefaultValue = "2000", Description = "动画持续时间，单位毫秒，对齐 Naive UI 的 duration。" },
            new ApiDocRow { Name = "Precision", Type = "int", DefaultValue = "0", Description = "小数精度，按指定位数四舍五入并补零显示。" },
            new ApiDocRow { Name = "ShowSeparator", Type = "bool", DefaultValue = "false", Description = "是否显示千分位分隔符，对齐 Naive UI 的 show-separator。" },
            new ApiDocRow { Name = "Locale", Type = "string", DefaultValue = "null", Description = "格式化区域名称，例如 en-US、de-DE、zh-CN。" },
            new ApiDocRow { Name = "Prefix", Type = "string", DefaultValue = "\"\"", Description = "显示文本前缀。" },
            new ApiDocRow { Name = "Suffix", Type = "string", DefaultValue = "\"\"", Description = "显示文本后缀。" },
            new ApiDocRow { Name = "FormatString", Type = "string", DefaultValue = "null", Description = ".NET 数值格式字符串；设置后会优先使用该格式。" },
            new ApiDocRow { Name = "IsAnimationEnabled", Type = "bool", DefaultValue = "true", Description = "是否启用动画；关闭后直接显示 To。" },
            new ApiDocRow { Name = "RestartOnValueChanged", Type = "bool", DefaultValue = "true", Description = "From、To、Active 等输入变化时是否自动重新播放。" },
            new ApiDocRow { Name = "UseCurrentValueAsFromOnChange", Type = "bool", DefaultValue = "false", Description = "To 变化时是否从当前显示值继续过渡。" },
            new ApiDocRow { Name = "EasingFunction", Type = "IEasingFunction", DefaultValue = "null", Description = "自定义 WPF 缓动函数；为空时使用 Naive UI 的 ease-out quint。" },
            new ApiDocRow { Name = "CurrentValue", Type = "double", DefaultValue = "0", Description = "只读，当前动画数值。" },
            new ApiDocRow { Name = "DisplayText", Type = "string", DefaultValue = "\"\"", Description = "只读，当前格式化后的显示文本。" },
            new ApiDocRow { Name = "IsAnimating", Type = "bool", DefaultValue = "false", Description = "只读，当前是否正在播放动画。" }
        ];

        NumberAnimationMethodsRows =
        [
            new ApiDocRow { Name = "Play()", Type = "void", DefaultValue = "-", Description = "当前未播放时，从 From 播放到 To，对齐 Naive UI 暴露的 play()。" },
            new ApiDocRow { Name = "Restart()", Type = "void", DefaultValue = "-", Description = "停止当前动画，并从 From 重新播放到 To。" },
            new ApiDocRow { Name = "Stop()", Type = "void", DefaultValue = "-", Description = "停止当前帧循环，保留当前显示值。" }
        ];

        NumberAnimationEventsRows =
        [
            new ApiDocRow { Name = "Started", Type = "RoutedEventHandler", DefaultValue = "-", Description = "动画开始时触发。" },
            new ApiDocRow { Name = "Completed", Type = "RoutedEventHandler", DefaultValue = "-", Description = "动画到达 To 时触发，对应 Naive UI 的 on-finish。" },
            new ApiDocRow { Name = "ValueChanged", Type = "RoutedPropertyChangedEventHandler<double>", DefaultValue = "-", Description = "CurrentValue 变化时触发。" }
        ];

        ReplayDynamicCommand = new RelayCommand(ReplayDynamic);
        ToggleManualCommand = new RelayCommand(ToggleManual);
        ResetManualCommand = new RelayCommand(ResetManual);
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> NumberAnimationPropsRows { get; }

    public IReadOnlyList<ApiDocRow> NumberAnimationMethodsRows { get; }

    public IReadOnlyList<ApiDocRow> NumberAnimationEventsRows { get; }

    public double DynamicFrom
    {
        get => dynamicFrom;
        private set => SetProperty(ref dynamicFrom, value);
    }

    public double DynamicTo
    {
        get => dynamicTo;
        private set => SetProperty(ref dynamicTo, value);
    }

    public bool ManualActive
    {
        get => manualActive;
        private set => SetProperty(ref manualActive, value);
    }

    public string EventText
    {
        get => eventText;
        set => SetProperty(ref eventText, value);
    }

    public ICommand ReplayDynamicCommand { get; }

    public ICommand ToggleManualCommand { get; }

    public ICommand ResetManualCommand { get; }

    private void ReplayDynamic()
    {
        var nextFrom = DynamicTo;
        var nextTo = DynamicTo >= 120000d ? -98765.432d : 120000d;
        DynamicFrom = nextFrom;
        DynamicTo = nextTo;
    }

    private void ToggleManual()
    {
        ManualActive = true;
    }

    private void ResetManual()
    {
        ManualActive = false;
    }
}
