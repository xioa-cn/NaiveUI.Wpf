using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.ViewModels;

public sealed class AutoCompleteDocsPageViewModel : ViewModelBase
{
    private readonly IReadOnlyList<SelectDemoOption> songSource;
    private readonly IReadOnlyList<SelectDemoOption> citySource;
    private readonly IReadOnlyList<SelectDemoOption> emailDomainSource;
    private string basicText = string.Empty;
    private object? basicValue;
    private string completionText = "xiaoming";
    private object? completionValue;
    private string valueChangedText = "暂无事件。";
    private string remoteText = string.Empty;
    private object? remoteValue;
    private bool remoteLoading;
    private string focusText = string.Empty;
    private object? focusValue;
    private string focusStatusText = "暂未触发焦点操作。";
    private string templateText = "北京";
    private object? templateValue = "beijing";
    private string statusText = "成功状态";
    private string prefixText = string.Empty;
    private object? prefixValue;
    private string internalFilterText = string.Empty;
    private object? internalFilterValue;
    private string noDataText = string.Empty;

    public AutoCompleteDocsPageViewModel(string selectedKey = "auto-complete")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("补全提示", "SectionCompletion"),
            ("尺寸", "SectionSize"),
            ("状态", "SectionState"),
            ("前后缀", "SectionAffix"),
            ("内置过滤", "SectionInternalFilter"),
            ("远程搜索", "SectionRemote"),
            ("自定义渲染", "SectionTemplate"),
            ("空状态", "SectionEmpty"),
            ("焦点控制", "SectionFocusBlur"),
            ("弹层位置", "SectionPlacement"),
            ("选择行为", "SectionBehavior"),
            ("触发方式", "SectionTrigger"),
            ("过滤细节", "SectionFilter"),
            ("加载与空态", "SectionLoadingEmpty"),
            ("自定义图标", "SectionCustomIcon"),
            ("长文本输入", "SectionLongText"),
            ("API", "SectionApi"),
            ("属性", "SectionProps"),
            ("事件", "SectionEvents"),
            ("方法", "SectionMethods"));

        songSource =
        [
            new SelectDemoOption("Sunny Day", "remote-sunny-day", "Demo", "type sun"),
            new SelectDemoOption("Sunrise Love", "remote-sunrise-love", "Demo", "type love"),
            new SelectDemoOption("Remote Search Demo", "remote-search-demo", "Demo", "type remote"),
            new SelectDemoOption("稻香", "song0", "周杰伦", "2008"),
            new SelectDemoOption("晴天", "song1", "周杰伦", "2003"),
            new SelectDemoOption("起风了", "song2", "买辣椒也用券", "2017"),
            new SelectDemoOption("光年之外", "song3", "邓紫棋", "2016"),
            new SelectDemoOption("平凡之路", "song4", "朴树", "2014"),
            new SelectDemoOption("夜空中最亮的星", "song5", "逃跑计划", "2011")
        ];

        citySource =
        [
            new SelectDemoOption("北京", "beijing", "华北", "京"),
            new SelectDemoOption("上海", "shanghai", "华东", "沪"),
            new SelectDemoOption("广州", "guangzhou", "华南", "粤"),
            new SelectDemoOption("深圳", "shenzhen", "华南", "粤"),
            new SelectDemoOption("杭州", "hangzhou", "华东", "浙"),
            new SelectDemoOption("成都", "chengdu", "西南", "川")
        ];

        emailDomainSource =
        [
            new SelectDemoOption("@qq.com", "@qq.com", "腾讯邮箱", "常用"),
            new SelectDemoOption("@163.com", "@163.com", "网易邮箱", "常用"),
            new SelectDemoOption("@126.com", "@126.com", "网易邮箱", "常用"),
            new SelectDemoOption("@outlook.com", "@outlook.com", "微软邮箱", "国际"),
            new SelectDemoOption("@gmail.com", "@gmail.com", "Google 邮箱", "国际"),
            new SelectDemoOption("@icloud.com", "@icloud.com", "Apple 邮箱", "Apple ID")
        ];

        BasicOptions = [];
        CompletionOptions = [];
        PrefixOptions = [];
        TemplateOptions = [];
        RemoteOptions = [];
        InternalFilterOptions = CreateOptionCollection(citySource);

        UpdateBasicOptions(string.Empty);
        UpdateCompletionOptions(completionText);
        UpdatePrefixOptions(string.Empty);
        UpdateTemplateOptions(templateText);
        ApplyRemoteResults(string.Empty);

        PropsRows =
        [
            new ApiDocRow { Name = "Text", Type = "string", DefaultValue = "string.Empty", Description = "输入框文本，是自动补全的主状态。" },
            new ApiDocRow { Name = "ItemsSource", Type = "IEnumerable", DefaultValue = "null", Description = "候选项来源；默认由调用方根据 Text 生成，贴近 Naive UI 的 options 用法。" },
            new ApiDocRow { Name = "Value", Type = "object", DefaultValue = "null", Description = "选中候选项后提交的值。" },
            new ApiDocRow { Name = "GetShow", Type = "Func<string, bool>", DefaultValue = "null", Description = "等价于 Naive UI get-show，用当前文本决定是否展示下拉面板。" },
            new ApiDocRow { Name = "EnableInternalFilter", Type = "bool", DefaultValue = "false", Description = "可选内置过滤；默认关闭，避免把 AutoComplete 做成 Select。" },
            new ApiDocRow { Name = "ShowCompletionHint", Type = "bool", DefaultValue = "true", Description = "聚焦时显示灰色补全尾巴。" },
            new ApiDocRow { Name = "AcceptSuggestionOnTab", Type = "bool", DefaultValue = "true", Description = "按 Tab 接受当前补全或高亮候选项。" },
            new ApiDocRow { Name = "OpenOnFocus", Type = "bool", DefaultValue = "false", Description = "输入框获得焦点时打开候选面板。" },
            new ApiDocRow { Name = "OpenOnInput", Type = "bool", DefaultValue = "true", Description = "输入时打开候选面板。" },
            new ApiDocRow { Name = "SelectOnEnter", Type = "bool", DefaultValue = "true", Description = "按 Enter 提交高亮候选项。" },
            new ApiDocRow { Name = "SelectFirstOptionOnOpen", Type = "bool", DefaultValue = "false", Description = "打开面板或候选项刷新后自动高亮第一个可用项。" },
            new ApiDocRow { Name = "CloseOnSelect", Type = "bool", DefaultValue = "true", Description = "选择后关闭下拉面板。" },
            new ApiDocRow { Name = "BlurAfterSelect", Type = "bool", DefaultValue = "false", Description = "选择后让输入框失焦。" },
            new ApiDocRow { Name = "ClearAfterSelect", Type = "bool", DefaultValue = "false", Description = "选择后清空输入框文本。" },
            new ApiDocRow { Name = "UpdateTextOnSelect", Type = "bool", DefaultValue = "true", Description = "选择后用候选项标签回填 Text。" },
            new ApiDocRow { Name = "Clearable", Type = "bool", DefaultValue = "false", Description = "悬浮或聚焦时显示清除按钮。" },
            new ApiDocRow { Name = "Loading", Type = "bool", DefaultValue = "false", Description = "候选面板显示加载状态。" },
            new ApiDocRow { Name = "Placeholder", Type = "string", DefaultValue = "string.Empty", Description = "输入框占位文本。" },
            new ApiDocRow { Name = "PrefixContent", Type = "object", DefaultValue = "null", Description = "输入框前缀内容。" },
            new ApiDocRow { Name = "SuffixContent", Type = "object", DefaultValue = "null", Description = "输入框后缀内容。" },
            new ApiDocRow { Name = "HeaderContent", Type = "object", DefaultValue = "null", Description = "下拉面板头部内容。" },
            new ApiDocRow { Name = "ActionContent", Type = "object", DefaultValue = "null", Description = "下拉面板头部右侧操作内容。" },
            new ApiDocRow { Name = "OptionTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "候选项自定义模板。" },
            new ApiDocRow { Name = "NoDataContent", Type = "object", DefaultValue = "\"暂无匹配项\"", Description = "无候选项时显示的内容。" },
            new ApiDocRow { Name = "LoadingContent", Type = "object", DefaultValue = "\"加载中\"", Description = "Loading 为 true 时显示的内容。" },
            new ApiDocRow { Name = "Size", Type = "NSelectSize", DefaultValue = "Medium", Description = "Tiny / Small / Medium / Large 四种尺寸。" },
            new ApiDocRow { Name = "Status", Type = "NSelectStatus", DefaultValue = "Default", Description = "Success / Warning / Error 语义状态。" },
            new ApiDocRow { Name = "IsInvalid", Type = "bool", DefaultValue = "false", Description = "快速错误状态。" }
        ];

        EventRows =
        [
            new ApiDocRow { Name = "ValueChanged", Type = "RoutedEvent", DefaultValue = "-", Description = "文本变化或选择提交后触发。" },
            new ApiDocRow { Name = "OptionSelected", Type = "RoutedEvent", DefaultValue = "-", Description = "候选项被选中后触发。" },
            new ApiDocRow { Name = "Clear", Type = "RoutedEvent", DefaultValue = "-", Description = "点击清除按钮后触发。" }
        ];

        MethodRows =
        [
            new ApiDocRow { Name = "Focus()", Type = "bool", DefaultValue = "-", Description = "聚焦内部输入框。" },
            new ApiDocRow { Name = "Blur()", Type = "void", DefaultValue = "-", Description = "关闭面板并移走焦点。" },
            new ApiDocRow { Name = "ClearSelection()", Type = "void", DefaultValue = "-", Description = "清空已提交值和文本。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public ObservableCollection<NSelectOption> BasicOptions
    {
        get;
        private set;
    }

    public ObservableCollection<NSelectOption> CompletionOptions
    {
        get;
        private set;
    }

    public ObservableCollection<NSelectOption> PrefixOptions
    {
        get;
        private set;
    }

    public ObservableCollection<NSelectOption> TemplateOptions
    {
        get;
        private set;
    }

    public ObservableCollection<NSelectOption> RemoteOptions
    {
        get;
        private set;
    }

    public ObservableCollection<NSelectOption> InternalFilterOptions { get; }

    public IReadOnlyList<ApiDocRow> PropsRows { get; }

    public IReadOnlyList<ApiDocRow> EventRows { get; }

    public IReadOnlyList<ApiDocRow> MethodRows { get; }

    public string BasicText
    {
        get => basicText;
        set
        {
            SetProperty(ref basicText, value);
            UpdateBasicOptions(value);
        }
    }

    public object? BasicValue
    {
        get => basicValue;
        set => SetProperty(ref basicValue, value);
    }

    public string CompletionText
    {
        get => completionText;
        set
        {
            SetProperty(ref completionText, value);
            UpdateCompletionOptions(value);
        }
    }

    public object? CompletionValue
    {
        get => completionValue;
        set => SetProperty(ref completionValue, value);
    }

    public string ValueChangedText
    {
        get => valueChangedText;
        set => SetProperty(ref valueChangedText, value);
    }

    public string RemoteText
    {
        get => remoteText;
        set => SetProperty(ref remoteText, value);
    }

    public object? RemoteValue
    {
        get => remoteValue;
        set => SetProperty(ref remoteValue, value);
    }

    public bool RemoteLoading
    {
        get => remoteLoading;
        set => SetProperty(ref remoteLoading, value);
    }

    public string FocusText
    {
        get => focusText;
        set => SetProperty(ref focusText, value);
    }

    public object? FocusValue
    {
        get => focusValue;
        set => SetProperty(ref focusValue, value);
    }

    public string FocusStatusText
    {
        get => focusStatusText;
        set => SetProperty(ref focusStatusText, value);
    }

    public string TemplateText
    {
        get => templateText;
        set
        {
            SetProperty(ref templateText, value);
            UpdateTemplateOptions(value);
        }
    }

    public object? TemplateValue
    {
        get => templateValue;
        set => SetProperty(ref templateValue, value);
    }

    public string StatusText
    {
        get => statusText;
        set => SetProperty(ref statusText, value);
    }

    public string PrefixText
    {
        get => prefixText;
        set
        {
            SetProperty(ref prefixText, value);
            UpdatePrefixOptions(value);
        }
    }

    public object? PrefixValue
    {
        get => prefixValue;
        set => SetProperty(ref prefixValue, value);
    }

    public string InternalFilterText
    {
        get => internalFilterText;
        set => SetProperty(ref internalFilterText, value);
    }

    public object? InternalFilterValue
    {
        get => internalFilterValue;
        set => SetProperty(ref internalFilterValue, value);
    }

    public string NoDataText
    {
        get => noDataText;
        set => SetProperty(ref noDataText, value);
    }

    public void UpdateBasicOptions(string query)
    {
        BasicOptions = CreateOptionCollection(BuildEmailSuffixOptions(query));
        OnPropertyChanged(nameof(BasicOptions));
    }

    public void UpdatePrefixOptions(string query)
    {
        PrefixOptions = CreateOptionCollection(FilterByLabel(citySource, query));
        OnPropertyChanged(nameof(PrefixOptions));
    }

    public void UpdateCompletionOptions(string query)
    {
        CompletionOptions = CreateOptionCollection(BuildEmailSuffixOptions(query));
        OnPropertyChanged(nameof(CompletionOptions));
    }

    public void UpdateTemplateOptions(string query)
    {
        TemplateOptions = CreateOptionCollection(FilterByLabel(citySource, query));
        OnPropertyChanged(nameof(TemplateOptions));
    }

    public void RecordValueChanged(string oldText, string newText, object? oldValue, object? newValue)
    {
        ValueChangedText = $"ValueChanged：文本 {FormatValue(oldText)} -> {FormatValue(newText)}，值 {FormatValue(oldValue)} -> {FormatValue(newValue)}";
    }

    public void RecordOptionSelected(string label, object? value)
    {
        ValueChangedText = $"OptionSelected：{label} ({FormatValue(value)})";
    }

    public void RecordFocusAction(string action)
    {
        FocusStatusText = $"最近操作：{action}，时间 {DateTime.Now:HH:mm:ss}";
    }

    public void ApplyRemoteResults(string query)
    {
        RemoteOptions = CreateOptionCollection(FilterByLabel(songSource, query).Take(4));
        OnPropertyChanged(nameof(RemoteOptions));
    }

    private static IEnumerable<SelectDemoOption> FilterByLabel(IEnumerable<SelectDemoOption> source, string query)
    {
        var keyword = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return source.Take(6);
        }

        return source.Where(item =>
            item.Label.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
            item.Group.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
    }

    private IEnumerable<SelectDemoOption> BuildEmailSuffixOptions(string query)
    {
        var keyword = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        var atIndex = keyword.IndexOf('@');
        if (atIndex >= 0)
        {
            var local = keyword[..atIndex];
            var typedDomain = keyword[(atIndex + 1)..];

            return emailDomainSource
                .Where(item => item.Label.TrimStart('@').StartsWith(typedDomain, StringComparison.CurrentCultureIgnoreCase))
                .Select(item => new SelectDemoOption($"{local}{item.Label}", $"{local}{item.Label}", item.Group, item.Suffix));
        }

        return emailDomainSource
            .Select(item => new SelectDemoOption($"{keyword}{item.Label}", $"{keyword}{item.Label}", item.Group, item.Suffix));
    }

    private static ObservableCollection<NSelectOption> CreateOptionCollection(IEnumerable<SelectDemoOption> source)
    {
        return new ObservableCollection<NSelectOption>(
            source.Select(item => new NSelectOption
            {
                Label = item.Label,
                Value = item.Value,
                Suffix = item.Suffix,
                Source = item
            }));
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            "" => "\"\"",
            _ => value.ToString() ?? "null"
        };
    }
}
