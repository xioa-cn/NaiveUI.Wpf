using System.Windows.Input;

namespace NaiveUI.NControls.Tools
{
    public static class ControlCommands
    {
        //
        // 摘要:
        //     搜索
        public static RoutedCommand Search { get; } = new RoutedCommand("Search", typeof(ControlCommands));


        //
        // 摘要:
        //     清除
        public static RoutedCommand Clear { get; } = new RoutedCommand("Clear", typeof(ControlCommands));


        //
        // 摘要:
        //     切换
        public static RoutedCommand Switch { get; } = new RoutedCommand("Switch", typeof(ControlCommands));


        //
        // 摘要:
        //     右转
        public static RoutedCommand RotateRight { get; } = new RoutedCommand("RotateRight", typeof(ControlCommands));


        //
        // 摘要:
        //     左转
        public static RoutedCommand RotateLeft { get; } = new RoutedCommand("RotateLeft", typeof(ControlCommands));


        //
        // 摘要:
        //     小
        public static RoutedCommand Reduce { get; } = new RoutedCommand("Reduce", typeof(ControlCommands));


        //
        // 摘要:
        //     大
        public static RoutedCommand Enlarge { get; } = new RoutedCommand("Enlarge", typeof(ControlCommands));


        //
        // 摘要:
        //     还原
        public static RoutedCommand Restore { get; } = new RoutedCommand("Restore", typeof(ControlCommands));


        //
        // 摘要:
        //     打开
        public static RoutedCommand Open { get; } = new RoutedCommand("Open", typeof(ControlCommands));


        //
        // 摘要:
        //     保存
        public static RoutedCommand Save { get; } = new RoutedCommand("Save", typeof(ControlCommands));


        //
        // 摘要:
        //     选中
        public static RoutedCommand Selected { get; } = new RoutedCommand("Selected", typeof(ControlCommands));


        //
        // 摘要:
        //     关闭
        public static RoutedCommand Close { get; } = new RoutedCommand("Close", typeof(ControlCommands));


        //
        // 摘要:
        //     取消
        public static RoutedCommand Cancel { get; } = new RoutedCommand("Cancel", typeof(ControlCommands));


        //
        // 摘要:
        //     确定
        public static RoutedCommand Confirm { get; } = new RoutedCommand("Confirm", typeof(ControlCommands));


        //
        // 摘要:
        //     是
        public static RoutedCommand Yes { get; } = new RoutedCommand("Yes", typeof(ControlCommands));


        //
        // 摘要:
        //     否
        public static RoutedCommand No { get; } = new RoutedCommand("No", typeof(ControlCommands));


        //
        // 摘要:
        //     关闭所有
        public static RoutedCommand CloseAll { get; } = new RoutedCommand("CloseAll", typeof(ControlCommands));


        //
        // 摘要:
        //     关闭其他
        public static RoutedCommand CloseOther { get; } = new RoutedCommand("CloseOther", typeof(ControlCommands));


        //
        // 摘要:
        //     上一个
        public static RoutedCommand Prev { get; } = new RoutedCommand("Prev", typeof(ControlCommands));


        //
        // 摘要:
        //     下一个
        public static RoutedCommand Next { get; } = new RoutedCommand("Next", typeof(ControlCommands));


        //
        // 摘要:
        //     跳转
        public static RoutedCommand Jump { get; } = new RoutedCommand("Jump", typeof(ControlCommands));


        //
        // 摘要:
        //     上午
        public static RoutedCommand Am { get; } = new RoutedCommand("Am", typeof(ControlCommands));


        //
        // 摘要:
        //     下午
        public static RoutedCommand Pm { get; } = new RoutedCommand("Pm", typeof(ControlCommands));


        //
        // 摘要:
        //     确认
        public static RoutedCommand Sure { get; } = new RoutedCommand("Sure", typeof(ControlCommands));


        //
        // 摘要:
        //     小时改变
        public static RoutedCommand HourChange { get; } = new RoutedCommand("HourChange", typeof(ControlCommands));


        //
        // 摘要:
        //     分钟改变
        public static RoutedCommand MinuteChange { get; } = new RoutedCommand("MinuteChange", typeof(ControlCommands));


        //
        // 摘要:
        //     秒改变
        public static RoutedCommand SecondChange { get; } = new RoutedCommand("SecondChange", typeof(ControlCommands));


        //
        // 摘要:
        //     鼠标移动
        public static RoutedCommand MouseMove { get; } = new RoutedCommand("MouseMove", typeof(ControlCommands));


        //
        // 摘要:
        //     按照类别排序
        public static RoutedCommand SortByCategory { get; } = new RoutedCommand("SortByCategory", typeof(ControlCommands));


        //
        // 摘要:
        //     按照名称排序
        public static RoutedCommand SortByName { get; } = new RoutedCommand("SortByName", typeof(ControlCommands));


        //
        // 摘要:
        //     更多
        public static RoutedCommand More { get; } = new RoutedCommand("More", typeof(ControlCommands));


        public static RoutedCommand Toggle { get; } = new RoutedCommand("Toggle", typeof(ControlCommands));

    }
}
