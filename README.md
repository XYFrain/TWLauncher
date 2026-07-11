# TWLauncher

## Minecraft 1.12.2 本地启动

启动器固定从程序同级目录的 `Game` 文件夹读取 Minecraft 1.12.2，不会使用 `%AppData%\\.minecraft`。请准备以下文件：

```text
Game/
  Minecraft.json
  Minecraft.jar
  libraries/
  assets/
```

点击“启动游戏”会以离线用户名 `Player` 启动。可选地，将 Java 8 放在 `Game/java/bin/javaw.exe`；否则启动器会使用系统 PATH 中的 `javaw.exe`。

一个基于 WPF（.NET Framework 4.8）打造的启动器界面。

## 📸 界面预览

![TWLauncher](Images/bg.png)

## ✨ 功能特性

- 🪟 **无边框窗口** — 自定义标题栏，全透明背景
- 🖼️ **高清背景** — 自适应背景图展示
- 🔘 **窗口控制** — 最小化、关闭按钮（设置按钮待实现）
- 👤 **用户入口** — 用户登录按钮（界面已预留，逻辑待实现）
- 📐 **自适应布局** — 窗口尺寸根据屏幕分辨率自动适配

## 🛠️ 技术栈

- **语言**: C#
- **框架**: WPF (.NET Framework 4.8)
- **IDE**: Visual Studio 2022 / 2019

## 🚀 运行

### 方式一：Visual Studio

1. 打开 `TWlauncher.slnx`
2. 按 `F5` 直接运行

### 方式二：MSBuild 命令行

```bash
msbuild TWlauncher.csproj /p:Configuration=Debug
.\bin\Debug\TWlauncher.exe
```

## 📁 项目结构

```
TWLauncher/
├── Images/               # 资源图片
│   └── bg.png           # 主界面背景图
├── Styles/
│   └── Icons.xaml       # SVG 图标路径 & 按钮样式
├── Properties/          # 程序集属性 & 资源文件
├── App.xaml / .cs       # 应用程序入口
├── MainWindow.xaml / .cs # 主窗口
├── TWlauncher.csproj    # 项目文件
├── TWlauncher.slnx      # 解决方案文件
└── README.md            # 项目说明
```

## 📦 构建

| 配置 | 命令 |
|------|------|
| Debug | `msbuild TWlauncher.csproj /p:Configuration=Debug` |
| Release | `msbuild TWlauncher.csproj /p:Configuration=Release` |

## 📄 许可证

MIT License
