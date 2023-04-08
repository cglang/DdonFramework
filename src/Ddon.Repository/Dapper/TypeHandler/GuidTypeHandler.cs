using System;
using System.Data;
using Dapper;

namespace Ddon.Repository.Dapper.TypeHandler;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid guid)
    {
        parameter.Value = guid.ToString();
    }

    public override Guid Parse(object value)
    {
        return new Guid((string)value);
    }
}

public class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    public override void SetValue(IDbDataParameter parameter, Guid? guid)
    {
        parameter.Value = guid?.ToString();
    }

    public override Guid? Parse(object value)
    {
        return new Guid((string)value);
    }
}

public class StringGuidHandler : SqlMapper.TypeHandler<string>
{
    public override void SetValue(IDbDataParameter parameter, string value)
    {
        parameter.Value = value;
    }

    public override string Parse(object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}
