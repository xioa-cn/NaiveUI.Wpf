using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class ButtonDocsPageViewModel: ViewModelBase
{
    public ButtonDocsPageViewModel(string selectedKey = "button")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);

        ButtonPropsRows =
        [
            new ApiDocRow { Name = "Grade", Type = "Grade", DefaultValue = "Default", Description = "设置按钮语义色阶。Default：默认中性色按钮；Primary：主按钮，用于页面主操作；Info：信息态按钮；Success：成功态按钮；Warning：警告态按钮；Error：危险或错误态按钮；Customize：启用自定义配色，通常配合 CustomBackgroundBrush、CustomBorderBrush、CustomForegroundBrush 等属性使用。" },
            new ApiDocRow { Name = "Kind", Type = "NButtonKind", DefaultValue = "Filled", Description = "设置按钮视觉形态。Filled：实心按钮；Secondary：次要实色按钮；Tertiary：浅边框按钮；Quaternary：轻量级按钮；Ghost：透明背景描边按钮；Dashed：虚线边框按钮；Text：纯文本按钮。" },
            new ApiDocRow { Name = "Size", Type = "NControlSize", DefaultValue = "Medium", Description = "设置按钮尺寸。Tiny：超小尺寸；Small：小尺寸；Medium：中尺寸；Large：大尺寸。" },
            new ApiDocRow { Name = "Round", Type = "bool", DefaultValue = "false", Description = "是否启用胶囊圆角按钮。" },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "3", Description = "自定义按钮圆角大小。设置后会优先于 Round 的默认胶囊圆角。" },
            new ApiDocRow { Name = "IsLoading", Type = "bool", DefaultValue = "false", Description = "是否显示加载状态。" },
            new ApiDocRow { Name = "LoadingBrush", Type = "Brush", DefaultValue = "null", Description = "加载指示器的画刷颜色。" },
            new ApiDocRow { Name = "CustomBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义常态背景色。" },
            new ApiDocRow { Name = "CustomBorderBrush", Type = "Brush", DefaultValue = "null", Description = "自定义常态边框色。" },
            new ApiDocRow { Name = "CustomForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义常态文字色。" },
            new ApiDocRow { Name = "CustomHoverBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义鼠标悬停背景色。" },
            new ApiDocRow { Name = "CustomHoverBorderBrush", Type = "Brush", DefaultValue = "null", Description = "自定义鼠标悬停边框色。" },
            new ApiDocRow { Name = "CustomHoverForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义鼠标悬停文字色。" },
            new ApiDocRow { Name = "CustomPressedBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按下状态背景色。" },
            new ApiDocRow { Name = "CustomPressedBorderBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按下状态边框色。" },
            new ApiDocRow { Name = "CustomPressedForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按下状态文字色。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "按钮内容，支持文本、图标或任意 WPF 视觉元素。" },
            new ApiDocRow { Name = "Command", Type = "ICommand", DefaultValue = "null", Description = "绑定命令源。" },
            new ApiDocRow { Name = "Click", Type = "RoutedEventHandler", DefaultValue = "-", Description = "点击按钮时触发。" },
            new ApiDocRow { Name = "IsEnabled", Type = "bool", DefaultValue = "true", Description = "是否可交互。" }
        ];

        ButtonGroupPropsRows =
        [
            new ApiDocRow { Name = "当前状态", Type = "说明", DefaultValue = "-", Description = "当前控件库尚未抽离独立的 NButtonGroup 控件，Demo 中按钮组由多个 NButton 组合排版实现。" }
        ];

        ButtonSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "按钮主内容区域。" },
            new ApiDocRow { Name = "Icon Content", Type = "UIElement", DefaultValue = "null", Description = "通过 Content 组合图标与文本时，可自行控制图标布局。" }
        ];

        ButtonGroupSlotsRows =
        [
            new ApiDocRow { Name = "当前状态", Type = "说明", DefaultValue = "-", Description = "尚未提供独立 ButtonGroup 插槽模型。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<ApiDocRow> ButtonPropsRows { get; }

    public IReadOnlyList<ApiDocRow> ButtonGroupPropsRows { get; }

    public IReadOnlyList<ApiDocRow> ButtonSlotsRows { get; }

    public IReadOnlyList<ApiDocRow> ButtonGroupSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
