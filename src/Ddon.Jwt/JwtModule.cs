using Ddon.Cache;
using Ddon.Core;
using Ddon.Jwt.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Ddon.Identity
{
    public class JwtModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);

            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            services.AddSingleton(jwtSettings!);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,      // 是否在令牌验证期间验证颁发者
                ValidateAudience = false,   // 是否在令牌验证期间验证受众
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings!.SecurityKey)),
                ValidateLifetime = true,                    // 是否验证Token有效期
                ClockSkew = TimeSpan.FromMilliseconds(5),   // Token缓冲过期时间
            };
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => { options.TokenValidationParameters = tokenValidationParameters; });
        }
    }
}
