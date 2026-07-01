```
{
  "SqliteCacheOptions": {
    "ConnectionString": "Data Source=cache.db"
  }
}
```

DI:

```csharp
services.AddSqliteCache(o => o.ConnectionString = "Data Source=cache.db");
```
