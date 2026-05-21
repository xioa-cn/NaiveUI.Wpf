using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class FloatButtonDocsPageViewModel : ViewModelBase
{
    public FloatButtonDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("徽章", "SectionBadge"),
            ("气泡提示", "SectionTooltip"),
            ("描述", "SectionDescription"),
            ("按钮组", "SectionGroup"),
            ("菜单显示", "SectionMenu"),
            ("API", "SectionApi"),
            ("FloatButton Props", "SectionFloatButtonProps"),
            ("FloatButtonGroup Props", "SectionFloatButtonGroupProps"),
            ("FloatButton Slots", "SectionFloatButtonSlots"));

        FloatButtonPropsRows =
        [
            new ApiDocRow { Name = "Bottom", Type = "double | string", DefaultValue = "null", Description = "按钮的底部偏移。支持不带单位的数字或以 px 结尾的字符串。", Version = "2.38.0" },
            new ApiDocRow { Name = "Height", Type = "double", DefaultValue = "MinHeight = 40", Description = "按钮的最小高度。保留 Auto 布局能力，展示 description 时会自动增高。", Version = "2.38.0" },
            new ApiDocRow { Name = "Left", Type = "double | string", DefaultValue = "null", Description = "按钮的左侧偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "MenuTrigger", Type = "None | Hover | Click", DefaultValue = "None", Description = "菜单展开方式。设为 Hover 或 Click 后，Menu 插槽中的内容会以上浮菜单的方式显示。", Version = "2.38.0" },
            new ApiDocRow { Name = "Position", Type = "Relative | Absolute | Fixed", DefaultValue = "Fixed", Description = "按钮的定位方式。WPF 版本将 Absolute / Fixed 映射为父容器内对齐定位。", Version = "2.38.0" },
            new ApiDocRow { Name = "Right", Type = "double | string", DefaultValue = "null", Description = "按钮的右侧偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "Shape", Type = "Circle | Square", DefaultValue = "Circle", Description = "按钮形状。", Version = "2.38.0" },
            new ApiDocRow { Name = "ShowMenu", Type = "bool", DefaultValue = "False", Description = "是否展开菜单。支持 TwoWay 绑定。", Version = "2.38.0" },
            new ApiDocRow { Name = "Top", Type = "double | string", DefaultValue = "null", Description = "按钮的顶部偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "Type", Type = "Default | Primary", DefaultValue = "Default", Description = "按钮类型。", Version = "2.38.0" },
            new ApiDocRow { Name = "Width", Type = "double", DefaultValue = "40", Description = "按钮宽度。默认与 Naive UI 一样为 40。", Version = "2.38.0" }
        ];

        FloatButtonGroupPropsRows =
        [
            new ApiDocRow { Name = "Bottom", Type = "double | string", DefaultValue = "null", Description = "按钮组的底部偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "Left", Type = "double | string", DefaultValue = "null", Description = "按钮组的左侧偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "Position", Type = "Relative | Absolute | Fixed", DefaultValue = "Fixed", Description = "按钮组的定位方式。", Version = "2.38.0" },
            new ApiDocRow { Name = "Right", Type = "double | string", DefaultValue = "null", Description = "按钮组的右侧偏移。", Version = "2.38.0" },
            new ApiDocRow { Name = "Shape", Type = "Circle | Square", DefaultValue = "Circle", Description = "按钮组形状，会同步到组内每个 NFloatButton。", Version = "2.38.0" },
            new ApiDocRow { Name = "Top", Type = "double | string", DefaultValue = "null", Description = "按钮组的顶部偏移。", Version = "2.38.0" }
        ];

        FloatButtonSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "按钮主体内容，通常放置 NIcon。", Version = "2.38.0" },
            new ApiDocRow { Name = "Description", Type = "()", DefaultValue = "-", Description = "按钮中的描述信息。WPF 版本对应 Description 属性内容。", Version = "2.38.0" },
            new ApiDocRow { Name = "Menu", Type = "()", DefaultValue = "-", Description = "折叠菜单内容。WPF 版本对应 Menu 属性内容。多个按钮时请使用容器包裹。", Version = "2.38.0" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonPropsRows { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonGroupPropsRows { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonSlotsRows { get; }
}
