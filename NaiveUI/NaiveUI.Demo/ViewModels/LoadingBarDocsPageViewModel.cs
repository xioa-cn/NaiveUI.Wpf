using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using NaiveUI.Demo.Models;
using NaiveUI.Demo.Services;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.ViewModels;

public sealed class LoadingBarDocsPageViewModel : ViewModelBase
{
    public LoadingBarDocsPageViewModel(string selectedKey = "loading-bar")
    {
        SidebarCategories = ComponentSidebarViewModelFactory.Create(selectedKey);
        OutlineItems = DocOutlineItem.Create(
            ("基础用法", "SectionBasic"),
            ("粗细", "SectionThickness"),
            ("状态流转", "SectionStateFlow"),
            ("Provider", "SectionProvider"),
            ("API", "SectionApi"),
            ("LoadingBar API", "SectionLoadingBarApi"));

        LoadingBarCommand = new AsyncRelayCommand<object?>(HandleLoadingBarAsync);

        LoadingBarApiRows =
        [
            new ApiDocRow { Name = "NLoadingBar.UseLoadingBar()", Type = "INLoadingBarApi", DefaultValue = "-", Description = "获取当前窗口最近可用的 loading bar 接口。" },
            new ApiDocRow { Name = "NLoadingBarProvider.BarHeight", Type = "double", DefaultValue = "2.5", Description = "控制加载条粗细，单位为 px。" },
            new ApiDocRow { Name = "Start()", Type = "void", DefaultValue = "-", Description = "显示加载条，并启动默认的递进动画。" },
            new ApiDocRow { Name = "Update(double)", Type = "void", DefaultValue = "-", Description = "设置当前进度，取值范围为 0 到 1。" },
            new ApiDocRow { Name = "Finish()", Type = "void", DefaultValue = "-", Description = "推进到 100%，然后淡出结束。" },
            new ApiDocRow { Name = "Error()", Type = "void", DefaultValue = "-", Description = "切换到错误状态，补满进度后淡出结束。" }
        ];
    }

    public ObservableCollection<ComponentSidebarCategoryViewModel> SidebarCategories { get; }

    public IReadOnlyList<DocOutlineItem> OutlineItems { get; }

    public IReadOnlyList<ApiDocRow> LoadingBarApiRows { get; }

    public IAsyncRelayCommand<object?> LoadingBarCommand { get; }

    private static INLoadingBarApi ResolveLoadingBar()
    {
        return NLoadingBar.UseLoadingBar(Application.Current?.MainWindow);
    }

    public async Task RunSequenceAsync(bool error = false)
    {
        var loadingBar = ResolveLoadingBar();
        loadingBar.Start();
        await Task.Delay(380);
        loadingBar.Update(0.28d);
        await Task.Delay(380);
        loadingBar.Update(0.56d);
        await Task.Delay(420);
        loadingBar.Update(0.82d);
        await Task.Delay(360);

        if (error)
        {
            loadingBar.Error();
            return;
        }

        loadingBar.Finish();
    }

    private async Task HandleLoadingBarAsync(object? parameter)
    {
        var loadingBar = ResolveLoadingBar();

        switch (parameter?.ToString())
        {
            case "start":
                loadingBar.Start();
                break;
            case "finish":
                loadingBar.Finish();
                break;
            case "error":
                loadingBar.Error();
                break;
            case "sequence":
                await RunSequenceAsync();
                break;
            case "error-sequence":
                await RunSequenceAsync(true);
                break;
            case "manual-30":
                loadingBar.Start();
                loadingBar.Update(0.3d);
                break;
            case "manual-70":
                loadingBar.Start();
                loadingBar.Update(0.7d);
                break;
        }
    }
}
