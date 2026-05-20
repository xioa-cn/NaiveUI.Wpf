using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class CarouselDocsPageViewModel : ViewModelBase
{
    public CarouselDocsPageViewModel(string selectedKey = "carousel")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("自动播放", "SectionAutoplay"),
            ("淡入淡出", "SectionFade"),
            ("卡片效果", "SectionCardEffect"),
            ("指示器位置", "SectionPlacement"),
            ("触发方式", "SectionTrigger"),
            ("接口说明", "SectionApi"),
            ("轮播属性", "SectionCarouselProps"),
            ("轮播插槽", "SectionCarouselSlots"));

        CarouselPropsRows =
        [
            new ApiDocRow { Name = "CurrentIndex", Type = "int", DefaultValue = "0", Description = "设置当前激活的轮播项索引。" },
            new ApiDocRow { Name = "Autoplay", Type = "bool", DefaultValue = "false", Description = "是否自动轮播。" },
            new ApiDocRow { Name = "Interval", Type = "int", DefaultValue = "5000", Description = "自动轮播间隔时间，单位为毫秒。" },
            new ApiDocRow { Name = "TransitionDuration", Type = "int", DefaultValue = "300", Description = "切换动画持续时间，单位为毫秒。" },
            new ApiDocRow { Name = "ShowArrow", Type = "bool", DefaultValue = "false", Description = "是否在悬停时显示左右切换箭头。" },
            new ApiDocRow { Name = "ShowDots", Type = "bool", DefaultValue = "true", Description = "是否显示轮播指示器。" },
            new ApiDocRow { Name = "Loop", Type = "bool", DefaultValue = "true", Description = "是否启用循环切换。" },
            new ApiDocRow { Name = "PauseOnHover", Type = "bool", DefaultValue = "true", Description = "鼠标悬停时是否暂停自动轮播。" },
            new ApiDocRow { Name = "Draggable", Type = "bool", DefaultValue = "true", Description = "是否允许通过鼠标拖拽切换轮播项。" },
            new ApiDocRow { Name = "Keyboard", Type = "bool", DefaultValue = "true", Description = "聚焦后是否允许使用左右方向键切换。" },
            new ApiDocRow { Name = "Effect", Type = "NCarouselEffect", DefaultValue = "Slide", Description = "设置切换效果。\nSlide 表示左右滑动切换；\nFade 表示淡入淡出切换；\nCard 表示带缩放和层叠感的卡片式切换。" },
            new ApiDocRow { Name = "DotType", Type = "NCarouselDotType", DefaultValue = "Dot", Description = "设置指示器类型，可选圆点或线条。" },
            new ApiDocRow { Name = "DotPlacement", Type = "NCarouselDotPlacement", DefaultValue = "Bottom", Description = "设置指示器位置，可放在上、下、左、右。" },
            new ApiDocRow { Name = "Trigger", Type = "NCarouselTrigger", DefaultValue = "Click", Description = "设置指示器触发方式，可选点击或悬停切换。" },
            new ApiDocRow { Name = "CornerRadius", Type = "CornerRadius", DefaultValue = "3", Description = "设置轮播容器圆角，同时影响内容裁剪区域。" },
            new ApiDocRow { Name = "ItemsSource", Type = "IEnumerable", DefaultValue = "null", Description = "通过标准 ItemsControl 方式绑定轮播数据源。" },
            new ApiDocRow { Name = "ItemTemplate", Type = "DataTemplate", DefaultValue = "null", Description = "为数据源中的轮播项指定展示模板。" }
        ];

        CarouselSlotsRows =
        [
            new ApiDocRow { Name = "默认插槽", Type = "object", DefaultValue = "null", Description = "默认内容插槽，可直接放置子元素或结合 ItemTemplate 渲染数据项。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> CarouselPropsRows { get; }

    public IReadOnlyList<ApiDocRow> CarouselSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}

