using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public partial class ColorPickerDocsPageViewModel : ViewModelBase
{
    private readonly INMessageApi message = NMessage.UseMessage();
    
    private string basicValue = "#18A058";
    private string inputTriggerValue = "rgba(32, 128, 240, 0.74)";
    private string alphaValue = "rgba(24, 160, 88, 0.62)";
    private string modeValue = "hsl(153, 74%, 36%)";
    private string swatchValue = "#2080F0";
    private string eventValue = "#D03050";
    private string eventStatusText = "暂无事件。";

    [RelayCommand]
    private void ConfirmButton()
    {
        message.Info("Command 点击确认按钮");
    }

    public ColorPickerDocsPageViewModel(string selectedKey = "color-picker")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("触发器", "SectionTrigger"),
            ("透明度", "SectionAlpha"),
            ("尺寸", "SectionSize"),
            ("格式", "SectionModes"),
            ("色板", "SectionSwatches"),
            ("确认动作", "SectionActions"),
            ("预览与禁用", "SectionPreviewDisabled"),
            ("受控弹层", "SectionControlled"),
            ("API", "SectionApi"),
            ("属性", "SectionProps"),
            ("事件", "SectionEvents"),
            ("方法", "SectionMethods"));

        PropsRows =
        [
            new ApiDocRow { Name = "Value", Type = "string", DefaultValue = "null", Description = "当前颜色值，支持 Hex、RGB/RGBA、HSL/HSLA、HSV/HSVA 和 WPF 已知颜色名。" },
            new ApiDocRow { Name = "DefaultValue", Type = "string", DefaultValue = "\"rgb(0, 0, 0)\"", Description = "非受控初始值，对齐 Naive UI default-value。" },
            new ApiDocRow { Name = "IsDropDownOpen", Type = "bool", DefaultValue = "false", Description = "弹层显示状态，支持双向绑定，对齐 show / on-update:show。" },
            new ApiDocRow { Name = "DefaultShow", Type = "bool", DefaultValue = "false", Description = "初始化后默认展开弹层。" },
            new ApiDocRow { Name = "TriggerStyle", Type = "NColorPickerTriggerStyle", DefaultValue = "Button", Description = "触发器形态：Button 为默认色块按钮，Input 为输入框样式，Custom 为自定义触发器内容。" },
            new ApiDocRow { Name = "TriggerContent", Type = "object", DefaultValue = "null", Description = "自定义触发器内容，配合 TriggerStyle=Custom 使用。" },
            new ApiDocRow { Name = "ShowValueText", Type = "bool", DefaultValue = "false", Description = "是否在触发器里显示格式化颜色文本；默认关闭以贴近 Naive UI 色块按钮形态。" },
            new ApiDocRow { Name = "ShowAlpha", Type = "bool", DefaultValue = "true", Description = "是否显示透明度滑条；关闭后输出不带 alpha。" },
            new ApiDocRow { Name = "ShowPreview", Type = "bool", DefaultValue = "false", Description = "是否在面板右侧显示当前颜色预览。" },
            new ApiDocRow { Name = "Modes", Type = "ObservableCollection<NColorPickerMode>", DefaultValue = "Rgb, Hex, Hsl", Description = "允许切换的颜色格式集合。" },
            new ApiDocRow { Name = "Mode", Type = "NColorPickerMode", DefaultValue = "Rgb", Description = "当前输出格式：Rgb、Hex、Hsl、Hsv。" },
            new ApiDocRow { Name = "Swatches", Type = "ObservableCollection<string>", DefaultValue = "[]", Description = "预设色板集合。" },
            new ApiDocRow { Name = "Actions", Type = "ObservableCollection<NColorPickerAction>", DefaultValue = "[]", Description = "面板底部动作，目前支持 Confirm。" },
            new ApiDocRow { Name = "ConfirmButtonCommand", Type = "ICommand", DefaultValue = "ConfirmCommand", Description = "确认按钮执行的命令。默认使用内置 ConfirmCommand，可替换为业务命令。" },
            new ApiDocRow { Name = "ConfirmButtonCommandParameter", Type = "object", DefaultValue = "null", Description = "传递给 ConfirmButtonCommand 的参数。" },
            new ApiDocRow { Name = "Placement", Type = "NSelectPlacement", DefaultValue = "BottomStart", Description = "弹层位置，复用项目 Select 的 6 个方向。" },
            new ApiDocRow { Name = "Size", Type = "NSelectSize", DefaultValue = "Medium", Description = "Tiny / Small / Medium / Large 四种触发器尺寸。" },
            new ApiDocRow { Name = "PanelWidth", Type = "double", DefaultValue = "260", Description = "弹层宽度。" },
            new ApiDocRow { Name = "SaturationValueHeight", Type = "double", DefaultValue = "150", Description = "二维取色面板高度。" },
            new ApiDocRow { Name = "Clearable", Type = "bool", DefaultValue = "false", Description = "输入触发器悬浮时是否显示清除按钮。" },
            new ApiDocRow { Name = "Disabled", Type = "bool", DefaultValue = "false", Description = "禁用状态，对齐 Naive UI disabled。" },
            new ApiDocRow { Name = "IsReadOnly", Type = "bool", DefaultValue = "false", Description = "只读状态，保留值但禁止打开和编辑。" },
            new ApiDocRow { Name = "ShowArrow", Type = "bool", DefaultValue = "true", Description = "是否在输入触发器右侧显示箭头。" },
            new ApiDocRow { Name = "Placeholder", Type = "string", DefaultValue = "\"请选择颜色\"", Description = "无值时显示的占位文案。" },
            new ApiDocRow { Name = "DisplayValue", Type = "string", DefaultValue = "string.Empty", Description = "当前格式化后的显示文本，只读。" },
            new ApiDocRow { Name = "SelectedBrush", Type = "Brush", DefaultValue = "Transparent", Description = "当前颜色 Brush，只读，便于外部展示。" },
            new ApiDocRow { Name = "HasValue", Type = "bool", DefaultValue = "false", Description = "当前是否存在有效颜色值，只读。" },
            new ApiDocRow { Name = "IsInputTrigger", Type = "bool", DefaultValue = "false", Description = "当前是否为输入框触发器，只读，供模板状态使用。" },
            new ApiDocRow { Name = "IsCustomTrigger", Type = "bool", DefaultValue = "false", Description = "当前是否为自定义触发器，只读，供模板状态使用。" },
            new ApiDocRow { Name = "ShouldShowValueText", Type = "bool", DefaultValue = "false", Description = "当前触发器是否应该显示颜色文本，只读，供模板状态使用。" }
        ];

        EventRows =
        [
            new ApiDocRow { Name = "ValueChanged", Type = "RoutedEvent", DefaultValue = "-", Description = "颜色值变化时触发，对齐 on-update:value。" },
            new ApiDocRow { Name = "DropDownOpenChanged", Type = "RoutedEvent", DefaultValue = "-", Description = "弹层展开状态变化时触发，对齐 on-update:show。" },
            new ApiDocRow { Name = "Complete", Type = "RoutedEvent", DefaultValue = "-", Description = "拖拽结束、文本提交或确认时触发，对齐 on-complete。" },
            new ApiDocRow { Name = "ConfirmButtonClick", Type = "RoutedEvent", DefaultValue = "-", Description = "面板确认按钮被点击时触发，可配合 ConfirmButtonCommand 接管确认逻辑。" },
            new ApiDocRow { Name = "Clear", Type = "RoutedEvent", DefaultValue = "-", Description = "清除颜色值时触发。" }
        ];

        MethodRows =
        [
            new ApiDocRow { Name = "ClearSelection()", Type = "void", DefaultValue = "-", Description = "清空当前颜色值。" },
            new ApiDocRow { Name = "Confirm()", Type = "void", DefaultValue = "-", Description = "触发 Complete 并关闭弹层。" },
            new ApiDocRow { Name = "ClearCommand", Type = "RoutedUICommand", DefaultValue = "-", Description = "清除命令。" },
            new ApiDocRow { Name = "ConfirmCommand", Type = "RoutedUICommand", DefaultValue = "-", Description = "确认命令。" },
            new ApiDocRow { Name = "SelectSwatchCommand", Type = "RoutedUICommand", DefaultValue = "-", Description = "选择色板命令。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> PropsRows { get; }

    public IReadOnlyList<ApiDocRow> EventRows { get; }

    public IReadOnlyList<ApiDocRow> MethodRows { get; }

    public System.Collections.ObjectModel.ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public string BasicValue
    {
        get => basicValue;
        set => SetProperty(ref basicValue, value);
    }

    public string InputTriggerValue
    {
        get => inputTriggerValue;
        set => SetProperty(ref inputTriggerValue, value);
    }

    public string AlphaValue
    {
        get => alphaValue;
        set => SetProperty(ref alphaValue, value);
    }

    public string ModeValue
    {
        get => modeValue;
        set => SetProperty(ref modeValue, value);
    }

    public string SwatchValue
    {
        get => swatchValue;
        set => SetProperty(ref swatchValue, value);
    }

    public string EventValue
    {
        get => eventValue;
        set => SetProperty(ref eventValue, value);
    }

    public string EventStatusText
    {
        get => eventStatusText;
        set => SetProperty(ref eventStatusText, value);
    }

    public void RecordValueChanged(string? oldValue, string? newValue)
    {
        EventStatusText = $"ValueChanged：{FormatValue(oldValue)} -> {FormatValue(newValue)}";
    }

    public void RecordComplete(string value)
    {
        EventStatusText = $"Complete：{value}";
    }

    public void RecordOpenChanged(bool oldValue, bool newValue)
    {
        EventStatusText = $"DropDownOpenChanged：{oldValue} -> {newValue}";
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "null" : value;
    }
}
