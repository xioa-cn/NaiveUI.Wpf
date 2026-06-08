using System.Collections.Generic;
using NaiveUI.Demo.Models;

namespace NaiveUI.Demo.ViewModels;

public sealed class InputDocsPageViewModel : ViewModelBase
{
    private string basicText = string.Empty;
    private string clearableText = "可以清空的内容";
    private string passwordText = "naive-ui-wpf";
    private string textareaText = "NaiveUI.Wpf 让 WPF 输入框也能拥有接近 Naive UI 的视觉、状态和交互细节。";
    private string eventText = "最近一次输入：暂无";

    public InputDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("尺寸", "SectionSize"),
            ("状态", "SectionStatus"),
            ("可清空", "SectionClearable"),
            ("密码", "SectionPassword"),
            ("前后缀", "SectionAffix"),
            ("计数", "SectionCount"),
            ("文本域", "SectionTextarea"),
            ("加载与只读", "SectionLoading"),
            ("事件", "SectionEvent"),
            ("API", "SectionApi"));

        InputPropsRows =
        [
            new ApiDocRow { Name = "Text", Type = "string", DefaultValue = "\"\"", Description = "当前输入值。默认支持 TwoWay 绑定，对齐 Naive UI 的 value / on-update:value 语义。" },
            new ApiDocRow { Name = "Placeholder", Type = "string", DefaultValue = "\"\"", Description = "占位提示文本。" },
            new ApiDocRow { Name = "Type", Type = "NInputType", DefaultValue = "Text", Description = "输入类型。支持 Text、Password、Textarea。" },
            new ApiDocRow { Name = "Size", Type = "NControlSize", DefaultValue = "Medium", Description = "尺寸。支持 Tiny、Small、Medium、Large。" },
            new ApiDocRow { Name = "Status", Type = "NSelectStatus", DefaultValue = "Default", Description = "状态色。支持 Default、Success、Warning、Error。" },
            new ApiDocRow { Name = "IsInvalid", Type = "bool", DefaultValue = "false", Description = "错误态快捷属性，优先显示错误边框。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用。写法贴近 Naive UI 的 disabled，同时会同步 IsEnabled。" },
            new ApiDocRow { Name = "IsReadOnly", Type = "bool", DefaultValue = "false", Description = "是否只读。保留文本选择和聚焦，但不允许编辑。" },
            new ApiDocRow { Name = "Clearable", Type = "bool", DefaultValue = "false", Description = "是否显示清空按钮。鼠标移入且有内容时显示，行为对齐 Naive UI clearable。" },
            new ApiDocRow { Name = "Loading", Type = "bool", DefaultValue = "false", Description = "是否显示加载指示器。" },
            new ApiDocRow { Name = "Round", Type = "bool", DefaultValue = "false", Description = "是否使用胶囊形圆角。" },
            new ApiDocRow { Name = "ShowCount", Type = "bool", DefaultValue = "false", Description = "是否显示输入长度统计。" },
            new ApiDocRow { Name = "MaxLength / MinLength", Type = "int", DefaultValue = "0", Description = "输入长度限制。MaxLength 为 0 时表示不限制。" },
            new ApiDocRow { Name = "PrefixContent / SuffixContent", Type = "object", DefaultValue = "null", Description = "前缀和后缀内容，对应 Naive UI 的 prefix / suffix slot。" },
            new ApiDocRow { Name = "ShowPasswordToggle", Type = "bool", DefaultValue = "true", Description = "密码类型下是否显示明文切换按钮。" },
            new ApiDocRow { Name = "IsPasswordVisible", Type = "bool", DefaultValue = "false", Description = "密码明文显示状态，可绑定控制。" },
            new ApiDocRow { Name = "Rows", Type = "int", DefaultValue = "3", Description = "Textarea 默认行数。" },
            new ApiDocRow { Name = "Autosize", Type = "bool", DefaultValue = "false", Description = "Textarea 是否按行数范围自动计算高度。" },
            new ApiDocRow { Name = "MinRows / MaxRows", Type = "int", DefaultValue = "1 / 0", Description = "Autosize 时的最小/最大行数。MaxRows 为 0 时使用 Rows。" }
        ];

        InputEventRows =
        [
            new ApiDocRow { Name = "TextChanged", Type = "RoutedPropertyChangedEventHandler<string>", DefaultValue = "-", Description = "Text 变化事件，提供旧值和新值。" },
            new ApiDocRow { Name = "Clear", Type = "RoutedEventHandler", DefaultValue = "-", Description = "点击清空按钮后触发，对齐 Naive UI 的 on-clear。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> InputPropsRows { get; }

    public IReadOnlyList<ApiDocRow> InputEventRows { get; }

    public string BasicText
    {
        get => basicText;
        set => SetProperty(ref basicText, value);
    }

    public string ClearableText
    {
        get => clearableText;
        set => SetProperty(ref clearableText, value);
    }

    public string PasswordText
    {
        get => passwordText;
        set => SetProperty(ref passwordText, value);
    }

    public string TextareaText
    {
        get => textareaText;
        set => SetProperty(ref textareaText, value);
    }

    public string EventText
    {
        get => eventText;
        private set => SetProperty(ref eventText, value);
    }

    public void RecordTextChanged(string value)
    {
        EventText = $"最近一次输入：{(string.IsNullOrEmpty(value) ? "空" : value)}";
    }

    public void RecordClear()
    {
        EventText = "最近一次操作：点击了清空按钮";
    }
}
