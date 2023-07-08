namespace Ddon.Core.Services.IdWorker.Snowflake;


public interface ISnowflakeGenerator
{
    long[] NextId(uint size);

    long NextId();
}

