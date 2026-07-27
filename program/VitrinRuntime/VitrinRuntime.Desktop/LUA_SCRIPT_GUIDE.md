# Lua 脚本事件订阅指南

## 约定胜过配置

在同组目录下编写 `.lua` 脚本，定义约定名称的全局函数，引擎自动订阅系统事件并在事件触发时调用。

## 支持的事件

| Lua 函数名 | 对应 C# 事件 | 触发时机 |
|-----------|-------------|---------|
| `OnTagValueChanged` | `TagValueChangedEvent` | 点位值发生变化时 |

## Lua 脚本示例

```lua
-- 放在脚本组目录下的任意 .lua 文件
-- 函数名规则: On + 事件类型名(去掉 Event 后缀)

function OnTagValueChanged(data)
    -- data 可直接访问 .NET 属性
    print("点位变化: " .. data.TagName)
    print("旧值: " .. tostring(data.OldValue))
    print("新值: " .. tostring(data.NewValue))
    print("地址: " .. data.Address)
    print("类型: " .. data.DataType)
end
```

### 参数说明

`OnTagValueChanged` 的 `data` 参数包含以下属性：

| 属性 | 类型 | 说明 |
|------|------|------|
| `TagName` | string | 点位全名（格式: PLC名称.分组名称.点位名称） |
| `Address` | string | 地址 |
| `DataType` | string | 数据类型 |
| `OldValue` | object? | 变化前的值 |
| `NewValue` | object? | 变化后的值 |

## 工作原理

```
Lua VM (组内共享)
  │
  ├── 脚本加载 → 扫描全局表 _G
  │              找到 OnTagValueChanged 等函数
  │              自动订阅 GeneralEventBus.Default
  │
  └── 事件触发 → 带锁串行调用 Lua 函数
                 同一组内的事件回调不会并发执行
```

## 生命周期

| 操作 | 订阅行为 |
|------|---------|
| 加载脚本组 | 自动扫描并订阅所有 OnXxx 函数 |
| 重载脚本组 | 取消旧订阅 → 重新扫描并订阅 |
| 卸载脚本组 | 取消所有事件订阅 |
| 自动加载（重启） | 恢复持久化的组并重新订阅 |

## 注意事项

1. **同组脚本共享全局表**：不同文件中定义的同名 `OnXxx` 函数，后加载的会覆盖先加载的
2. **建议使用 `local` 做私有函数**：辅助函数用 `local function` 声明，不污染全局表
3. **线程安全**：同一组内的 Lua 函数调用已串行化，不会并发执行
