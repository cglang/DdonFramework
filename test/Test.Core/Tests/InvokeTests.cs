using Ddon.Core.Use.Reflection;
using Ddon.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.Threading.Tasks;
using Test.Core.TestInvokeClasses;

namespace Test.Core.Tests
{
    [TestClass]
    public class InvokeTests : TestBase<TestCoreModule>
    {
        /// <summary>
        /// ����ͬ�����첽����
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestInvokeBaseSyncOrAsyncMethod()
        {
            var obj = "result";

            // �첽��������
            var result1 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodAsync), obj);
            Assert.AreEqual(result1, obj);

            // ͬ����������
            var result = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSync), obj);
            Assert.AreEqual(result, obj);
        }

        /// <summary>
        /// ���Ե�������
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestInvokeBaseSingleParameter()
        {
            // �������� byte
            byte obj1 = 1;
            var result1 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSingleParameterByte), obj1);
            Assert.AreEqual(result1, obj1);

            // �������� int
            int obj2 = 1234;
            var result2 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSingleParameterInt), obj2);
            Assert.AreEqual(result2, obj2);

            // �������� long
            long obj3 = 1234;
            var result3 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSingleParameterLong), obj3);
            Assert.AreEqual(result3, obj3);

            // �������� short
            short obj4 = 1234;
            var result4 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSingleParameterLong), obj4);
            Assert.AreEqual(result4, obj4);

            // �������� string
            string obj5 = "text";
            var result5 = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodSingleParameterString), obj5);
            Assert.AreEqual(result5, obj5);
        }


        /// <summary>
        /// ���Զ������
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestInvokeBaseMultipleParameter()
        {
            var userInfo = new UserInfo() { UserName = "cglang", Age = 22 };
            var userInfoJson = JsonSerializer.Serialize(userInfo);

            var result = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodMultipleParameter), userInfoJson);

            var resultJson = JsonSerializer.Serialize(userInfo);
            Assert.AreEqual(resultJson, userInfoJson);
        }

        /// <summary>
        /// ���Ը��Ӷ���
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestInvokeBaseClassObject()
        {
            var userInfo = new UserInfo() { UserName = "cglang", Age = 22 };
            var userInfoJson = JsonSerializer.Serialize(userInfo);

            var result = await DdonInvoke.InvokeAsync(nameof(TestClass), nameof(TestClass.MethodClassObject), userInfoJson);

            var resultJson = JsonSerializer.Serialize(result);
            Assert.AreEqual(resultJson, userInfoJson);
        }
    }
}