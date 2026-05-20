using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class IconDocsPageViewModel : ViewModelBase
{
    public IconDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("自定义图标", "SectionCustom"),
            ("深度", "SectionDepth"),
            ("带背景色的图标", "SectionWrapper"),
            ("API", "SectionApi"),
            ("Icon Props", "SectionIconProps"),
            ("IconWrapper Props", "SectionIconWrapperProps"),
            ("Icon Slots", "SectionIconSlots"));

        IconPropsRows =
        [
            new ApiDocRow { Name = "Color", Type = "Brush | string", DefaultValue = "null", Description = "图标颜色。支持直接传 Brush，也支持在 XAML 中写颜色字符串，例如 #0e7a0d。" },
            new ApiDocRow { Name = "Depth", Type = "1 | 2 | 3 | 4 | 5", DefaultValue = "null", Description = "图标深度。会按 Naive UI 的浅色 / 深色透明度规则降低图标不透明度。" },
            new ApiDocRow { Name = "Size", Type = "number | string", DefaultValue = "20", Description = "图标大小。未指定单位时按 px 处理。WPF 默认渲染尺寸为 20。" },
            new ApiDocRow { Name = "Component", Type = "object", DefaultValue = "null", Description = "要展示的图标组件。WPF 版本可直接传入一个可视元素，对应 Naive UI 的 component。" },
            new ApiDocRow { Name = "Data", Type = "Geometry", DefaultValue = "null", Description = "WPF 补充属性。直接传入 Geometry 来绘制图标，适合本地图标资源和几何字符串。" },
            new ApiDocRow { Name = "IconBrush", Type = "Brush", DefaultValue = "null", Description = "WPF 兼容属性。作为图标颜色别名，适合现有 Data 图标的前景色绑定。" }
        ];

        IconWrapperPropsRows =
        [
            new ApiDocRow { Name = "BorderRadius", Type = "number", DefaultValue = "6", Description = "边框圆角大小。" },
            new ApiDocRow { Name = "Color", Type = "Brush | string", DefaultValue = "主题 Primary 色", Description = "背景颜色。" },
            new ApiDocRow { Name = "IconColor", Type = "Brush | string", DefaultValue = "light: #FFFFFF, dark: #000000", Description = "包裹区域内图标的默认颜色。" },
            new ApiDocRow { Name = "Size", Type = "number | string", DefaultValue = "24", Description = "包裹容器的宽高尺寸。未指定单位时按 px 处理。" }
        ];

        IconSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "图标的内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> IconPropsRows { get; }

    public IReadOnlyList<ApiDocRow> IconWrapperPropsRows { get; }

    public IReadOnlyList<ApiDocRow> IconSlotsRows { get; }
}
