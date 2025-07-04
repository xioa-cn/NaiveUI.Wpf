namespace NaiveUI.NControls.Tools;

public static class NElMessage {
    public static void Show(string message, string type = "info") {
        switch (type.ToLower())
        {
            case "info":
                ElMessage.Wpf.Utils.ElMessage.Info(message);
                break;
            case "success":
                ElMessage.Wpf.Utils.ElMessage.Success(message);
                break;
            case "warning":
                ElMessage.Wpf.Utils.ElMessage.Warning(message);
                break;
            case "error":
                ElMessage.Wpf.Utils.ElMessage.Error(message);
                break;
            default:
                ElMessage.Wpf.Utils.ElMessage.Info(message);
                break;
        }
    }
}