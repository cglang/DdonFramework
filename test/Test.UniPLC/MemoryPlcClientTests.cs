using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Clients;
using Ddon.UniPLC.DependencyInjection;
using Ddon.UniPLC.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.UniPLC;

[TestClass]
public class MemoryPlcClientTests
{
    [TestMethod]
    public async Task Connect_ShouldSetIsConnected()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);

        // Act
        await client.ConnectAsync();

        // Assert
        Assert.IsTrue(client.IsConnected);
    }

    [TestMethod]
    public async Task Disconnect_ShouldUnsetIsConnected()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        await client.DisconnectAsync();

        // Assert
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    public async Task WriteBytesAsync_WhenConnected_ShouldSucceed()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        var result = await client.WriteBytesAsync("Addr1", new byte[] { 1, 2, 3 });

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task WriteBytesAsync_WhenNotConnected_ShouldFail()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);

        // Act
        var result = await client.WriteBytesAsync("Addr1", new byte[] { 1, 2, 3 });

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task ReadBytesAsync_WhenConnected_ShouldReturnBytes()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();
        await client.WriteBytesAsync("Addr1", new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await client.ReadBytesAsync("Addr1", 3);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(3, result.Value.Length);
        Assert.AreEqual(1, result.Value[0]);
    }

    [TestMethod]
    public async Task WriteAsync_Int_ShouldSucceed()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        await client.WriteAsync("Addr1", 42);

        // Assert
        var value = await client.ReadAsync<int>("Addr1");
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public async Task WriteAsync_Float_ShouldSucceed()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        await client.WriteAsync("Addr1", 3.14f);

        // Assert
        var value = await client.ReadAsync<float>("Addr1");
        Assert.AreEqual(3.14f, value, 0.01f);
    }

    [TestMethod]
    public async Task WriteAsync_String_ShouldSucceed()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        await client.WriteAsync("Addr1", "Hello");

        // Assert
        var value = await client.ReadAsync<string>("Addr1");
        Assert.AreEqual("Hello", value);
    }

    [TestMethod]
    public async Task BatchReadAsync_ShouldReturnMultipleResults()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();
        await client.WriteAsync("Addr1", 10);
        await client.WriteAsync("Addr2", 20);
        await client.WriteAsync("Addr3", 30);

        // Act
        var results = await client.BatchReadAsync("Addr1", "Addr2", "Addr3");

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results[0].IsSuccess);
        Assert.IsTrue(results[1].IsSuccess);
        Assert.IsTrue(results[2].IsSuccess);
    }

    [TestMethod]
    public async Task PingAsync_WhenConnected_ShouldReturnTrue()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);
        await client.ConnectAsync();

        // Act
        var result = await client.PingAsync();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PingAsync_WhenNotConnected_ShouldReturnFalse()
    {
        // Arrange
        var options = new PlcOptions { Name = "Test" };
        var client = new MemoryPlcClient(options);

        // Act
        var result = await client.PingAsync();

        // Assert
        Assert.IsFalse(result);
    }
}
