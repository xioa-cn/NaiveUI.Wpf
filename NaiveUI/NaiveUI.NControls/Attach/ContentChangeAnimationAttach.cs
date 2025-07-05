using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Attach;

public class ContentChangeAnimationAttach {
    public static readonly DependencyProperty ContentChangedAnimationProperty =
        ElementBase.PropertyAttach<ContentChangeAnimationAttach, string>(
            nameof(ContentChangedAnimationProperty), "fade", OnContentChangedAnimationChanged);


    public static string GetContentChangedAnimation(DependencyObject obj) {
        return (string)obj.GetValue(ContentChangedAnimationProperty);
    }

    public static void SetContentChangedAnimation(DependencyObject obj, string value) {
        obj.SetValue(ContentChangedAnimationProperty, value);
    }

    private static void OnContentChangedAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ContentPresenter contentPresenter)
        {
            // 监听Content属性变化
            DependencyPropertyDescriptor.FromProperty(ContentPresenter.ContentProperty, typeof(ContentPresenter))
                .AddValueChanged(contentPresenter, (sender, args) =>
                {
                    if (sender is ContentPresenter cp && e.NewValue is string animationType)
                    {
                        switch (animationType.ToLower())
                        {
                            case "fade":
                                // 淡入淡出动画
                                cp.BeginAnimation(UIElement.OpacityProperty,
                                    new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.1)));
                                break;
                            case "slide":
                                // 滑动动画
                                var transform = new TranslateTransform();
                                cp.RenderTransform = transform;
                                transform.BeginAnimation(TranslateTransform.XProperty,
                                    new DoubleAnimation(100, 0, TimeSpan.FromSeconds(0.5)));
                                break;
                            // 其他动画类型...
                        }
                    }
                });
        }
    }
}