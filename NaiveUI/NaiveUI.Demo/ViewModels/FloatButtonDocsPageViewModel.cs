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
            new ApiDocRow { Name = "Shape", Type = "Circle | Square", DefaultValue = "Circle", Description = "按钮形状。",   },
            new ApiDocRow { Name = "Bottom", Type = "double | string", DefaultValue = "null", Description = "按钮的底部偏移。支持不带单位的数字或以 px 结尾的字符串。",   },
            new ApiDocRow { Name = "Left", Type = "double | string", DefaultValue = "null", Description = "按钮的左侧偏移。",   },
            new ApiDocRow { Name = "Right", Type = "double | string", DefaultValue = "null", Description = "按钮的右侧偏移。",   },
            new ApiDocRow { Name = "Top", Type = "double | string", DefaultValue = "null", Description = "按钮的顶部偏移。",   },
            new ApiDocRow { Name = "Width", Type = "double", DefaultValue = "40", Description = "按钮宽度。默认与 Naive UI 一样为 40。",   },
            new ApiDocRow { Name = "Height", Type = "double", DefaultValue = "MinHeight = 40", Description = "按钮的最小高度。保留 Auto 布局能力，展示 description 时会自动增高。",   },
            new ApiDocRow { Name = "Position", Type = "Relative | Absolute | Fixed", DefaultValue = "Fixed", Description = "按钮的定位方式。WPF 版本将 Absolute / Fixed 映射为父容器内对齐定位。",   },
            new ApiDocRow { Name = "CustomBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮常态背景色。设置后会覆盖 Type 对应的默认背景色。",   },
            new ApiDocRow { Name = "CustomForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮常态前景色，通常用于自定义图标或文字颜色。",   },
            new ApiDocRow { Name = "CustomHoverBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮悬浮态背景色。未设置时会回退到常态背景或主题默认悬浮色。",   },
            new ApiDocRow { Name = "CustomHoverForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮悬浮态前景色。",   },
            new ApiDocRow { Name = "CustomPressedBackgroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮按下态背景色。",   },
            new ApiDocRow { Name = "CustomPressedForegroundBrush", Type = "Brush", DefaultValue = "null", Description = "自定义按钮按下态前景色。",   },
            new ApiDocRow { Name = "MenuPlacement", Type = "Top | Right | Bottom | Left", DefaultValue = "Top", Description = "菜单弹出位置。支持上、右、下、左四个方向。",   },
            new ApiDocRow { Name = "MenuOrientation", Type = "Vertical | Horizontal", DefaultValue = "Vertical", Description = "菜单内容未显式设置 Orientation 时的默认排列方向。若在 NFloatButtonStackPanel、StackPanel、WrapPanel 等容器上直接设置了 Orientation，则以容器自身配置为准。",   },
            new ApiDocRow { Name = "MenuTrigger", Type = "None | Hover | Click", DefaultValue = "None", Description = "菜单展开方式。设为 Hover 或 Click 后，Menu 插槽中的内容会以上浮菜单的方式显示。",   },
            new ApiDocRow { Name = "ShowMenu", Type = "bool", DefaultValue = "False", Description = "是否展开菜单。支持 TwoWay 绑定。",   },
            new ApiDocRow { Name = "Type", Type = "Default | Primary | Info | Success | Warning | Error | Customize", DefaultValue = "Default", Description = "按钮语义色类型。Customize 通常与 CustomBackgroundBrush、CustomForegroundBrush、CustomPressedBackgroundBrush 等属性配合使用。",   },
        ];

        FloatButtonGroupPropsRows =
        [
            new ApiDocRow { Name = "Shape", Type = "Circle | Square", DefaultValue = "Circle", Description = "按钮组形状，会同步到组内每个 NFloatButton。",   },
            new ApiDocRow { Name = "Background", Type = "Brush", DefaultValue = "Transparent / Square 时为 Theme.Surface.0.Brush", Description = "按钮组容器背景色。方形按钮组默认会使用主题面板色，手动设置后可覆盖默认值。",   },
            new ApiDocRow { Name = "Padding", Type = "Thickness", DefaultValue = "0", Description = "按钮组容器内边距。可为按钮内容与组边界之间留出额外空白。",   },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "8", Description = "按钮组容器圆角。常用于方形按钮组的外轮廓调整。",   },
            new ApiDocRow { Name = "BorderBrush", Type = "Brush", DefaultValue = "Transparent", Description = "按钮组容器边框颜色。",   },
            new ApiDocRow { Name = "BorderThickness", Type = "Thickness", DefaultValue = "0", Description = "按钮组容器边框粗细。",   },
            new ApiDocRow { Name = "Bottom", Type = "double | string", DefaultValue = "null", Description = "按钮组的底部偏移。",   },
            new ApiDocRow { Name = "Left", Type = "double | string", DefaultValue = "null", Description = "按钮组的左侧偏移。",   },
            new ApiDocRow { Name = "Right", Type = "double | string", DefaultValue = "null", Description = "按钮组的右侧偏移。",   },
            new ApiDocRow { Name = "Top", Type = "double | string", DefaultValue = "null", Description = "按钮组的顶部偏移。",   },
            new ApiDocRow { Name = "Orientation", Type = "Vertical | Horizontal", DefaultValue = "Vertical", Description = "按钮组排列方向。可配置纵向或横向布局。",   },
            new ApiDocRow { Name = "Position", Type = "Relative | Absolute | Fixed", DefaultValue = "Fixed", Description = "按钮组的定位方式。",   },
        ];

        FloatButtonSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "按钮主体内容，通常放置 NIcon。",   },
            new ApiDocRow { Name = "Description", Type = "()", DefaultValue = "-", Description = "按钮中的描述信息。WPF 版本对应 Description 属性内容。",   },
            new ApiDocRow { Name = "Menu", Type = "()", DefaultValue = "-", Description = "折叠菜单内容。WPF 版本对应 Menu 属性内容。多个按钮时请使用容器包裹。",   }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonPropsRows { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonGroupPropsRows { get; }

    public IReadOnlyList<ApiDocRow> FloatButtonSlotsRows { get; }
}
