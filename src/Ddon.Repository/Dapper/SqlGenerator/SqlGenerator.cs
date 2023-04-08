using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ddon.Repository.Dapper.SqlGenerator;

public partial class SqlGenerator<TEntity> where TEntity : class
{
    public SqlGenerator()
    {
        UseQuotationMarks ??= false;

        Initialize();
    }

    private void Initialize()
    {
        InitProperties();
    }

    private void InitProperties()
    {
        var entityType = typeof(TEntity);
        var entityTypeInfo = entityType.GetTypeInfo();
        var tableAttribute = entityTypeInfo.GetCustomAttribute<TableAttribute>();

        TableName = tableAttribute == null ? typeof(TEntity).Name : tableAttribute.Name;

        SqlProperties = entityType.GetProperties().Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any())
            .Select(p => new SqlPropertyMetadata(p)).ToArray();
    }

    public bool? UseQuotationMarks { get; set; }


    public PropertyInfo[] AllProperties { get; protected set; } = Array.Empty<PropertyInfo>();


    [MemberNotNullWhen(true, nameof(UpdatedAtProperty), nameof(UpdatedAtPropertyMetadata))]
    public bool HasUpdatedAt => UpdatedAtProperty != null;


    public PropertyInfo? UpdatedAtProperty { get; protected set; }


    public SqlPropertyMetadata? UpdatedAtPropertyMetadata { get; protected set; }


    [MemberNotNullWhen(true, nameof(IdentitySqlProperty))]
    public bool IsIdentity => IdentitySqlProperty != null;


    public string TableName { get; protected set; } = string.Empty;


    public SqlPropertyMetadata? IdentitySqlProperty { get; protected set; }

    public SqlPropertyMetadata[] SqlProperties { get; protected set; } = Array.Empty<SqlPropertyMetadata>();

    private static string ParameterSymbol => "@";
}
