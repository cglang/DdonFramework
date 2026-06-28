# DdonFramework 架构文档

## 项目元信息
| 属性 | 值 |
|------|-----|
| 作者 | cglang |
| 仓库 | https://github.com/cglang/DdonFramework |
| 版本 | 7.0.12 |
| 许可 | Apache-2.0 |

## 构建系统
- **集中包管理**: `Directory.Packages.props` (`ManagePackageVersionsCentrally`)
- **公共属性**: `common.props` (TargetFramework=net7.0, Nullable=enable)
- **版本定义**: `version.props` (Version=7.0.12)
- **NuGet 发布**: `push_nupkg.ps1` 脚本

## 依赖层次 (已知部分)
```
Ddon.Pipeline
  └─ 被 Ddon.Socket / Ddon.Serial / Ddon.Core / Ddon.OpenProtocol 引用

Ddon.Core
  ├─ Ddon.DependencyInjection
  ├─ Ddon.Module
  ├─ Ddon.Pipeline
  ├─ MediatR
  └─ Microsoft.Extensions.*

Ddon.Serial
  ├─ Ddon.Pipeline
  └─ System.IO.Ports

Ddon.Socket
  └─ Ddon.Pipeline

Ddon.OpenProtocol
  ├─ Ddon.Pipeline
  └─ Ddon.Socket (TCP 传输)
```

## Pipeline 复用模式
多个通信模块（Socket、Serial、OpenProtocol、UniPLC）共享同一套 Pipeline 机制：
1. 定义上下文 Context 类
2. 定义中间件接口继承 `IGeneralPipelineMiddleware<Context>`
3. 使用 `GeneralCustomPipelineFactory` 构建管道
4. 通过 `PipelineRegistrar` 注册中间件

## 命名空间约定
- 扩展方法类统一放在 `Microsoft.Extensions.DependencyInjection` / `Microsoft.Extensions.Hosting` 命名空间
- 核心逻辑在各自模块命名空间内
- Pipeline 的决策管道（DecisionPipeline）支持条件分支执行

## 目标框架策略
| 框架 | 适用模块 |
|------|----------|
| netstandard2.0;net8.0 | Ddon.Pipeline, Ddon.Serial, Ddon.Socket, Ddon.OpenProtocol（多目标） |
| net7.0 | Ddon.Core 等（common.props 默认） |
