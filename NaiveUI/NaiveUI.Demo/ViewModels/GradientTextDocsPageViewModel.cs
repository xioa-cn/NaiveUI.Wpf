using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class GradientTextDocsPageViewModel : ViewModelBase
{
    public GradientTextDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("尺寸", "SectionSize"),
            ("定制", "SectionCustom"),
            ("API", "SectionApi"),
            ("GradientText Props", "SectionGradientTextProps"),
            ("GradientTextGradient Props", "SectionGradientTextGradientProps"),
            ("GradientText Slots", "SectionGradientTextSlots"));

        GradientTextPropsRows =
        [
            new ApiDocRow { Name = "Gradient", Type = "string | NGradientTextGradient", DefaultValue = "null", Description = "文字渐变色参数。支持 linear-gradient(...) 字符串，例如 linear-gradient(90deg, rgb(85, 85, 85) 0%, rgb(170, 170, 170) 100%)，或通过 From / To / Deg 对象定义渐变。" },
            new ApiDocRow { Name = "Size", Type = "number | string", DefaultValue = "null", Description = "文字大小。当传入不带单位的字符串或数值时，按 px 处理。" },
            new ApiDocRow { Name = "Type", Type = "NGradientTextType", DefaultValue = "Primary", Description = "渐变文字的类型。支持 Primary、Info、Success、Warning、Error，另外兼容 Danger 并映射为 Error。" }
        ];

        GradientTextGradientPropsRows =
        [
            new ApiDocRow { Name = "From", Type = "string", DefaultValue = "\"\"", Description = "渐变起点颜色。支持 rgb(...)、rgba(...)、hex 或颜色名称等 WPF 可解析的颜色字符串。" },
            new ApiDocRow { Name = "To", Type = "string", DefaultValue = "\"\"", Description = "渐变终点颜色。写法与 From 一致。" },
            new ApiDocRow { Name = "Deg", Type = "double", DefaultValue = "0", Description = "渐变角度，单位为度。对应 linear-gradient(<deg>deg, from 0%, to 100%) 的写法。" }
        ];

        GradientTextSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "渐变文字的内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> GradientTextPropsRows { get; }

    public IReadOnlyList<ApiDocRow> GradientTextGradientPropsRows { get; }

    public IReadOnlyList<ApiDocRow> GradientTextSlotsRows { get; }
}
