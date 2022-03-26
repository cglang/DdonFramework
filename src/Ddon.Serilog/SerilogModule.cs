﻿using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Serilog
{
    public class SerilogModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(configuration);
        }
    }
}
