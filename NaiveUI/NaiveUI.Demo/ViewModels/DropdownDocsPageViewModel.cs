using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public partial class DropdownDocsPageViewModel : ObservableObject
{
    private bool manualDropdownVisible;
    private string boundOptionLabel = "从视图模型绑定的菜单标题";
    private string selectionText = "当前尚未选择菜单项。";
    private string optionActionText = "最近一次菜单动作：尚未触发。";

    public DropdownDocsPageViewModel(string selectedKey = "dropdown")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础", "SectionBasic"),
            ("图标", "SectionIcon"),
            ("触发方式", "SectionTrigger"),
            ("单项交互", "SectionAction"),
            ("多级联动", "SectionCascade"),
            ("分组与分割线", "SectionGroup"),
            ("显示箭头", "SectionArrow"),
            ("弹出位置", "SectionPlacement"),
            ("菜单尺寸", "SectionSize"),
            ("自定义内容", "SectionCustomContent"),
            ("接口说明", "SectionApi"),
            ("下拉菜单属性", "SectionDropdownProps"),
            ("菜单项与组合项属性", "SectionDropdownOptionProps"),
            ("其他类型", "SectionDropdownOtherTypes"),
            ("触发插槽", "SectionDropdownSlots"));
        OptionActionCommand = new RelayCommand<object>(HandleOptionActionCommand);

        DropdownPropsRows =
        [
            new ApiDocRow { Name = "Trigger", Type = "NDropdownTrigger", DefaultValue = "Hover", Description = "设置触发方式。可选 Hover、Click、Manual、ContextMenu。" },
            new ApiDocRow { Name = "Placement", Type = "NDropdownPlacement", DefaultValue = "Bottom", Description = "设置弹出位置。支持 Top / Bottom / Left / Right 及 Start、End 对齐变体。" },
            new ApiDocRow { Name = "Size", Type = "NDropdownSize", DefaultValue = "Medium", Description = "设置菜单尺寸。可选 Small、Medium、Large、Huge。" },
            new ApiDocRow { Name = "ShowArrow", Type = "bool", DefaultValue = "false", Description = "是否显示指向触发源的箭头。" },
            new ApiDocRow { Name = "Show", Type = "bool", DefaultValue = "false", Description = "设置当前下拉菜单是否显示，适合手动控制弹出状态。" },
            new ApiDocRow { Name = "Value", Type = "object", DefaultValue = "null", Description = "设置当前选中的菜单项键值，用于高亮当前项和多级路径。" },
            new ApiDocRow { Name = "MaxMenuHeight", Type = "double", DefaultValue = "320", Description = "设置菜单最大高度。超出后会出现滚动条。" },
            new ApiDocRow { Name = "HorizontalOffset", Type = "double", DefaultValue = "0", Description = "设置弹出层在水平方向上的额外偏移。" },
            new ApiDocRow { Name = "VerticalOffset", Type = "double", DefaultValue = "0", Description = "设置弹出层在垂直方向上的额外偏移。" },
            new ApiDocRow { Name = "Options", Type = "ObservableCollection<NDropdownOptionBase>", DefaultValue = "[]", Description = "设置下拉菜单项集合。单级场景使用 NDropdownOption，多级级联场景使用 NDropdownCombineOption。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "设置触发下拉菜单的内容，对应 Naive UI 的默认插槽。" }
        ];

        DropdownOptionRows =
        [
            new ApiDocRow { Name = "Key", Type = "object", DefaultValue = "null", Description = "菜单项唯一标识，对应 Naive UI 中的 key。" },
            new ApiDocRow { Name = "Label", Type = "object", DefaultValue = "null", Description = "菜单项显示内容，通常使用文本，也支持通过 Binding 绑定视图模型字段，或者直接放自定义 UI 元素。" },
            new ApiDocRow { Name = "Icon", Type = "object", DefaultValue = "null", Description = "菜单项左侧图标内容。" },
            new ApiDocRow { Name = "Suffix", Type = "object", DefaultValue = "null", Description = "菜单项右侧附加内容，例如快捷键、状态说明或补充文字。" },
            new ApiDocRow { Name = "Command", Type = "ICommand", DefaultValue = "null", Description = "绑定当前菜单项命令，适合在 MVVM 中直接处理单项行为。多级联动中，只有叶子 NDropdownOption 会执行命令。" },
            new ApiDocRow { Name = "CommandParameter", Type = "object", DefaultValue = "null", Description = "自定义传给 Command 的参数；未设置时会优先传入当前项的 NDropdownOptionClickEventArgs。" },
            new ApiDocRow { Name = "Click", Type = "event EventHandler<NDropdownOptionClickEventArgs>", DefaultValue = "null", Description = "当前菜单项的点击事件，适合在代码后置中直接处理；事件参数包含 Dropdown、Key 和 Option。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用当前菜单项。" },
            new ApiDocRow { Name = "Show", Type = "bool", DefaultValue = "true", Description = "是否显示当前菜单项。" },
            new ApiDocRow { Name = "Options", Type = "ObservableCollection<NDropdownOptionBase>", DefaultValue = "[]", Description = "仅 NDropdownCombineOption 提供。用于声明子菜单项，并支持继续嵌套多级联动结构。" }
        ];

        DropdownOtherOptionRows =
        [
            new ApiDocRow { Name = "NDropdownCombineOption", Type = "组合菜单", DefaultValue = "null", Description = "专门用于多级联动。它只负责组织子菜单、展示标签图标和展开层级，不提供 Click、Command、CommandParameter 等可执行能力。" },
            new ApiDocRow { Name = "NDropdownGroupOption", Type = "菜单分组", DefaultValue = "null", Description = "用于渲染分组标题，可与普通项或组合菜单混用。" },
            new ApiDocRow { Name = "NDropdownDividerOption", Type = "分割线", DefaultValue = "null", Description = "用于渲染菜单分割线。" },
            new ApiDocRow { Name = "NDropdownRenderOption", Type = "自定义内容", DefaultValue = "null", Description = "用于插入一段自定义内容，适合放说明、快捷入口或状态块。" }
        ];

        DropdownSlotsRows =
        [
            new ApiDocRow { Name = "默认插槽", Type = "object", DefaultValue = "null", Description = "触发下拉菜单的内容插槽。可以直接放 NButton 或其他任意 UI 元素。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> DropdownPropsRows { get; }

    public IReadOnlyList<ApiDocRow> DropdownOptionRows { get; }

    public IReadOnlyList<ApiDocRow> DropdownOtherOptionRows { get; }

    public IReadOnlyList<ApiDocRow> DropdownSlotsRows { get; }

    public bool ManualDropdownVisible
    {
        get => manualDropdownVisible;
        set => SetProperty(ref manualDropdownVisible, value);
    }

    public string BoundOptionLabel
    {
        get => boundOptionLabel;
        set => SetProperty(ref boundOptionLabel, value);
    }

    public string SelectionText
    {
        get => selectionText;
        set => SetProperty(ref selectionText, value);
    }

    public string OptionActionText
    {
        get => optionActionText;
        set => SetProperty(ref optionActionText, value);
    }

    public ICommand OptionActionCommand { get; }

    public void ToggleManualDropdown()
    {
        ManualDropdownVisible = !ManualDropdownVisible;
    }

    public void RecordSelection(object? key)
    {
        SelectionText = key is null
            ? "当前尚未选择菜单项。"
            : $"当前选择：{key}";
    }

    public void RecordOptionAction(string message)
    {
        OptionActionText = message;
    }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }

    private void HandleOptionActionCommand(object? parameter)
    {
        var message = parameter switch
        {
            NDropdownOptionClickEventArgs args when args.Key is not null => $"Command命令触发：{args.Key}",
            string text when !string.IsNullOrWhiteSpace(text) => $"Command命令参数：{text}",
            _ => "Command命令已触发"
        };

        NElMessage.Info(message);
        OptionActionText = message;
    }
}
