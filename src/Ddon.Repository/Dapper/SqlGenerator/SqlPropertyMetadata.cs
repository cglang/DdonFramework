using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Ddon.Repository.Dapper.SqlGenerator;

public sealed class SqlPropertyMetadata
{
    public SqlPropertyMetadata(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        var alias = PropertyInfo.GetCustomAttribute<ColumnAttribute>();
        if (alias != null && !string.IsNullOrEmpty(alias.Name))
        {
            Alias = alias.Name;
            ColumnName = Alias;
        }
        else
        {
            ColumnName = PropertyInfo.Name;
        }

        IsNullable = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private PropertyInfo PropertyInfo { get; }

    private string? Alias { get; set; }

    public string ColumnName { get; set; }

    public bool IsNullable { get; set; }

    public string PropertyName => PropertyInfo.Name;
}
