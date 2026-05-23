using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public sealed class MessageDocsPageViewModel : ViewModelBase
{
    private readonly INMessageApi message = NMessage.UseMessage();

    public MessageDocsPageViewModel()
    {
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("持续时间", "SectionTiming"),
            ("加载中", "SectionLoading"),
            ("手动关闭", "SectionManuallyClose"),
            ("修改内容", "SectionModifyContent"),
            ("多行内容", "SectionMultipleLine"),
            ("兼容 NElMessage", "SectionNElMessage"),
            ("MessageProvider", "SectionProvider"),
            ("API", "SectionApi"),
            ("消息 API", "SectionMessageApi"),
            ("消息参数", "SectionMessageOptions"),
            ("MessageProvider 属性", "SectionMessageProviderProps"));

        MessageCommand = new AsyncRelayCommand<object?>(HandleMessageAsync);

        MessageApiRows =
        [
            new ApiDocRow { Name = "NMessage.UseMessage()", Type = "INMessageApi", DefaultValue = "-", Description = "获取当前窗口可用的消息 API。优先挂载到最近的 NMessageProvider；没有 provider 时自动回退到离散宿主。" },
            new ApiDocRow { Name = "Create(string, NMessageType, int)", Type = "NMessageReactive", DefaultValue = "-", Description = "创建一条消息。第三个参数为持续时间，单位毫秒；传 0 表示不自动关闭。" },
            new ApiDocRow { Name = "Create(NMessageOptions)", Type = "NMessageReactive", DefaultValue = "-", Description = "使用选项对象创建消息，适合在 ViewModel 或服务层集中组装参数。" },
            new ApiDocRow { Name = "Info(string, int)", Type = "NMessageReactive", DefaultValue = "duration = 3000", Description = "显示信息态消息。" },
            new ApiDocRow { Name = "Success(string, int)", Type = "NMessageReactive", DefaultValue = "duration = 3000", Description = "显示成功态消息。" },
            new ApiDocRow { Name = "Warning(string, int)", Type = "NMessageReactive", DefaultValue = "duration = 3000", Description = "显示警告态消息。" },
            new ApiDocRow { Name = "Error(string, int)", Type = "NMessageReactive", DefaultValue = "duration = 3000", Description = "显示错误态消息。" },
            new ApiDocRow { Name = "Loading(string, int)", Type = "NMessageReactive", DefaultValue = "duration = 0", Description = "显示 loading 消息。默认不自动关闭，通常搭配 reactive handle 手动销毁。" },
            new ApiDocRow { Name = "NMessageReactive.Content", Type = "string", DefaultValue = "-", Description = "更新已显示消息的文本内容，可用于实现 Modify Content 这类演示场景。" },
            new ApiDocRow { Name = "NMessageReactive.Destroy()", Type = "void", DefaultValue = "-", Description = "手动销毁由 Create / Loading 返回的消息实例。" },
            new ApiDocRow { Name = "NElMessage.Info / Success / Warning / Error / Loading", Type = "static helper", DefaultValue = "-", Description = "保留旧的离散调用方式，内部已经统一委托到新的 Message API。" }
        ];

        MessageOptionsRows =
        [
            new ApiDocRow { Name = "Content", Type = "string", DefaultValue = "\"\"", Description = "消息内容。" },
            new ApiDocRow { Name = "Type", Type = "NMessageType", DefaultValue = "Info", Description = "消息类型。当前支持 Info、Success、Warning、Error、Loading。" },
            new ApiDocRow { Name = "Duration", Type = "int", DefaultValue = "3000", Description = "消息持续时间，单位毫秒。设为 0 时不会自动关闭。" }
        ];

        MessageProviderPropsRows =
        [
            new ApiDocRow { Name = "Content", Type = "object", DefaultValue = "null", Description = "被 provider 包裹的内容。一个窗口里可以有多个 provider，消息会优先渲染到最近的作用域顶部。" }
        ];
    }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> MessageApiRows { get; }

    public IReadOnlyList<ApiDocRow> MessageOptionsRows { get; }

    public IReadOnlyList<ApiDocRow> MessageProviderPropsRows { get; }

    public IAsyncRelayCommand<object?> MessageCommand { get; }

    private async Task HandleMessageAsync(object? parameter)
    {
        switch (parameter?.ToString())
        {
            case "info":
                message.Info("这是一条普通消息。");
                break;
            case "success":
                message.Success("这是一条成功消息。");
                break;
            case "warning":
                message.Warning("这是一条警告消息。");
                break;
            case "error":
                message.Error("这是一条错误消息。");
                break;
            case "timing":
                message.Create("这条消息会持续显示 5 秒。", NMessageType.Info, 5000);
                break;
            case "loading":
            {
                var loading = message.Loading("正在加载，请稍候...");
                await Task.Delay(1500);
                loading.Destroy();
                message.Success("加载完成。");
                break;
            }
            case "manual":
            {
                var keepAlive = message.Loading("这条消息会在 3 秒后手动关闭。");
                await Task.Delay(3000);
                keepAlive.Destroy();
                break;
            }
            case "modify":
            {
                var reactive = message.Loading("第 1 步：正在连接...");
                await Task.Delay(900);
                reactive.Content = "第 2 步：正在同步消息内容...";
                await Task.Delay(900);
                reactive.Content = "第 3 步：即将完成。";
                await Task.Delay(900);
                reactive.Destroy();
                message.Success("消息内容更新完成。");
                break;
            }
            case "multiline":
                message.Create(
                    "这是一条多行消息。\n当内容变长时，它会自然换行显示。",
                    NMessageType.Info,
                    5000);
                break;
            case "discrete":
                NElMessage.Info("这条消息通过 NElMessage 发出，但底层仍然复用了同一套 Message API。");
                break;
        }
    }
}
