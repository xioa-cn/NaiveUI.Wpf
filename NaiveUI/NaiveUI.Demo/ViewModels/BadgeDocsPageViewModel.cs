using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class BadgeDocsPageViewModel : ViewModelBase
{
    private int basicValue = 5;
    private int overflowValue = 101;
    private int showZeroValue;
    private bool manualBadgeVisible = true;

    public BadgeDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础", "SectionBasic"),
            ("数值溢出", "SectionOverflow"),
            ("处理中", "SectionProcessing"),
            ("显示零值", "SectionShowZero"),
            ("偏移", "SectionOffset"),
            ("类型", "SectionType"),
            ("颜色", "SectionColor"),
            ("自定义内容", "SectionCustomContent"),
            ("独立使用", "SectionRaw"),
            ("手动控制", "SectionManual"),
            ("API", "SectionApi"),
            ("Badge Props", "SectionBadgeProps"),
            ("Badge Slots", "SectionBadgeSlots"));

        BadgeActionCommand = new RelayCommand<object?>(HandleBadgeAction);

        BadgePropsRows =
        [
            new ApiDocRow { Name = "Value", Type = "object", DefaultValue = "null", Description = "标记内容。可传数字、字符串或任意 WPF 视觉元素，对应 Naive UI 的 value / value slot。" },
            new ApiDocRow { Name = "Max", Type = "int?", DefaultValue = "null", Description = "数值溢出阈值。Value 为数字且超过该值时显示为 Max+。" },
            new ApiDocRow { Name = "Dot", Type = "bool", DefaultValue = "false", Description = "是否只显示圆点。启用后会忽略 Value 的文本渲染。" },
            new ApiDocRow { Name = "Type", Type = "NBadgeType", DefaultValue = "Default", Description = "语义类型。支持 Default、Info、Success、Warning、Error。" },
            new ApiDocRow { Name = "Show", Type = "bool", DefaultValue = "true", Description = "是否显示标记，适合手动控制显示状态。" },
            new ApiDocRow { Name = "ShowZero", Type = "bool", DefaultValue = "false", Description = "Value 为数字 0 时是否仍然显示。" },
            new ApiDocRow { Name = "Processing", Type = "bool", DefaultValue = "false", Description = "是否显示 processing 波纹动画，主要用于 dot 模式。" },
            new ApiDocRow { Name = "Color", Type = "Brush", DefaultValue = "null", Description = "自定义标记背景色。设置后优先于 Type 对应的主题颜色。" },
            new ApiDocRow { Name = "Offset", Type = "Point", DefaultValue = "0,0", Description = "相对默认锚点的偏移量。X 向左为负、向右为正；Y 向上为负、向下为正。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "被标记包裹的内容。为空时进入 raw 模式，Badge 本身独立渲染。" },
            new ApiDocRow { Name = "Foreground / FontSize / FontWeight / Padding", Type = "Control 继承属性", DefaultValue = "主题默认值", Description = "用于调整标记文本颜色、字号、字重和胶囊内边距。" }
        ];

        BadgeSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "()", DefaultValue = "-", Description = "默认插槽，被标记包裹的内容。" },
            new ApiDocRow { Name = "Value", Type = "()", DefaultValue = "-", Description = "标记内容插槽。WPF 中可通过 Value 属性直接传字符串、数字或使用属性元素传入 UIElement。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> BadgePropsRows { get; }

    public IReadOnlyList<ApiDocRow> BadgeSlotsRows { get; }

    public ICommand BadgeActionCommand { get; }

    public int BasicValue
    {
        get => basicValue;
        private set => SetProperty(ref basicValue, value);
    }

    public int OverflowValue
    {
        get => overflowValue;
        private set => SetProperty(ref overflowValue, value);
    }

    public int ShowZeroValue
    {
        get => showZeroValue;
        private set => SetProperty(ref showZeroValue, value);
    }

    public bool ManualBadgeVisible
    {
        get => manualBadgeVisible;
        private set
        {
            SetProperty(ref manualBadgeVisible, value);
            OnPropertyChanged(nameof(ManualVisibilityText));
        }
    }

    public string ManualVisibilityText => $"当前 show = {ManualBadgeVisible.ToString().ToLowerInvariant()}";

    private void HandleBadgeAction(object? parameter)
    {
        switch (parameter?.ToString())
        {
            case "basic:-":
                BasicValue = Math.Max(0, BasicValue - 1);
                break;
            case "basic:+":
                BasicValue += 1;
                break;
            case "overflow:-":
                OverflowValue = Math.Max(0, OverflowValue - 1);
                break;
            case "overflow:+":
                OverflowValue += 1;
                break;
            case "show-zero:-":
                ShowZeroValue = Math.Max(0, ShowZeroValue - 1);
                break;
            case "show-zero:+":
                ShowZeroValue += 1;
                break;
            case "manual:show":
                ManualBadgeVisible = true;
                break;
            case "manual:hide":
                ManualBadgeVisible = false;
                break;
        }
    }
}
