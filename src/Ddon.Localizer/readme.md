```
{
  "JsonLocalizerOptions": {
    "ResourcesPath": "Localization"
  }
}
```

JSON localization file (`Localization/zh-CN.json`):

```json
{
  "greeting": "你好",
  "farewell": "再见",
  "menu": {
    "file": "文件",
    "edit": "编辑"
  }
}
```

DI:

```csharp
// 通过配置文件
services.AddJsonLocalizer(configuration);

// 或代码配置
services.AddJsonLocalizer(o => o.ResourcesPath = "Lang");
```

Usage:

```csharp
public class MyService
{
    private readonly IStringLocalizer _localizer;

    public MyService(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public void Greet()
    {
        // "你好"
        var msg = _localizer["greeting"];

        // "你好, 张三"
        var formatted = _localizer["greeting", "张三"];

        // 嵌套对象 menu -> file → "文件"
        var fileMenu = _localizer["menu:file"];

        // 嵌套对象 menu -> edit → "编辑"
        var editMenu = _localizer["menu:edit"];
    }
}
```
