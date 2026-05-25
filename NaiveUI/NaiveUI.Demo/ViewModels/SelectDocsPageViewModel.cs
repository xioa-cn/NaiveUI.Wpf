using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class SelectDocsPageViewModel : ViewModelBase
{
    private object? basicValue = "song0";
    private object? clearableValue = "song2";
    private object? searchableValue;
    private object? sizeValue = "medium";
    private object? statusValue;
    private object? templatedValue = "tokyo";
    private string eventText = "当前尚未触发选择。";
    private string searchText = string.Empty;
    private ObservableCollection<object?> multipleValues = ["song0", "song3"];
    private ObservableCollection<object?> tagValues = ["vue", "wpf"];

    public SelectDocsPageViewModel(string selectedKey = "select")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("尺寸", "SectionSize"),
            ("可清除", "SectionClearable"),
            ("多选", "SectionMultiple"),
            ("可搜索", "SectionFilterable"),
            ("禁用与加载", "SectionState"),
            ("状态", "SectionStatus"),
            ("自定义渲染", "SectionTemplate"),
            ("ItemsSource", "SectionItemsSource"),
            ("接口说明", "SectionApi"),
            ("Select 属性", "SectionSelectProps"),
            ("Option 属性", "SectionOptionProps"),
            ("事件", "SectionEvents"));

        SongOptions =
        [
            new SelectDemoOption("Drive My Car", "song0", "The Beatles", "1965"),
            new SelectDemoOption("Norwegian Wood", "song1", "The Beatles", "1965"),
            new SelectDemoOption("You Won't See Me", "song2", "The Beatles", "1965"),
            new SelectDemoOption("Nowhere Man", "song3", "The Beatles", "1965"),
            new SelectDemoOption("Think For Yourself", "song4", "The Beatles", "1965"),
            new SelectDemoOption("The Word", "song5", "The Beatles", "1965")
        ];

        CityOptions =
        [
            new SelectDemoOption("东京", "tokyo", "日本", "JST"),
            new SelectDemoOption("伦敦", "london", "英国", "GMT"),
            new SelectDemoOption("巴黎", "paris", "法国", "CET"),
            new SelectDemoOption("纽约", "new-york", "美国", "EST")
        ];

        SelectPropsRows =
        [
            new ApiDocRow { Name = "Options", Type = "ObservableCollection<NSelectOption>", DefaultValue = "[]", Description = "声明式选项集合，对应 Naive UI 的 options。" },
            new ApiDocRow { Name = "ItemsSource", Type = "IEnumerable", DefaultValue = "null", Description = "绑定外部数据源，配合 DisplayMemberPath 和 ValueMemberPath 使用。" },
            new ApiDocRow { Name = "SelectedValue", Type = "object", DefaultValue = "null", Description = "单选值，默认双向绑定，对应 value。" },
            new ApiDocRow { Name = "SelectedValues", Type = "IList", DefaultValue = "null", Description = "多选值集合，Multiple 为 true 时使用。" },
            new ApiDocRow { Name = "Multiple", Type = "bool", DefaultValue = "false", Description = "是否启用多选模式。" },
            new ApiDocRow { Name = "Filterable", Type = "bool", DefaultValue = "false", Description = "是否显示搜索输入并按标签和值过滤。" },
            new ApiDocRow { Name = "Clearable", Type = "bool", DefaultValue = "false", Description = "是否在有值时显示清除按钮。" },
            new ApiDocRow { Name = "Loading", Type = "bool", DefaultValue = "false", Description = "是否进入加载态。" },
            new ApiDocRow { Name = "Remote", Type = "bool", DefaultValue = "false", Description = "远程搜索标记，当前保留扩展语义，搜索文本可双向绑定后自行加载数据。" },
            new ApiDocRow { Name = "AllowCreate", Type = "bool", DefaultValue = "false", Description = "过滤无匹配时允许用搜索文本临时创建选项。" },
            new ApiDocRow { Name = "HideSelected", Type = "bool", DefaultValue = "false", Description = "是否在下拉列表中隐藏已选项。" },
            new ApiDocRow { Name = "CloseOnSelect", Type = "bool", DefaultValue = "true", Description = "单选选择后是否关闭弹层；多选默认保持弹层打开。" },
            new ApiDocRow { Name = "Placeholder", Type = "string", DefaultValue = "请选择", Description = "占位文本。" },
            new ApiDocRow { Name = "SearchText", Type = "string", DefaultValue = "string.Empty", Description = "搜索文本，支持双向绑定。" },
            new ApiDocRow { Name = "Size", Type = "NSelectSize", DefaultValue = "Medium", Description = "尺寸，可选 Tiny、Small、Medium、Large。" },
            new ApiDocRow { Name = "Status", Type = "NSelectStatus", DefaultValue = "Default", Description = "语义状态，可选 Default、Success、Warning、Error。" },
            new ApiDocRow { Name = "IsInvalid", Type = "bool", DefaultValue = "false", Description = "错误态快捷属性，会使用错误边框。" },
            new ApiDocRow { Name = "MaxDropDownHeight", Type = "double", DefaultValue = "280", Description = "下拉面板最大高度。" },
            new ApiDocRow { Name = "OptionTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "选项渲染模板，对应 render-option / render-label 能力。" },
            new ApiDocRow { Name = "SelectedItemTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "单选已选内容模板。" },
            new ApiDocRow { Name = "TagTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "多选标签文本模板。" }
        ];

        OptionPropsRows =
        [
            new ApiDocRow { Name = "Label", Type = "object", DefaultValue = "null", Description = "显示内容，可放文本或 UI 对象。" },
            new ApiDocRow { Name = "Value", Type = "object", DefaultValue = "null", Description = "选项值。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "是否禁用当前选项。" },
            new ApiDocRow { Name = "Show", Type = "bool", DefaultValue = "true", Description = "是否显示当前选项。" },
            new ApiDocRow { Name = "Icon", Type = "object", DefaultValue = "null", Description = "选项左侧图标内容。" },
            new ApiDocRow { Name = "Suffix", Type = "object", DefaultValue = "null", Description = "选项右侧补充内容。" }
        ];

        EventRows =
        [
            new ApiDocRow { Name = "SelectionChanged", Type = "RoutedEvent", DefaultValue = "-", Description = "选择变化事件，参数包含 OldValue 和 NewValue。" },
            new ApiDocRow { Name = "Clear", Type = "RoutedEvent", DefaultValue = "-", Description = "清除按钮触发后抛出。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public ObservableCollection<SelectDemoOption> SongOptions { get; }

    public ObservableCollection<SelectDemoOption> CityOptions { get; }

    public IReadOnlyList<ApiDocRow> SelectPropsRows { get; }

    public IReadOnlyList<ApiDocRow> OptionPropsRows { get; }

    public IReadOnlyList<ApiDocRow> EventRows { get; }

    public object? BasicValue
    {
        get => basicValue;
        set => SetProperty(ref basicValue, value);
    }

    public object? ClearableValue
    {
        get => clearableValue;
        set => SetProperty(ref clearableValue, value);
    }

    public object? SearchableValue
    {
        get => searchableValue;
        set => SetProperty(ref searchableValue, value);
    }

    public object? SizeValue
    {
        get => sizeValue;
        set => SetProperty(ref sizeValue, value);
    }

    public object? StatusValue
    {
        get => statusValue;
        set => SetProperty(ref statusValue, value);
    }

    public object? TemplatedValue
    {
        get => templatedValue;
        set => SetProperty(ref templatedValue, value);
    }

    public ObservableCollection<object?> MultipleValues
    {
        get => multipleValues;
        set => SetProperty(ref multipleValues, value);
    }

    public ObservableCollection<object?> TagValues
    {
        get => tagValues;
        set => SetProperty(ref tagValues, value);
    }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    public string EventText
    {
        get => eventText;
        set => SetProperty(ref eventText, value);
    }

    public void RecordSelection(object? oldValue, object? newValue)
    {
        EventText = $"选择变化：{FormatValue(oldValue)} -> {FormatValue(newValue)}";
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (value is System.Collections.IEnumerable enumerable and not string)
        {
            return string.Join(", ", enumerable.Cast<object?>().Select(item => item?.ToString() ?? "null"));
        }

        return value.ToString() ?? "null";
    }
}

public sealed class SelectDemoOption
{
    public SelectDemoOption(string label, string value, string group, string suffix)
    {
        Label = label;
        Value = value;
        Group = group;
        Suffix = suffix;
    }

    public string Label { get; }

    public string Value { get; }

    public string Group { get; }

    public string Suffix { get; }
}
