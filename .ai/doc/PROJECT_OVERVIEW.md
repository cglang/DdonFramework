# DdonFramework 项目概览

## 项目定位
DdonFramework 是一个 .NET 模块化框架，提供通用业务基础设施能力，包含缓存、事件总线、身份认证、工作流、IoT 通信、Pipeline 管道、文件存储等模块。

## 整体架构
```
framework/
├── common.props          # 公共编译属性 (net7.0)
├── version.props         # 版本定义 (7.0.12)
├── Directory.Packages.props  # 集中包版本管理
├── src/                  # 33 个子项目
├── test/                 # 测试项目
└── nupkgs/               # NuGet 打包输出
```

## 模块列表 (src/)

| 模块 | 命名空间 | 说明 |
|------|----------|------|
| Ddon.Core | Ddon.Core | 核心模块，依赖 MediatR、Pipeline、DI |
| Ddon.Pipeline | Ddon.Pipeline | 通用 Pipeline 管道中间件框架 |
| Ddon.Serial | Ddon.Serial | **串口通信框架**，基于 Ddon.Pipeline |
| Ddon.Socket | Ddon.Socket | Socket 通信框架（Server+Client），基于 Ddon.Pipeline |
| Ddon.SimpeSocket | - | 简单 Socket 封装 |
| Ddon.Cache | Ddon.Cache | 缓存抽象 |
| Ddon.Cache.Memory | - | 内存缓存实现 |
| Ddon.Cache.Redis | - | Redis 缓存实现 |
| Ddon.EventBus | - | 事件总线抽象 |
| Ddon.EventBus.Memory | - | 内存事件总线 |
| Ddon.Repository | - | 仓储抽象 |
| Ddon.Repository.EntityFrameworkCore | - | EF Core 仓储实现 |
| Ddon.Domain | - | 领域层基类 |
| Ddon.Application | - | 应用层 |
| Ddon.AspNetCore | - | ASP.NET Core 集成 |
| Ddon.DependencyInjection | - | DI 扩展 |
| Ddon.Module | Ddon.Module | 模块化加载机制 |
| Ddon.Jwt | - | JWT 认证 |
| Ddon.Identity | - | 身份标识 |
| Ddon.Localizer | - | 本地化 |
| Ddon.Mail | - | 邮件发送 |
| Ddon.Schedule | - | 任务调度 |
| Ddon.Pipeline | Ddon.Pipeline | Pipeline 管道 |
| Ddon.FileStorage | - | 文件存储 |
| Ddon.UploadFile | - | 文件上传 |
| Ddon.KeyValueStorage | - | KV 存储 |
| Ddon.Plugin | - | 插件机制 |
| Ddon.Workflow | - | 工作流引擎 (WorkflowCore) |
| Ddon.UniPLC | - | 通用 PLC 通信框架 |
| Ddon.VitrinPLC | - | Vitrin PLC 专用 |
| Ddon.IoTClient | - | IoT 客户端 |
| Ddon.IoTDevice | - | IoT 设备端 |
| Ddon.Test | - | 测试基类 |

## 技术栈
- **语言**: C# (.NET Standard 2.0 / .NET 7.0 / .NET 8.0)
- **DI**: Microsoft.Extensions.DependencyInjection
- **Pipeline**: 自研 Ddon.Pipeline
- **ORM**: EntityFrameworkCore
- **事件**: MediatR
- **序列化**: MemoryPack, System.Text.Json
- **工作流**: WorkflowCore

## 关键设计约定
1. **Pipeline 模式**: 多个模块（Socket、Serial、UniPLC）复用 Ddon.Pipeline 实现中间件链
2. **集中包管理**: Directory.Packages.props 统一管理 NuGet 版本
3. **双属性文件**: common.props（net7.0）和 version.props（7.0.12）分离
4. **扩展方法命名空间**: 使用 `Microsoft.Extensions.DependencyInjection` 作为扩展方法注册命名空间
5. **NetStandard 兼容**: 部分库同时支持 netstandard2.0 + net8.0
