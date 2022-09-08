using System.Threading.Tasks;

namespace Test.Core.TestInvokeClasses
{
    public class TestClass
    {
        public async Task<string> TestMethod001(string a)
        {
            await Task.Delay(1);
            return a;
        }

        public string TestMethod002(string a)
        {
            return a;
        }
    }
}
