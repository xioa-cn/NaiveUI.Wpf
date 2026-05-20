using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class EllipsisDocsPageViewModel : ViewModelBase
{
    private const string LongSongText = "住在我心里孤独的 孤独的海怪 痛苦之王 开始厌倦 深海的光 停滞的海浪";
    private const string ShortSongText = "住在我心里孤独的";
    private string dynamicText = LongSongText;

    public EllipsisDocsPageViewModel(string selectedKey = "ellipsis")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);

        EllipsisPropsRows =
        [
            new ApiDocRow { Name = "LineClamp", Type = "int?", DefaultValue = "null", Description = "设置最大显示行数。未设置时按单行省略处理，设置为正整数后按多行省略处理。" },
            new ApiDocRow { Name = "ExpandTrigger", Type = "NEllipsisExpandTrigger", DefaultValue = "None", Description = "设置展开触发方式。当前支持 Click，与 Naive UI 的 expand-trigger=\"click\" 对齐。" },
            new ApiDocRow { Name = "Tooltip", Type = "bool", DefaultValue = "true", Description = "文本被截断时，是否在悬浮时显示完整内容提示。" },
            new ApiDocRow { Name = "TooltipContent", Type = "object", DefaultValue = "null", Description = "自定义 Tooltip 内容。未设置时默认显示完整文本。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "需要省略的内容。当前实现优先针对字符串和 TextBlock.Text 场景做还原。" },
            new ApiDocRow { Name = "HasOverflow", Type = "bool", DefaultValue = "false", Description = "只读属性。表示当前内容在当前宽度和行数限制下是否发生溢出。" },
            new ApiDocRow { Name = "IsExpanded", Type = "bool", DefaultValue = "false", Description = "只读属性。表示当前是否处于展开态。" },
            new ApiDocRow { Name = "IsTruncated", Type = "bool", DefaultValue = "false", Description = "只读属性。表示当前渲染结果是否正处于省略状态。" }
        ];

        EllipsisSlotsRows =
        [
            new ApiDocRow { Name = "Default Content", Type = "object", DefaultValue = "null", Description = "省略组件的主内容区域，对应 Naive UI 的 default slot。" },
            new ApiDocRow { Name = "TooltipContent", Type = "object", DefaultValue = "null", Description = "Tooltip 内容区域，对应 Naive UI 的 tooltip slot。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<ApiDocRow> EllipsisPropsRows { get; }

    public IReadOnlyList<ApiDocRow> EllipsisSlotsRows { get; }

    public string BasicText => LongSongText;

    public string MultiLineText => "电灯熄灭 物换星移 泥牛入海\n黑暗好像 一颗巨石 按在胸口\n独脚大盗 百万富翁 摸爬滚打\n黑暗好像 一颗巨石 按在胸口";

    public string DynamicText
    {
        get => dynamicText;
        set => SetProperty(ref dynamicText, value);
    }

    public void ToggleDynamicText()
    {
        DynamicText = DynamicText.Length > ShortSongText.Length ? ShortSongText : LongSongText;
    }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
