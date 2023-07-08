﻿using System;
using Ddon.Core.Services.IdWorker.Guids;
using Ddon.Core.Services.IdWorker.Snowflake;

namespace Ddon.Core.Services.IdWorker
{
    public class IdGenerator : IIdGenerator
    {
        private readonly ISnowflakeGenerator snowflakeGenerator;
        private readonly Guids.IGuidGenerator guidGenerator;

        public IdGenerator(Guids.IGuidGenerator guidGenerator, ISnowflakeGenerator snowflakeGenerator)
        {
            this.snowflakeGenerator = snowflakeGenerator;
            this.guidGenerator = guidGenerator;
        }

        public Guid CreateGuid() => guidGenerator.Create();

        public long CreateId() => snowflakeGenerator.NextId();
    }
}
