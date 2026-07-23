# VitrinRuntime 项目文件地图

## 项目概述

**VitrinRuntime** 是一个基于 Avalonia UI + Vue 3 的桌面应用程序（上位机案例），采用 .NET 8 作为后端框架，Vue 3 + Element Plus 作为前端技术栈，通过嵌入式 WebView 方式集成前后端。

---

## 根目录

| 文件/目录 | 说明 |
|-----------|------|
| `VitrinRuntime.slnx` | 解决方案文件，引用框架层（Ddon.Desktop.Avalonia/Core/VitrinPLC）和本应用项目 |
| `ai.md` | AI Agent 执行入口文件 |
| `wookflow.zh.md` | 工作流说明文档 |

### 隐藏目录

#### `.ai/` — AI Agent 上下文目录

| 文件 | 说明 |
|------|------|
| `rules.md` | AI Agent 行为规则（5条） |
| `project/` | 项目上下文目录（当前为空） |

#### `.agents/` — Agent 技能目录

| 文件 | 说明 |
|------|------|
| `skills/` | 技能存放目录（当前为空） |

#### `.vs/` — Visual Studio 配置目录

- 添加西门子 PLC 管理功能后新增的目录和文件如下：

#### 服务层：`Services/`

| 文件 | 说明 |
|------|------|
| `Models.cs` | 数据模型：PlcConfig（PLC连接配置）、DbGroup（DB块分组）、TagConfig（点位配置） |
| `PlcConfigStore.cs` | 内存存储（ConcurrentDictionary）：PLC配置、分组、点位的CRUD |
| `PlcManagerService.cs` | Bridge Service（PlcManager）：ListPlcs/AddPlc/RemovePlc/ConnectPlc/DisconnectPlc/GetPlcStatus |
| `PlcDataService.cs` | Bridge Service（PlcData）：ListDbGroups/CreateDbGroup/DeleteDbGroup/RenameDbGroup/ListTags/AddTag/RemoveTag/ReadTag/WriteTag |

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

## 应用层：`VitrinRuntime.Desktop/`

### 配置文件

| 文件 | 说明 |
|------|------|
| `VitrinRuntime.Desktop.csproj` | 项目文件，TargetFramework: net8.0，引用 Ddon.Desktop.Avalonia、Ddon.VitrinPLC |
| `appsettings.json` | 应用配置：后端地址 `http://localhost:5000`，前端地址 `http://localhost:5173`，窗口标题/尺寸 |
| `app.manifest` | Windows 应用程序清单 |
| `App.axaml` | Avalonia XAML 应用入口 |
| `App.axaml.cs` | Avalonia 应用代码后置，配置 DI 容器：注册 `IPlcHub`、`PlcConfigStore`、`PlcManagerService`、`PlcDataService` |
| `Program.cs` | .NET 程序入口 |

### 前端：`Frontend/`

**技术栈：** Vue 3 + TypeScript + Vite + Element Plus + Vue Router

#### 配置文件

| 文件 | 说明 |
|------|------|
| `package.json` | 项目依赖：vue 3.5, vue-router 4.5, element-plus 2.9, vite 6.2, typescript 5.7 |
| `vite.config.ts` | Vite 构建配置 |
| `tsconfig.json` / `tsconfig.node.json` | TypeScript 编译配置 |
| `index.html` | HTML 入口 |
| `env.d.ts` | 环境类型声明 |
| `.editorconfig` / `.prettierrc` | 代码风格配置 |
| `AI_RULES.md` | 前端 AI 规则 |

#### 源码：`src/`

| 文件 | 说明 |
|------|------|
| `main.ts` | Vue 应用入口 |
| `App.vue` | Vue 根组件 |
| `bridge.ts` | 前后端桥接层（与 .NET 后端通信） |
| `router/index.ts` | Vue Router 路由配置：`/` → `/main`、`/plc/list` → PlcListView、`/plc/detail/:name` → PlcDetailView |
| `views/MainView.vue` | 主页面视图组件，含 PLC 管理导航入口 |

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
