using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Ddon.Desktop.Core.Annotations;

namespace Ddon.Desktop.Core.Bridge;

public class BridgeDispatcher : IBridgeDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<string, MethodEntry?> _methods = new();

    public BridgeDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<object?> DispatchAsync(string method, object? payload)
    {
        var entry = _methods.GetOrAdd(method, _ => FindMethod(method));

        if (entry is null)
            throw new InvalidOperationException($"Bridge method '{method}' not found");

        var service = _serviceProvider.GetService(entry.ServiceType)
            ?? throw new InvalidOperationException($"Service '{entry.ServiceType.Name}' not registered");

        var args = DeserializeArgs(entry.Method, payload);
        var result = entry.Method.Invoke(service, args);

        if (result is not Task task) 
            return result;
        
        await task;
        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty?.GetValue(task);
    }

    private static object?[] DeserializeArgs(MethodInfo method, object? payload)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
            return [];

        if (payload is null)
            return [.. parameters.Select(p => p.DefaultValue)];

        if (parameters.Length == 1)
        {
            var raw = RawValue(payload, parameters[0].ParameterType);
            return [raw];
        }

        if (payload is not JsonElement je)
            throw new InvalidOperationException("Multi-parameter methods require a JSON object payload");

        var args = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (je.TryGetProperty(param.Name!, out var prop))
                args[i] = JsonSerializer.Deserialize(prop.GetRawText(), param.ParameterType, _jsonOptions);
            else
                args[i] = param.DefaultValue;
        }
        return args;
    }

    private static object? RawValue(object? value, Type targetType)
    {
        switch (value)
        {
            case null:
                return null;
            case JsonElement je:
                return JsonSerializer.Deserialize(je.GetRawText(), targetType, _jsonOptions);
        }

        if (targetType.IsInstanceOfType(value))
            return value;
        return JsonSerializer.Deserialize(JsonSerializer.Serialize(value), targetType, _jsonOptions);
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static MethodEntry? FindMethod(string fullName)
    {
        string? serviceFilter = null;
        string methodFilter;

        var dot = fullName.IndexOf('.');
        if (dot > 0)
        {
            serviceFilter = fullName[..dot];
            methodFilter = fullName[(dot + 1)..];
        }
        else
        {
            methodFilter = fullName;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            Type[] types;
            try
            {
                types = assembly.GetExportedTypes();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                var serviceAttr = type.GetCustomAttribute<BridgeServiceAttribute>();
                if (serviceAttr is null)
                    continue;

                var serviceName = serviceAttr.Name ?? type.Name;

                if (serviceFilter is not null &&
                    !serviceName.Equals(serviceFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var methodAttr = method.GetCustomAttribute<BridgeMethodAttribute>();
                    var methodName = methodAttr?.Name ?? method.Name;

                    if (methodName.Equals(methodFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        return new MethodEntry(type, method);
                    }
                }
            }
        }
        return null;
    }

    private record MethodEntry(Type ServiceType, MethodInfo Method);
}
