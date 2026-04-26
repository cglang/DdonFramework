namespace Ddon.UniPLC.Exceptions;

/// <summary>
/// PLC 通信异常基类
/// </summary>
public class PlcException : Exception
{
    public PlcException(string message) : base(message) { }
    public PlcException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// PLC 连接异常
/// </summary>
public class PlcConnectionException : PlcException
{
    public PlcConnectionException(string message) : base(message) { }
    public PlcConnectionException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// PLC 超时异常
/// </summary>
public class PlcTimeoutException : PlcException
{
    public PlcTimeoutException(string message) : base(message) { }
    public PlcTimeoutException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// PLC 地址异常
/// </summary>
public class PlcAddressException : PlcException
{
    public PlcAddressException(string message) : base(message) { }
    public PlcAddressException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// PLC 协议异常
/// </summary>
public class PlcProtocolException : PlcException
{
    public PlcProtocolException(string message) : base(message) { }
    public PlcProtocolException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// PLC 序列化异常
/// </summary>
public class PlcSerializationException : PlcException
{
    public PlcSerializationException(string message) : base(message) { }
    public PlcSerializationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
