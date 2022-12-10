using System.Threading.Tasks;
using Ddon.Cache;
using Ddon.Core;
using Ddon.Jwt;
using Ddon.Jwt.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ddon.Identity
{
    public class JwtModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);

            services.AddTransient<JwtTokenManager>();

            var jwtJwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new();
            services.AddSingleton(jwtJwtOptions);

            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            authenticationBuilder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 颁发者;默认会开启验证颁发者,可通过设置 ValidateIssuer = false 关闭验证,并删除此项
                    ValidIssuer = jwtJwtOptions.Issuer,
                    // 受众;默认会开启验证受众,可通过设置 ValidateAudience = false 关闭验证,并删除此项
                    ValidAudience = jwtJwtOptions.Audience,
                    // 检查签名密钥
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtJwtOptions.SecurityKey,
                    // 是否验证Token有效期
                    ValidateLifetime = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[jwtJwtOptions.CookieKey] ?? string.Empty;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpContextAccessor();
        }
    }
}
