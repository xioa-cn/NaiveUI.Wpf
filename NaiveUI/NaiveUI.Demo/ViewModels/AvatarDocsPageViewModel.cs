using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;

namespace NaiveUI.Demo.ViewModels;

public sealed class AvatarDocsPageViewModel: ViewModelBase
{
    public AvatarDocsPageViewModel(string selectedKey = "avatar")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);

        AvatarPropsRows =
        [
            new ApiDocRow { Name = "Size", Type = "double", DefaultValue = "34", Description = "设置头像宽高尺寸，单位为像素。" },
            new ApiDocRow { Name = "Shape", Type = "NAvatarShape", DefaultValue = "Square", Description = "设置头像形状。\nSquare 为圆角方形，\nRound 为圆形。" },
            new ApiDocRow { Name = "Src", Type = "ImageSource", DefaultValue = "null", Description = "设置头像主图片源。存在时优先显示图片。" },
            new ApiDocRow { Name = "FallbackSrc", Type = "ImageSource", DefaultValue = "null", Description = "主图片加载失败时使用的后备图片源。" },
            new ApiDocRow { Name = "FallbackContent", Type = "object", DefaultValue = "null", Description = "图片不可用且没有主内容时使用的后备内容，可传字符串、图标或任意视觉元素。" },
            new ApiDocRow { Name = "ImageStretch", Type = "Stretch", DefaultValue = "UniformToFill", Description = "设置图片填充方式。" },
            new ApiDocRow { Name = "Badge", Type = "string", DefaultValue = "\"\"", Description = "设置头像右上角徽标内容。" },
            new ApiDocRow { Name = "Background", Type = "Brush", DefaultValue = "Theme.Fill.2.Brush", Description = "设置头像背景色。" },
            new ApiDocRow { Name = "Foreground", Type = "Brush", DefaultValue = "Theme.Text.Primary.Brush", Description = "设置文字头像或图标头像的前景色。" },
            new ApiDocRow { Name = "PlaceholderForeground", Type = "Brush", DefaultValue = "Theme.Text.Tertiary.Brush", Description = "设置默认占位图标颜色。" },
            new ApiDocRow { Name = "BadgeBackground", Type = "Brush", DefaultValue = "Error.First.Brush", Description = "设置徽标背景色。" },
            new ApiDocRow { Name = "BadgeForeground", Type = "Brush", DefaultValue = "Theme.Text.Inverse.Brush", Description = "设置徽标文字颜色。" },
            new ApiDocRow { Name = "BadgeFontSize", Type = "double", DefaultValue = "Auto", Description = "设置徽标字号。默认会随头像尺寸自动计算，设置后优先使用自定义值。" },
            new ApiDocRow { Name = "BadgePadding", Type = "Thickness", DefaultValue = "Auto", Description = "设置徽标内边距。默认会根据头像尺寸自动计算，设置后优先使用自定义值。" },
            new ApiDocRow { Name = "BadgeMargin", Type = "Thickness", DefaultValue = "Auto", Description = "设置徽标相对头像右上角的偏移位置。默认会根据头像尺寸自动计算，设置后优先使用自定义值。" },
            new ApiDocRow { Name = "BadgeCornerRadius", Type = "CornerRadius", DefaultValue = "999", Description = "设置徽标圆角，可将默认胶囊改成圆角矩形、直角或其他自定义形状。" },
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "设置头像主内容，支持字符串、Path 或任意 WPF 视觉元素。" },
            new ApiDocRow { Name = "BorderBrush", Type = "Brush", DefaultValue = "Transparent", Description = "设置头像边框颜色，常用于头像组叠放效果。" },
            new ApiDocRow { Name = "BorderThickness", Type = "Thickness", DefaultValue = "0", Description = "设置头像边框厚度。" }
        ];

        AvatarSlotsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "头像内容插槽，可放文字、图标或自定义视觉元素。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<ApiDocRow> AvatarPropsRows { get; }

    public IReadOnlyList<ApiDocRow> AvatarSlotsRows { get; }

    public void SelectSidebarItem(ComponentSidebarItemViewModel targetItem)
    {
        foreach (var item in SidebarCategories.SelectMany(category => category.Items))
        {
            item.IsSelected = item == targetItem;
        }
    }
}
