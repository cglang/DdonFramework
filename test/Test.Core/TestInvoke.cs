using Ddon.Core.Use.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Test.Core.TestInvokeClasses;
using Test.Repository;

namespace Test.Core
{
    [TestClass]
    public class TestInvoke : TestBase<TestCoreModule>
    {
        [TestMethod]
        public async Task TestInvokeBaseTest001()
        {
            var obj = "result";

            IDdonServiceInvoke serviceInvoke = ServiceProvider.LazyGetService<IDdonServiceInvoke>()!;
            var result = await serviceInvoke.IvnvokeAsync(nameof(TestClass), nameof(TestClass.TestMethod001), obj);
            Assert.AreEqual(result, obj);
        }

        [TestMethod]
        public async Task TestInvokeBaseTest002()
        {
            var obj = "result";

            IDdonServiceInvoke serviceInvoke = ServiceProvider.LazyGetService<IDdonServiceInvoke>()!;

            var result = await serviceInvoke.IvnvokeAsync(nameof(TestClass), nameof(TestClass.TestMethod002), obj);
            Assert.AreEqual(result, obj);
        }
    }
}