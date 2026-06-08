<p align="center">
  <img width="144px" src="https://naiveui.oss-cn-hongkong.aliyuncs.com/naivelogo.svg" />
</p>
<h1 align="center">Naive UI On Wpf</h1>
<p align="center">
  <b>把 Naive UI 的清爽、克制和高完成度，带到 WPF 桌面应用。</b>
</p>

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square" />
  <img alt="WPF" src="https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square" />
  <img alt="Language" src="https://img.shields.io/badge/C%23-12-239120?style=flat-square" />
  <img alt="Status" src="https://img.shields.io/badge/status-active-brightgreen?style=flat-square" />
</p>

NaiveUI.Wpf 是一个面向现代 Windows 桌面应用的 WPF 组件库。它以 Naive UI 的视觉语言和交互体验为参考，使用 WPF 原生控件、依赖属性、模板和资源字典来实现，适合后台系统、工控软件、桌面工具、企业内部应用等需要“好看但不抢戏”的场景。

如果你喜欢 Naive UI 的干净边界、轻量阴影、清晰状态和紧凑布局，但项目技术栈是 WPF，这个库就是为你准备的。

## 特性

- **Naive UI 风格还原**：按钮、选择器、自动完成、颜色选择器、标签、提示、消息、加载条等组件持续对齐 Naive UI 的视觉和 API 语义。
- **WPF 原生实现**：基于 `ControlTemplate`、`DependencyProperty`、`RoutedEvent`、`Command` 和资源字典，不引入 WebView 包壳。
- **主题化设计**：内置 `SkinDefault.xaml`、`LightTheme.xaml`、`DarkTheme.xaml`、`ThemeTokens.xaml`，方便统一调整颜色、圆角、阴影和状态样式。
- **MVVM 友好**：核心交互尽量开放绑定属性、命令、事件和模板入口，减少 code-behind 依赖。
- **Demo 即文档**：Demo 工程按组件文档页组织，包含基础用法、不同状态、API 表和可复制的 XAML 示例。
- **持续补齐中**：当前重点完善数据录入、反馈和通用组件，适合在真实项目中边用边迭代。

## 快速开始

### 1. 克隆项目

```powershell
git clone https://github.com/xioa-cn/NaiveUI.Wpf.git
cd NaiveUI.Wpf\NaiveUI
```

### 2. 构建 Demo

```powershell
dotnet build NaiveUI.Demo\NaiveUI.Demo.csproj -v minimal
```

### 3. 运行 Demo

```powershell
dotnet run --project NaiveUI.Demo\NaiveUI.Demo.csproj
```

> 运行环境：Windows + .NET 8 SDK。WPF 是 Windows 桌面技术，目标框架为 `net8.0-windows`。

## 在你的 WPF 项目中使用

当前仓库以源码引用方式开发。你可以先把 `NaiveUI.NControls` 作为项目引用加入到自己的 WPF 应用：

```xml
<ItemGroup>
  <ProjectReference Include="..\NaiveUI.NControls\NaiveUI.NControls.csproj" />
</ItemGroup>
```

然后在 `App.xaml` 合并主题资源：

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="/NaiveUI.NControls;component/SkinDefault.xaml" />
      <ResourceDictionary Source="/NaiveUI.NControls;component/Themes/ThemeResources.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

在页面中引入命名空间并使用组件：

```xml
<Window
    xmlns:nc="clr-namespace:NaiveUI.NControls.Controls;assembly=NaiveUI.NControls">

    <StackPanel Orientation="Horizontal">
        <nc:NButton Content="主要按钮" Kind="Filled" Margin="0,0,12,0" />
        <nc:NSelect Width="220" Placeholder="请选择" Margin="0,0,12,0" />
        <nc:NColorPicker Width="220" />
    </StackPanel>
</Window>
```

## 组件覆盖

Demo 导航目前覆盖 8 个分组、95 个组件条目，已实现和重点打磨的组件包括：

| 分类 | 组件 |
| --- | --- |
| 通用 | Button、Card、Avatar、Badge、Tag、Icon、Typography、Divider、Carousel、Collapse、Dropdown、Ellipsis、Gradient Text、Page Header、Watermark、Float Button |
| 数据录入 | Input、Select、Auto Complete、Color Picker、Switch |
| 导航 | Breadcrumb、Menu、Loading Bar |
| 反馈 | Tooltip、Message、Loading Bar |
| 布局与基础 | Layout、ScrollViewer、ClipBorder、主题 Token 与基础样式 |

更多组件正在按 Naive UI 的能力模型继续补齐。每个组件优先保证默认视觉、常用状态、可绑定属性和模板扩展点，再逐步完善边界交互。

## 项目结构

```text
NaiveUI.Wpf/
├─ README.md
└─ NaiveUI/
   ├─ NaiveUI.sln
   ├─ NaiveUI.NControls/        # WPF 控件库，包含控件、主题、模板、转换器
   │  ├─ Controls/
   │  ├─ Themes/
   │  └─ SkinDefault.xaml
   └─ NaiveUI.Demo/             # 组件文档与演示应用
      ├─ Views/Pages/
      ├─ ViewModels/
      └─ Data/ComponentSidebar.json
```

## 设计目标

NaiveUI.Wpf 不追求把 Web 组件机械搬到桌面端，而是把 Naive UI 的体验翻译成 WPF 应用该有的形态：

- 控件默认值要好用，放到业务页面里不突兀。
- 状态反馈要明确，hover、focus、disabled、loading 都应可感知。
- 布局尺寸要稳定，弹层、下拉框、选择器、颜色面板不能因为内容变化抖动。
- 属性要尽量开放，既能快速使用，也能深度定制。
- 样式要集中在资源字典里，方便主题替换和团队统一维护。

## 适合用在这些场景

- 企业后台、MES、SCADA、设备管理、配置工具等长期运行的桌面软件。
- 需要现代视觉，但仍依赖 WPF 生态、Windows API 或本地硬件能力的项目。
- 已有 WPF 项目想逐步替换传统控件样式，不希望一次性重写 UI 技术栈。
- 希望用 XAML、MVVM、Command 和模板扩展来保持桌面端工程习惯的团队。

## 开发状态

项目处于快速迭代阶段，组件 API 和视觉细节仍在持续对齐 Naive UI。建议在接入生产项目前锁定具体提交版本，并优先从 Demo 中复制当前推荐写法。

近期重点：

- 补齐更多 Naive UI 组件语义和属性映射。
- 完善 Select、AutoComplete、ColorPicker 等复杂输入组件的边界交互。
- 强化浅色/深色主题 Token、弹层圆角、阴影、边框和文字对比度。
- 扩充 Demo 文档页和 API 表。

## 贡献

欢迎提交 issue、建议和 PR。比较推荐的贡献方式：

1. 先在 Demo 中补一个可复现示例或目标组件页。
2. 再修改 `NaiveUI.NControls` 中的控件、主题和模板。
3. 最后运行 Demo，确认默认态、hover、focus、disabled、弹层和绑定场景都正常。

## 致谢

视觉与交互方向参考了优秀的 [Naive UI](https://www.naiveui.com/) 设计体系。本项目是 WPF 生态下的社区实现，并非 Naive UI 官方项目。