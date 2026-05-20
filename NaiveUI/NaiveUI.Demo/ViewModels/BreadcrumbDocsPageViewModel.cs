using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public sealed class BreadcrumbDocsPageViewModel : ViewModelBase
{
    private string breadcrumbCommandText = "Last command: none.";

    public BreadcrumbDocsPageViewModel()
    {
        BreadcrumbCommand = new RelayCommand<object?>(HandleBreadcrumbCommand);

        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("下拉菜单", "SectionCustom"),
            ("自定义分隔符", "SectionSeparator"),
            ("自定义每项分隔符", "SectionSeparatorPerItem"),
            ("API", "SectionApi"),
            ("Breadcrumb Props", "SectionBreadcrumbProps"),
            ("BreadcrumbItem Props", "SectionBreadcrumbItemProps"),
            ("Breadcrumb Slots", "SectionBreadcrumbSlots"),
            ("Breadcrumb Item Slots", "SectionBreadcrumbItemSlots"));

        BreadcrumbPropsRows =
        [
            new ApiDocRow { Name = "Separator", Type = "string", DefaultValue = "\"/\"", Description = "面包屑之间的分隔符。" }
        ];

        BreadcrumbItemPropsRows =
        [
            new ApiDocRow { Name = "Clickable", Type = "bool", DefaultValue = "true", Description = "是否可点击。" },
            new ApiDocRow { Name = "Href", Type = "string", DefaultValue = "null", Description = "链接地址。设置后点击项时会尝试使用系统默认程序打开该地址。" },
            new ApiDocRow { Name = "Separator", Type = "string", DefaultValue = "null", Description = "当前子项的分隔符。未设置时回退到父级 NBreadcrumb 的 Separator。" },
            new ApiDocRow { Name = "ShowSeparator", Type = "bool", DefaultValue = "true", Description = "是否显示分隔符。" },
            new ApiDocRow { Name = "SeparatorContent", Type = "object", DefaultValue = "null", Description = "WPF 版本用于映射 separator 插槽。设置后优先级高于 Separator。" },
            new ApiDocRow { Name = "Command", Type = "ICommand", DefaultValue = "null", Description = "绑定当前面包屑项命令，适合在 MVVM 中直接处理导航或跳转。" },
            new ApiDocRow { Name = "CommandParameter", Type = "object", DefaultValue = "null", Description = "传递给 Command 的参数。" },
            new ApiDocRow { Name = "Click", Type = "event RoutedEventHandler", DefaultValue = "null", Description = "当前项被点击时触发的事件，对应 Naive UI 的 onClick 回调；WPF 中更推荐优先使用 Command。" }
        ];

        BreadcrumbSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "Breadcrumb 默认填充的内容。" }
        ];

        BreadcrumbItemSlotsRows =
        [
            new ApiDocRow { Name = "Default", Type = "()", DefaultValue = "-", Description = "BreadcrumbItem 默认填充的内容。" },
            new ApiDocRow { Name = "Separator", Type = "()", DefaultValue = "-", Description = "分隔符填充的内容。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> BreadcrumbPropsRows { get; }

    public IReadOnlyList<ApiDocRow> BreadcrumbItemPropsRows { get; }

    public IReadOnlyList<ApiDocRow> BreadcrumbSlotsRows { get; }

    public IReadOnlyList<ApiDocRow> BreadcrumbItemSlotsRows { get; }

    public string BreadcrumbCommandText
    {
        get => breadcrumbCommandText;
        set => SetProperty(ref breadcrumbCommandText, value);
    }

    public ICommand BreadcrumbCommand { get; }

    private void HandleBreadcrumbCommand(object? parameter)
    {
        var message = parameter switch
        {
            string text when !string.IsNullOrWhiteSpace(text) => $"Last command: {text}",
            _ => "Last command: null"
        };

        BreadcrumbCommandText = message;
        NElMessage.Info(message);
    }
}
