namespace Ddon.Core.Services.IdWorker.Guids
{
    public class SnowflakeGeneratorOptions
    {
        public SnowflakeGeneratorOptions()
        {
            WorkerId = WorkerId == default ? DefaultWorkerId : WorkerId;
        }

        public uint WorkerId { get; set; }

        public uint GetDefaultWorkerId()
        {
            return WorkerId == default ? DefaultWorkerId : WorkerId;
        }

        public const uint DefaultWorkerId = 1;
    }
}
