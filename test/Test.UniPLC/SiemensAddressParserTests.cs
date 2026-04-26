using Ddon.UniPLC.Clients.Siemens;
using Ddon.UniPLC.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.UniPLC;

[TestClass]
public class SiemensAddressParserTests
{
    [TestMethod]
    public void Parse_DBXAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "DB1.DBX0.0";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("DB", result.Area);
        Assert.AreEqual(1, result.BlockNumber);
        Assert.AreEqual(0, result.Offset);
        Assert.AreEqual(0, result.Bit);
        Assert.AreEqual(PlcDataType.Bool, result.DataType);
    }

    [TestMethod]
    public void Parse_DBWAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "DB1.DBW4";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("DB", result.Area);
        Assert.AreEqual(1, result.BlockNumber);
        Assert.AreEqual(4, result.Offset);
        Assert.AreEqual(PlcDataType.UShort, result.DataType);
    }

    [TestMethod]
    public void Parse_DBDAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "DB1.DBD8";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("DB", result.Area);
        Assert.AreEqual(1, result.BlockNumber);
        Assert.AreEqual(8, result.Offset);
        Assert.AreEqual(PlcDataType.Int, result.DataType);
    }

    [TestMethod]
    public void Parse_MXAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "M0.0";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("M", result.Area);
        Assert.AreEqual(0, result.Offset);
        Assert.AreEqual(0, result.Bit);
        Assert.AreEqual(PlcDataType.Bool, result.DataType);
    }

    [TestMethod]
    public void Parse_MAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "M100";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("M", result.Area);
        Assert.AreEqual(100, result.Offset);
        Assert.AreEqual(PlcDataType.Byte, result.DataType);
    }

    [TestMethod]
    public void Parse_IXAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "I1.5";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("I", result.Area);
        Assert.AreEqual(1, result.Offset);
        Assert.AreEqual(5, result.Bit);
        Assert.AreEqual(PlcDataType.Bool, result.DataType);
    }

    [TestMethod]
    public void Parse_QXAddress_ShouldParseCorrectly()
    {
        // Arrange
        var address = "Q2.3";

        // Act
        var result = SiemensAddressParser.Parse(address);

        // Assert
        Assert.AreEqual("Q", result.Area);
        Assert.AreEqual(2, result.Offset);
        Assert.AreEqual(3, result.Bit);
        Assert.AreEqual(PlcDataType.Bool, result.DataType);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Parse_InvalidAddress_ShouldThrow()
    {
        // Arrange
        var address = "Invalid.Address";

        // Act
        SiemensAddressParser.Parse(address);
    }
}
