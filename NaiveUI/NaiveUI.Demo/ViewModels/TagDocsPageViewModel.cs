using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class TagDocsPageViewModel : ViewModelBase
{
    private bool disabledTags = true;
    private bool sharedChecked;

    public TagDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("无边框", "SectionBordered"),
            ("可关闭", "SectionClosable"),
            ("禁用", "SectionDisabled"),
            ("尺寸", "SectionSize"),
            ("可选择", "SectionCheckable"),
            ("形状", "SectionShape"),
            ("颜色", "SectionColor"),
            ("头像", "SectionAvatar"),
            ("图标", "SectionIcon"),
            ("API", "SectionApi"),
            ("Tag Props", "SectionTagProps"),
            ("TagColor Props", "SectionTagColorProps"),
            ("Tag Slots", "SectionTagSlots"));

        TagPropsRows =
        [
            new ApiDocRow { Name = "Bordered", Type = "bool", DefaultValue = "true", Description = "是否有边框。" },
            new ApiDocRow { Name = "Checkable", Type = "bool", DefaultValue = "false", Description = "是否可以选择。启用后 Type 和 Color 的视觉语义将不再生效。" },
            new ApiDocRow { Name = "Checked", Type = "bool", DefaultValue = "false", Description = "当前是否选中。配合 Checkable 使用，WPF 版本默认支持双向绑定。" },
            new ApiDocRow { Name = "Closable", Type = "bool", DefaultValue = "false", Description = "是否可关闭。" },
            new ApiDocRow { Name = "Color", Type = "NTagColor", DefaultValue = "null", Description = "标签颜色配置。可分别定制背景、边框和文字颜色；设置后会覆盖当前 Type 的对应颜色。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用。" },
            new ApiDocRow { Name = "Round", Type = "bool", DefaultValue = "false", Description = "是否圆角。" },
            new ApiDocRow { Name = "Size", Type = "NTagSize", DefaultValue = "Medium", Description = "尺寸。支持 Tiny、Small、Medium、Large。" },
            new ApiDocRow { Name = "Strong", Type = "bool", DefaultValue = "false", Description = "文字是否加粗。" },
            new ApiDocRow { Name = "TriggerClickOnClose", Type = "bool", DefaultValue = "false", Description = "点击关闭按钮时是否同时触发 Click。" },
            new ApiDocRow { Name = "Type", Type = "NTagType", DefaultValue = "Default", Description = "类型。支持 Default、Primary、Info、Success、Warning、Error。" },
            new ApiDocRow { Name = "AvatarContent", Type = "object", DefaultValue = "null", Description = "WPF 版本对 avatar 插槽的映射。" },
            new ApiDocRow { Name = "IconContent", Type = "object", DefaultValue = "null", Description = "WPF 版本对 icon 插槽的映射。" },
            new ApiDocRow { Name = "Click", Type = "event RoutedEventHandler", DefaultValue = "null", Description = "点击标签时触发。" },
            new ApiDocRow { Name = "Close", Type = "event RoutedEventHandler", DefaultValue = "null", Description = "点击关闭按钮时触发。" },
            new ApiDocRow { Name = "CheckedChanged", Type = "event RoutedPropertyChangedEventHandler<bool>", DefaultValue = "null", Description = "选择状态变更时触发。" },
            new ApiDocRow { Name = "TextFontSize", Type = "double", DefaultValue = "NaN", Description = "可选的文本字号重写值。当值为非数值（NaN）时，组件将采用由尺寸规格（Size）映射而来的字号" }
        ];

        TagColorPropsRows =
        [
            new ApiDocRow { Name = "Color", Type = "string", DefaultValue = "null", Description = "标签背景色，例如 #BBB。" },
            new ApiDocRow { Name = "BorderColor", Type = "string", DefaultValue = "null", Description = "标签边框颜色，例如 #555。" },
            new ApiDocRow { Name = "TextColor", Type = "string", DefaultValue = "null", Description = "标签文字颜色，例如 #555。" }
        ];

        TagSlotsRows =
        [
            new ApiDocRow { Name = "Avatar", Type = "()", DefaultValue = "-", Description = "标签中的头像。WPF 中可通过 AvatarContent 属性元素设置。" },
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "标签内容。" },
            new ApiDocRow { Name = "Icon", Type = "()", DefaultValue = "-", Description = "标签中的图标。WPF 中可通过 IconContent 属性元素设置。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> TagPropsRows { get; }

    public IReadOnlyList<ApiDocRow> TagColorPropsRows { get; }

    public IReadOnlyList<ApiDocRow> TagSlotsRows { get; }

    public bool DisabledTags
    {
        get => disabledTags;
        set
        {
            SetProperty(ref disabledTags, value);
            OnPropertyChanged(nameof(DisabledTagsDescription));
        }
    }

    public bool SharedChecked
    {
        get => sharedChecked;
        set
        {
            SetProperty(ref sharedChecked, value);
            OnPropertyChanged(nameof(SharedCheckedDescription));
        }
    }

    public string DisabledTagsDescription => DisabledTags ? "当前禁用状态：开启" : "当前禁用状态：关闭";

    public string SharedCheckedDescription => SharedChecked ? "当前 checked: true" : "当前 checked: false";
}
