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

        public UserInfo TestMethod003(string UserName, int Age)
        {
            return new() { UserName = UserName, Age = Age };
        }
    }

    public class UserInfo
    {
        public string? UserName { get; set; }

        public int Age { get; set; }
    }
}
