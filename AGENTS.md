# TWLauncher — 项目约定

WPF .NET Framework 4.8 Minecraft 启动器，C# 7.3，轻量 MVVM。

- .NET Framework 4.8，默认 C# 7.3，不可用 `new()` 简写、`switch` 表达式等 C# 8+ 特性。

## 目录结构

```
Controller/    MainButtonController（按钮流程）、ConfigController（静态配置入口）
MainWindow/    MainViewModel、MainButtonViewModel、ProgressViewModel、SettingsViewModel、RelayCommand
Service/       DownloadService、LaunchService、JavaService、ConfigService、MainWindowService、CheckService
Utils/         HttpUtil、JsonUtil、FileUtil、ByteUtil、RuleUtil、LogUtil
Models/        ResourceItem、JavaEntry（POCO，属性方法需 XML 注释中文）
Paths.cs       根目录，静态路径常量
```

## 职责边界

| 层 | 做什么 | 不做什么 |
|----|--------|---------|
| Code-behind | DragMove、Minimize/Close Click、SetMainWindow | 业务逻辑 |
| Controller | 按钮状态、配置入口 | 文件 IO、网络 |
| ViewModel | 命令绑定、状态暴露、流程编排 | 直接操作 UI 控件 |
| Service | 下载、启动、Java 检测、配置读写 | WPF 绑定 |
| Utils | 无状态工具方法 | 业务判断 |
| Models | 纯数据载体 | 任何逻辑 |

## XAML 约定

- 所有业务按钮用 `Command="{Binding ...}"`
- 纯窗口操作（Minimize、Close）用 `Click="..."` 留在 code-behind
- 无 Click/Checked 事件处理器（窗口操作除外）

## 铁律

- Agent 修改任何文件前必须 `read_file` 确认当前内容，在用户改动上做增量修改
- 禁止凭记忆重写整个文件覆盖用户 IDE 中的修改
- 改进建议用语言描述让用户选择，禁止未经允许擅自改代码
- 编译由用户在 IDE 中完成，Agent 只做 ad-hoc 结构验证脚本
- Models/ 下所有类的属性、方法均需 XML 文档注释（`/// <summary>`），中文
