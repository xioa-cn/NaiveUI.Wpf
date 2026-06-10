using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class MarqueeDocsPageViewModel : ViewModelBase
{
    private string eventText = "等待操作";

    public MarqueeDocsPageViewModel(string selectedKey = "marquee")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("图片内容", "SectionImage"),
            ("方向", "SectionDirection"),
            ("暂停与控制", "SectionControl"),
            ("复杂内容", "SectionRichContent"),
            ("绑定与副本", "SectionBinding"),
            ("接口说明", "SectionApi"),
            ("Marquee Props", "SectionMarqueeProps"),
            ("Marquee Methods", "SectionMarqueeMethods"),
            ("Marquee Events", "SectionMarqueeEvents"));

        MarqueePropsRows =
        [
            new ApiDocRow { Name = "Active", Type = "bool", DefaultValue = "true", Description = "是否启用跑马灯动画。关闭后内容停留在起始位置。" },
            new ApiDocRow { Name = "IsAnimationEnabled", Type = "bool", DefaultValue = "true", Description = "是否启用动画。用于禁用动效但保留布局和内容。" },
            new ApiDocRow { Name = "Direction", Type = "NMarqueeDirection", DefaultValue = "Left", Description = "滚动方向，可选 Left、Right、Up、Down。" },
            new ApiDocRow { Name = "Speed", Type = "double", DefaultValue = "48", Description = "滚动速度，单位为像素每秒。Duration 大于 0 时优先使用 Duration 计算速度。" },
            new ApiDocRow { Name = "Duration", Type = "double", DefaultValue = "0", Description = "完成一次内容位移的时长，单位为毫秒。用于对齐 Naive UI 中按时长控制动画节奏的场景。" },
            new ApiDocRow { Name = "Delay", Type = "double", DefaultValue = "0", Description = "开始滚动前的等待时间，单位为毫秒。" },
            new ApiDocRow { Name = "Gap", Type = "double", DefaultValue = "24", Description = "相邻两份内容之间的间距，单位为像素。" },
            new ApiDocRow { Name = "AutoFill", Type = "bool", DefaultValue = "false", Description = "是否按 Naive UI 的 autoFill 形式自动复制内容，让单个滚动组覆盖视口。" },
            new ApiDocRow { Name = "Repeat", Type = "int", DefaultValue = "1", Description = "每个滚动组内的最小内容份数；AutoFill=true 时会在此基础上按视口自动增补。" },
            new ApiDocRow { Name = "Loop", Type = "bool", DefaultValue = "true", Description = "是否循环播放。关闭后完成一轮位移会停止。" },
            new ApiDocRow { Name = "PauseOnHover", Type = "bool", DefaultValue = "true", Description = "鼠标悬停时是否暂停滚动。" },
            new ApiDocRow { Name = "Padding", Type = "Thickness", DefaultValue = "0", Description = "内容视口内边距。" },
            new ApiDocRow { Name = "Background", Type = "Brush", DefaultValue = "Transparent", Description = "容器背景。" },
            new ApiDocRow { Name = "BorderBrush / BorderThickness", Type = "Brush / Thickness", DefaultValue = "Transparent / 0", Description = "容器边框。" },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "0", Description = "容器圆角。" },
            new ApiDocRow { Name = "HorizontalContentAlignment", Type = "HorizontalAlignment", DefaultValue = "Left", Description = "垂直滚动时内容在水平方向上的对齐方式。" },
            new ApiDocRow { Name = "VerticalContentAlignment", Type = "VerticalAlignment", DefaultValue = "Center", Description = "水平滚动时内容在垂直方向上的对齐方式。" },
            new ApiDocRow { Name = "IsRunning", Type = "bool", DefaultValue = "false", Description = "只读，当前是否正在播放。" },
            new ApiDocRow { Name = "IsPaused", Type = "bool", DefaultValue = "false", Description = "只读，当前是否处于暂停状态。" },
            new ApiDocRow { Name = "ContentExtent", Type = "double", DefaultValue = "0", Description = "只读，内容在滚动轴上的测量尺寸。" },
            new ApiDocRow { Name = "ViewportExtent", Type = "double", DefaultValue = "0", Description = "只读，视口在滚动轴上的尺寸。" },
            new ApiDocRow { Name = "EffectiveRepeatCount", Type = "int", DefaultValue = "1", Description = "只读，当前每个滚动组内实际使用的内容份数。" }
        ];

        MarqueeMethodsRows =
        [
            new ApiDocRow { Name = "Play()", Type = "void", DefaultValue = "-", Description = "启用并开始播放。" },
            new ApiDocRow { Name = "Pause()", Type = "void", DefaultValue = "-", Description = "暂停当前播放进度。" },
            new ApiDocRow { Name = "Resume()", Type = "void", DefaultValue = "-", Description = "从暂停位置继续播放。" },
            new ApiDocRow { Name = "Stop()", Type = "void", DefaultValue = "-", Description = "停止播放，并把内容复位到起始位置。" },
            new ApiDocRow { Name = "Restart()", Type = "void", DefaultValue = "-", Description = "复位并重新播放。" }
        ];

        MarqueeEventsRows =
        [
            new ApiDocRow { Name = "Started", Type = "RoutedEventHandler", DefaultValue = "-", Description = "开始播放时触发。" },
            new ApiDocRow { Name = "Stopped", Type = "RoutedEventHandler", DefaultValue = "-", Description = "主动停止或非循环播放结束时触发。" },
            new ApiDocRow { Name = "CycleCompleted", Type = "RoutedEventHandler", DefaultValue = "-", Description = "每完成一轮内容位移时触发。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> MarqueePropsRows { get; }

    public IReadOnlyList<ApiDocRow> MarqueeMethodsRows { get; }

    public IReadOnlyList<ApiDocRow> MarqueeEventsRows { get; }

    public MarqueeBindingDemoItem BindingDemo { get; } = new();

    public string EventText
    {
        get => eventText;
        set => SetProperty(ref eventText, value);
    }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}

public sealed class MarqueeBindingDemoItem : ViewModelBase
{
    private string message = "这段内容来自 ContentTemplate，TextBox.Text 使用 TwoWay 绑定。";

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }
}
