using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Ddon.Repository.Dapper.SqlGenerator.Expressions;

public class GeneratorExpression<T> where T : class
{
    public static string TableName
    {
        get
        {
            var tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>();
            return tableAttribute == null ? typeof(T).Name : tableAttribute.Name;
        }
    }

    public static Dictionary<string, string> GetFields()
    {
        var props = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
            .Where(x => x.GetCustomAttributes(typeof(NotMappedAttribute), true).Length == 0)
            .Where(x => x.CanWrite);
        var result = new Dictionary<string, string>();
        foreach (var prop in props)
        {
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                continue;
            if (prop.GetCustomAttribute<ForeignKeyAttribute>() != null)
                continue;
            if (prop.PropertyType.GetCustomAttribute<TableAttribute>() != null)
                continue;

            var col = prop.GetCustomAttribute<ColumnAttribute>();
            result.Add(string.IsNullOrWhiteSpace(col?.Name) ? prop.Name : col.Name, prop.Name);
        }

        return result;
    }


    public static Dictionary<string, string> GetPrimaryKeys()
    {
        var props = typeof(T).GetProperties().Where(t => t.GetCustomAttribute<KeyAttribute>() != null);
        var result = new Dictionary<string, string>();
        foreach (var prop in props)
        {
            var col = prop.GetCustomAttribute<ColumnAttribute>();
            result.Add(string.IsNullOrWhiteSpace(col?.Name) ? prop.Name : col.Name, prop.Name);
        }

        return result;
    }
}
