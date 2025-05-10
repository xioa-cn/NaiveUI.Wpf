using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace NaiveUI.NControls.ControlsExample.Loading
{
    /// <summary>
    /// LoadingAnimation.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        public Brush BrushElement
        {
            get { return (Brush)GetValue(BrushElementProperty); }
            set { SetValue(BrushElementProperty, value); }
        }

        public static readonly DependencyProperty BrushElementProperty =
            ElementBase.Property<LoadingAnimation, Brush>(nameof(BrushElementProperty), (Brush)Application.Current.FindResource("Main.Brush"));


        private Storyboard _rotationStoryboard;
        public LoadingAnimation()
        {
            InitializeComponent();
            this.Loaded += LoadedAni;
            this.Unloaded += UnloadedAni;
        }

        private void UnloadedAni(object sender, RoutedEventArgs e)
        {
            _rotationStoryboard.Stop();
        }

        private void LoadedAni(object sender, RoutedEventArgs e)
        {
            // 获取Storyboard资源
            _rotationStoryboard = (Storyboard)FindResource("RotationStoryboard");

           
            _rotationStoryboard?.Begin();
        }

        

      
    }
}
