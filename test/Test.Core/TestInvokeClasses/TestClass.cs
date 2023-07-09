using System.Threading.Tasks;

namespace Test.Core.TestInvokeClasses
{
    public class TestClass
    {
        public async Task<string> Method1Async(string p)
        {
            await Task.CompletedTask;
            return p;
        }

        public async Task Method2Async(string _)
        {
            await Task.CompletedTask;
        }

        public string Method1(string p)
        {
            return p;
        }

        public void Method2(string _)
        {
            return;
        }

        public int MethodSingleParameterInt(int number) => number;
        public long MethodSingleParameterLong(long number) => number;
        public short MethodSingleParameterShort(short number) => number;
        public byte MethodSingleParameterByte(byte numbber) => numbber;
        public string MethodSingleParameterString(string text) => text;


        public UserInfo MethodMultipleParameter(string UserName, int Age) => new() { UserName = UserName, Age = Age };

        public UserInfo MethodClassObject(UserInfo userInfo) => userInfo;
    }

    public class UserInfo
    {
        public string? UserName { get; set; }

        public int Age { get; set; }
    }
}
