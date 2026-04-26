using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.UniPLC;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void AddPlc_ShouldRegisterPlcProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPlc(builder => builder.UseMemory("TestPLC"));
        var provider = services.BuildServiceProvider();

        // Assert
        var plcProvider = provider.GetService<IPlcProvider>();
        Assert.IsNotNull(plcProvider);
    }

    [TestMethod]
    public void AddPlc_WithMemory_ShouldRegisterMemoryClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPlc(builder => builder.UseMemory("TestPLC"));
        var provider = services.BuildServiceProvider();
        var plcProvider = provider.GetService<IPlcProvider>();

        // Assert
        var client = plcProvider?.GetClient("TestPLC");
        Assert.IsNotNull(client);
        Assert.AreEqual("TestPLC", client.Name);
    }

    [TestMethod]
    public void AddPlc_WithSiemens_ShouldRegisterSiemensClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPlc(builder =>
        {
            builder.UseSiemens(options =>
            {
                options.Ip = "192.168.1.10";
                options.Port = 102;
            });
        });
        var provider = services.BuildServiceProvider();
        var plcProvider = provider.GetService<IPlcProvider>();

        // Assert
        var client = plcProvider?.GetClient("Siemens");
        Assert.IsNotNull(client);
        Assert.AreEqual("Siemens", client.Name);
    }

    [TestMethod]
    public void AddPlc_WithMultipleClients_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPlc(builder =>
        {
            builder.UseSiemens("PLC1", options => options.Ip = "192.168.1.10");
            builder.UseSiemens("PLC2", options => options.Ip = "192.168.1.20");
            builder.UseMemory("SimPLC");
        });
        var provider = services.BuildServiceProvider();
        var plcProvider = provider.GetService<IPlcProvider>();

        // Assert
        var plc1 = plcProvider?.GetClient("PLC1");
        var plc2 = plcProvider?.GetClient("PLC2");
        var simPlc = plcProvider?.GetClient("SimPLC");

        Assert.IsNotNull(plc1);
        Assert.IsNotNull(plc2);
        Assert.IsNotNull(simPlc);
        Assert.AreEqual("PLC1", plc1.Name);
        Assert.AreEqual("PLC2", plc2.Name);
        Assert.AreEqual("SimPLC", simPlc.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void GetClient_WithInvalidName_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPlc(builder => builder.UseMemory("TestPLC"));
        var provider = services.BuildServiceProvider();
        var plcProvider = provider.GetService<IPlcProvider>();

        // Act
        plcProvider?.GetClient("NonexistentPLC");
    }
}
