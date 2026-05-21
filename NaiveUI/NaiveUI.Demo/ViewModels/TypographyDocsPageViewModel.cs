using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public sealed class TypographyDocsPageViewModel : ViewModelBase
{
    public TypographyDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("标题", "SectionHeader"),
            ("标签", "SectionTags"),
            ("文本", "SectionText"),
            ("Router Link", "SectionRouterLink"),
            ("API", "SectionApi"),
            ("Text Props", "SectionTextProps"),
            ("A Props", "SectionAProps"),
            ("P Props", "SectionPProps"),
            ("H1, H2, H3, H4, H5, H6 Props", "SectionHeaderProps"),
            ("Ul, Ol Props", "SectionListProps"),
            ("Blockquote Props", "SectionBlockquoteProps"),
            ("All Typography Components Slots", "SectionSlots"));

        RouterLinkCommand = new RelayCommand<object?>(HandleRouterLinkCommand);

        TextPropsRows =
        [
            new ApiDocRow { Name = "Type", Type = "NTypographyType", DefaultValue = "Default", Description = "排印类型。支持 Default、Success、Info、Warning、Error。" },
            new ApiDocRow { Name = "Strong", Type = "bool", DefaultValue = "false", Description = "是否使用粗体字重。" },
            new ApiDocRow { Name = "Italic", Type = "bool", DefaultValue = "false", Description = "是否使用斜体。" },
            new ApiDocRow { Name = "Underline", Type = "bool", DefaultValue = "false", Description = "是否显示下划线。" },
            new ApiDocRow { Name = "Delete", Type = "bool", DefaultValue = "false", Description = "是否显示删除线。" },
            new ApiDocRow { Name = "Code", Type = "bool", DefaultValue = "false", Description = "是否启用代码样式。" },
            new ApiDocRow { Name = "Depth", Type = "int", DefaultValue = "0", Description = "文字深度。0 表示默认；1、2、3 分别对应 primary / secondary / tertiary depth。" },
            new ApiDocRow { Name = "Tag", Type = "object", DefaultValue = "null", Description = "复用 WPF 原生 FrameworkElement.Tag 属性承载官方的 tag 语义，例如 Tag=\"div\"。该属性主要用于语义映射，不影响既有 WPF 布局机制。" }
        ];

        APropsRows =
        [
            new ApiDocRow { Name = "Href", Type = "string", DefaultValue = "null", Description = "链接地址。设置后点击会使用系统默认方式打开。" },
            new ApiDocRow { Name = "Target", Type = "string", DefaultValue = "null", Description = "与 Web 端 target 保持同名，便于迁移。WPF 中主要作为语义保留。" },
            new ApiDocRow { Name = "Clickable", Type = "bool", DefaultValue = "true", Description = "是否允许触发点击行为。" },
            new ApiDocRow { Name = "Command", Type = "ICommand", DefaultValue = "null", Description = "更符合 WPF 使用习惯的命令入口。" },
            new ApiDocRow { Name = "CommandParameter", Type = "object", DefaultValue = "null", Description = "命令参数。" }
        ];

        PPropsRows =
        [
            new ApiDocRow { Name = "Depth", Type = "int", DefaultValue = "0", Description = "段落文字深度。0 表示默认，1、2、3 对应不同深浅层级。" }
        ];

        HeaderPropsRows =
        [
            new ApiDocRow { Name = "AlignText", Type = "bool", DefaultValue = "false", Description = "是否让前缀条块按文本高度对齐。" },
            new ApiDocRow { Name = "Type", Type = "NTypographyType", DefaultValue = "Default", Description = "标题排印类型。" },
            new ApiDocRow { Name = "Prefix", Type = "NTypographyHeaderPrefix", DefaultValue = "None", Description = "字首前缀。当前支持 Bar，对应官方的 prefix=\"bar\"。" }
        ];

        ListPropsRows =
        [
            new ApiDocRow { Name = "AlignText", Type = "bool", DefaultValue = "false", Description = "是否让项目符号或序号按文本区域对齐。" }
        ];

        BlockquotePropsRows =
        [
            new ApiDocRow { Name = "AlignText", Type = "bool", DefaultValue = "false", Description = "是否让引用左侧条块与文本区域更紧密地对齐。" }
        ];

        SlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "排印内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> TextPropsRows { get; }

    public IReadOnlyList<ApiDocRow> APropsRows { get; }

    public IReadOnlyList<ApiDocRow> PPropsRows { get; }

    public IReadOnlyList<ApiDocRow> HeaderPropsRows { get; }

    public IReadOnlyList<ApiDocRow> ListPropsRows { get; }

    public IReadOnlyList<ApiDocRow> BlockquotePropsRows { get; }

    public IReadOnlyList<ApiDocRow> SlotsRows { get; }

    public ICommand RouterLinkCommand { get; }

    private static void HandleRouterLinkCommand(object? parameter)
    {
        var target = parameter switch
        {
            string text when !string.IsNullOrWhiteSpace(text) => text,
            _ => "/"
        };

        NElMessage.Info($"Navigate: {target}");
    }
}
