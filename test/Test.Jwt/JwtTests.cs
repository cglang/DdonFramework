using Ddon.Jwt;
using Ddon.Jwt.Options;
using NUnit.Framework;
using System;

namespace Test.Jwt;

public class JwtTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GenerateJwtTokenTest()
    {
        var set = new JwtSettings()
        {
            SecurityKey = Guid.NewGuid().ToString(),
            ExpiresIn = new TimeSpan(1, 2, 3, 4),
            TokenCacheExpiration = new TimeSpan(1, 0, 0)
        };
        var jwt = new JwtTokenManager(set);
        var token = jwt.GenerateJwtToken();
        Assert.True(!token.IsNullOrEmpty());
    }

    [Test]
    public void CheckJwtTokenTest()
    {
        var set = new JwtSettings()
        {
            SecurityKey = Guid.NewGuid().ToString(),
            ExpiresIn = new TimeSpan(1, 2, 3, 4),
            TokenCacheExpiration = new TimeSpan(1, 0, 0)
        };
        var jwt = new JwtTokenManager(set);
        var token = jwt.GenerateJwtToken();
        Assert.True(!token.IsNullOrEmpty());

        var decide = jwt.ValidateToken(token);
        Assert.True(decide);
    }
}