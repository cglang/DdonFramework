```
{
  "CacheRedisOptions": {
    "Configuration": "127.0.0.1:6379,abortConnect=False",
    "InstanceName": "YourProject:"
  }
}
```

DI:

```csharp
// 通过配置文件
services.AddRedisCache(configuration);

// 或代码配置
services.AddRedisCache(o =>
{
    o.Configuration = "127.0.0.1:6379,abortConnect=False";
    o.InstanceName = "YourProject:";
});
```
