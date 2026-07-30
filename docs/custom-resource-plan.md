# 自定义资源下载更新方案

## 概述

MC 专属服务器需要部署自定义资源（mods、resourcepacks 等），独立于 Mojang 官方资源的更新周期。采用**压缩包 + 版本号**方案，简单可靠。

---

## 架构

```
你的服务器
├─ custom/version.json              {"version": "1.0.3", "url": "https://xxx/resources-1.0.3.zip"}
└─ custom/resources-1.0.3.zip       mods + resourcepacks + 配置等

客户端 Game/
├─ custom_version.txt               ← 本地版本号，例: "1.0.2"
├─ mods/
├─ resourcepacks/
└─ ...
```

---

## 初始化流程

```
MainWindowController.InitializeAsync()
├─ 1. CheckNetworkAsync        ← 测 BMCLAPI 连通
├─ 2. CheckJavaAsync           ← 扫描 Java 17+
├─ 3. CheckJsonAsync           ← 下载合并 Minecraft.json
├─ 4. CheckResourcesAsync      ← CheckService.ScanResources() + CheckService.Items
└─ 5. CheckCustomAsync (新)    ← 拉 version.json → 比对版本 → 写入 Items
                                  ↓
                         Phase.Launch / Phase.Download
```

---

## CheckCustomAsync 实现细节

### 输入

- 服务器 `custom/version.json`，格式：
  ```json
  {
    "version": "1.0.3",
    "url": "https://你的服务器/custom/resources-1.0.3.zip"
  }
  ```
- 本地 `Game/custom_version.txt`，内容为版本号字符串，如 `"1.0.2"`

### 逻辑

1. HTTP GET 拉取 `{服务器}/custom/version.json`
2. 读本地 `Game/custom_version.txt`
3. 版本号相同 → 跳过（日志: "自定义资源已是最新"）
4. 版本号不同（或本地文件不存在）→ 把 zip 写入 `CheckService.Items`：
   ```csharp
   var item = new ResourceItem {
       Url  = serverVersion.url,
       Path = Paths.CustomZip,       // Game/custom.zip
       Sha1 = "",                    // zip 不做 SHA1 校验
       Size = 0                      // 未知大小
   };
   CheckService.Items.Add(item);
   CheckService.TotalBytes += 0;     // 未知，下载时不占总大小
   ```

### 与 CheckService 的关系

- **不新建类**，逻辑直接写在 `MainWindowService.CheckCustomAsync` 内（20 行以内）
- 复用 `CheckService.Items` 作为下载清单，下载时一起走 `DownloadService.DownloadResourcesAsync`
- 下载完成后，由 `DownloadService` 调用解压 + 写入 `custom_version.txt`

---

## 下载端改造

`DownloadService.DownloadResourcesAsync` 下载完所有 Items 后：

1. 判断 `Game/custom.zip` 是否存在 → 存在则解压到 `Game/`
2. 写入 `Game/custom_version.txt`（版本号从服务器 version.json 获取，需传递或缓存）

---

## 实施步骤

### 阶段 1：接通现有下载按钮（无自定义资源）

1. `MainViewModel.DownloadAsync` 取消注释，调 `DownloadService.DownloadResourcesAsync`
2. `MainViewModel.Launch` 取消注释，调 `LaunchService`
3. `LaunchService` 适配 Fabric 合并后的 `arguments` 格式（非旧的 `minecraftArguments`）

### 阶段 2：加自定义资源检测

4. `Paths.cs` 加 `CustomVersionPath`、`CustomZipPath` 常量
5. `MainWindowService.CheckCustomAsync` 实现版本比对逻辑
6. `MainWindowController.InitializeAsync` 追加第 5 步

### 阶段 3：加自定义资源下载解压

7. `DownloadService.DownloadResourcesAsync` 下载完后处理 custom.zip 解压
8. 写入 `custom_version.txt`

### 阶段 4：自定义 URL 常量

9. `Paths.cs` 加 `CustomManifestUrl` 常量，指向你服务器上 `version.json` 的完整 URL

---

## 路径常量（Paths.cs 新增）

```csharp
/// <summary>自定义资源 manifest URL</summary>
public const string CustomManifestUrl = "https://你的服务器/custom/version.json";

/// <summary>自定义资源包下载临时路径</summary>
public static string CustomZip => Path.Combine(_root, "custom.zip");

/// <summary>本地自定义资源版本号文件</summary>
public static string CustomVersionPath => Path.Combine(_root, "custom_version.txt");
```

---

## 注意事项

### 安全

- `custom.zip` 解压前确保目标路径在 `Game/` 内，防止 zip slip 攻击
- 服务器 `version.json` 拉取失败 → 跳过自定义资源检查，不阻断启动（网络波动不应阻止玩家进游戏）

### 版本比对

- 使用 `Version` 类或简单字符串比较（纯数字版本号如 `1.0.3`）
- 本地 `custom_version.txt` 不存在 → 视为版本 `"0.0.0"`，触发下载

### 下载时机

- 自定义 zip 和 MC 资源一起并发下载（已经在同一个 Items 列表里）
- zip 本身不校验 SHA1（服务器上不提供），只检验文件存在
