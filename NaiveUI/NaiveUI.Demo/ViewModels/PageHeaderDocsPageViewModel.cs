using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public partial class PageHeaderDocsPageViewModel : ObservableObject
{
    public PageHeaderDocsPageViewModel()
    {
        BreadcrumbCommand = new RelayCommand<object?>(HandleBreadcrumbCommand);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("API", "SectionApi"),
            ("PageHeader Props", "SectionPageHeaderProps"),
            ("PageHeader Slots", "SectionPageHeaderSlots"));

        BackCommand = new RelayCommand(HandleBack);

        PageHeaderPropsRows =
        [
            new ApiDocRow { Name = "Title", Type = "string", DefaultValue = "null", Description = "主标题。当设置 TitleContent 时该属性会被对应插槽内容覆盖。" },
            new ApiDocRow { Name = "Subtitle", Type = "string", DefaultValue = "null", Description = "副标题。当设置 SubtitleContent 时该属性会被对应插槽内容覆盖。" },
            new ApiDocRow { Name = "Extra", Type = "string", DefaultValue = "null", Description = "额外的文本信息。当设置 ExtraContent 时该属性会被对应插槽内容覆盖。" },
            new ApiDocRow { Name = "OnBack", Type = "ICommand", DefaultValue = "null", Description = "点击返回按钮时执行的命令，对应 Naive UI 的 on-back。WPF 版本另外提供 Back 事件给代码后置使用。" },
            new ApiDocRow { Name = "HeaderContent", Type = "object", DefaultValue = "null", Description = "头部信息内容，对应 header 插槽。" },
            new ApiDocRow { Name = "AvatarContent", Type = "object", DefaultValue = "null", Description = "头像区域内容，对应 avatar 插槽。" },
            new ApiDocRow { Name = "TitleContent", Type = "object", DefaultValue = "null", Description = "标题区域内容，对应 title 插槽。" },
            new ApiDocRow { Name = "SubtitleContent", Type = "object", DefaultValue = "null", Description = "副标题区域内容，对应 subtitle 插槽。" },
            new ApiDocRow { Name = "ExtraContent", Type = "object", DefaultValue = "null", Description = "额外操作区域内容，对应 extra 插槽。" },
            new ApiDocRow { Name = "FooterContent", Type = "object", DefaultValue = "null", Description = "底部信息内容，对应 footer 插槽。" },
            new ApiDocRow { Name = "BackContent", Type = "object", DefaultValue = "null", Description = "返回图标内容，对应 back 插槽。未设置时使用内置返回箭头。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "主体内容区域，对应默认插槽。" }
        ];

        PageHeaderSlotsRows =
        [
            new ApiDocRow { Name = "avatar", Type = "()", DefaultValue = "-", Description = "图片信息。" },
            new ApiDocRow { Name = "header", Type = "()", DefaultValue = "-", Description = "头部信息。" },
            new ApiDocRow { Name = "default", Type = "()", DefaultValue = "-", Description = "内容。" },
            new ApiDocRow { Name = "extra", Type = "()", DefaultValue = "-", Description = "额外信息。" },
            new ApiDocRow { Name = "footer", Type = "()", DefaultValue = "-", Description = "底部信息。" },
            new ApiDocRow { Name = "subtitle", Type = "()", DefaultValue = "-", Description = "副标题信息。" },
            new ApiDocRow { Name = "title", Type = "()", DefaultValue = "-", Description = "标题信息。" },
            new ApiDocRow { Name = "back", Type = "()", DefaultValue = "-", Description = "返回图标。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> PageHeaderPropsRows { get; }

    public IReadOnlyList<ApiDocRow> PageHeaderSlotsRows { get; }

    public ICommand BackCommand { get; }

    private static void HandleBack()
    {
        NElMessage.Info("Command [onBack]");
    }
    public ICommand BreadcrumbCommand { get; }

    private void HandleBreadcrumbCommand(object? parameter)
    {
        var message = parameter switch
        {
            string text when !string.IsNullOrWhiteSpace(text) => $"Last command: {text}",
            _ => "Last command: null"
        };

        NElMessage.Info(message);
    }
}
