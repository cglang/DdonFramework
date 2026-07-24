# VitrinRuntime 项目文件地图

## 项目概述

**VitrinRuntime** 是一个基于 Avalonia UI + Vue 3 的桌面应用程序（上位机案例），采用 .NET 8 作为后端框架，Vue 3 + Element Plus 作为前端技术栈，通过嵌入式 WebView 方式集成前后端。

---

## 根目录

| 文件/目录 | 说明 |
|-----------|------|
| `VitrinRuntime.slnx` | 解决方案文件，引用框架层（Ddon.Desktop.Avalonia/Core/VitrinPLC）和本应用项目 |
| `ai.md` | AI Agent 执行入口文件：读取`.ai/rules.md` → 读取`.ai/project/` → 注册`.agents/skills/`技能 |
| `wookflow.zh.md` | 工作流说明文档 |

### 隐藏目录

#### `.ai/` — AI Agent 上下文目录

| 文件 | 说明 |
|------|------|
| `rules.md` | AI Agent 行为规则（7条）：自动更新上下文、禁止全项目扫描、文件过滤(忽略zh.md)、思考语言(中文优先)、模糊需求处理、不写自定义CSS、不使用URL传参 |
| `project/` | 项目上下文目录（含 file-map.md） |

#### `.agents/` — Agent 技能目录

| 文件 | 说明 |
|------|------|
| `skills/` | 技能存放目录（当前为空） |

#### `.qoder/` — Qoder 配置目录

| 文件 | 说明 |
|------|------|
| `plans/` | Qoder 任务计划存放目录 |

#### `.vs/` — Visual Studio 配置目录

---

## 应用层：`VitrinRuntime.Desktop/`

### 服务层：`Services/`

| 文件 | 说明 |
|------|------|
| `Models.cs` | 数据模型：PlcConfig（PLC连接配置）、DbGroup（DB块分组）、TagConfig（点位配置） |
| `PlcManagerService.cs` | Bridge Service（PlcManager）：ListPlcs/AddPlc/RemovePlc/ConnectPlc/DisconnectPlc/GetPlcStatus |
| `PlcDataService.cs` | Bridge Service（PlcData）：ListDbGroups/CreateDbGroup/DeleteDbGroup/RenameDbGroup/ListTags/AddTag/RemoveTag/ReadTag/WriteTag |
| `TagSubscriptionManager.cs` | 点位订阅管理器：管理点位的订阅生命周期，触发 TagValueChangedEvent |

### 事件处理：`Handlers/`

| 文件 | 说明 |
|------|------|
| `TagValueChangedEvent.cs` | 点位值变化事件定义，通过EventBus发布 |
| `TagValueChangedFrontendHandler.cs` | 前端推送处理器：将点位值变化推送到WebView前端 |
| `TagValueChangedHistoryHandler.cs` | 历史记录处理器：记录点位值变化历史 |

### 持久化存储：`Stores/`

| 文件 | 说明 |
|------|------|
| `IPlcConfigStore.cs` | PLC配置存储接口 |
| `PlcConfigStoreJson.cs` | JSON文件持久化存储实现 |
| `PlcConfigStoreMemory.cs` | 内存存储实现（ConcurrentDictionary）：PLC配置、分组、点位的CRUD |

### 配置文件

| 文件 | 说明 |
|------|------|
| `VitrinRuntime.Desktop.csproj` | 项目文件，TargetFramework: net8.0，引用 Ddon.Desktop.Avalonia、Ddon.VitrinPLC |
| `appsettings.json` | 应用配置：后端地址 `http://localhost:5000`，前端地址 `http://localhost:5173`，窗口标题/尺寸 |
| `app.manifest` | Windows 应用程序清单 |
| `App.axaml` | Avalonia XAML 应用入口 |
| `App.axaml.cs` | Avalonia 应用代码后置，配置 DI 容器：注册 `IPlcConfigStore`、`PlcManagerService`、`PlcDataService`、`TagSubscriptionManager`、`Handlers` |
| `Program.cs` | .NET 程序入口 |

#### API 层：`Frontend/src/api/`

| 文件 | 说明 |
|------|------|
| `plcApi.ts` | 类型化 PLC API：封装 window.ui.invoke 调用后端 Bridge Service |

#### 组件：`Frontend/src/components/`

| 文件 | 说明 |
|------|------|
| `TagTable.vue` | 点位表格组件：展示/编辑点位值，支持Bool开关/数字输入/字符串，2秒自动刷新 |

#### 视图：`Frontend/src/views/`

| 文件 | 说明 |
|------|------|
| `PlcListView.vue` | PLC列表页：网格卡片布局，显示名称/IP/状态，连接/断开/移除操作，添加PLC弹窗 |
| `PlcDetailView.vue` | PLC详情页：左侧DB分组树（新建/重命名/删除），右侧点位管理（TagTable组件） |

---

## 框架引用（外部项目）

| 项目 | 路径 | 说明 |
|------|------|------|
| `Ddon.Desktop.Avalonia` | `../../src/Ddon.Desktop.Avalonia/` | Avalonia 桌面框架 |
| `Ddon.Desktop.Core` | `../../src/Ddon.Desktop.Core/` | 桌面核心库 |
| `Ddon.VitrinPLC` | `../../src/Ddon.VitrinPLC/` | PLC 交互库 |

---

## 构建流程

1. **NpmInstall** — 构建前自动执行 `npm install`（若 `node_modules` 不存在）
2. **NpmBuild** — Release 模式下构建前执行 `npm run build`（Vue 前端打包）
3. 前端资源输出到 `wwwroot/` 目录，由 .NET 应用托管

## 路由结构

| 路径 | 组件 | 说明 |
|------|------|------|
| `/` | — | 重定向至 `/main` |
| `/main` | `MainView.vue` | 主页面（含 PLC 管理导航卡片） |
| `/plc/list` | `PlcListView.vue` | PLC 列表（网格卡片，添加/连接/断开/移除） |
| `/plc/detail/:name` | `PlcDetailView.vue` | PLC 详情（DB分组Tab切换 + 点位管理） |
