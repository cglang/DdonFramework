using System.Threading.Tasks;

namespace Test.Core.TestInvokeClasses
{
    public class TestClass
    {
        public async Task<string> MethodAsync(string a) { await Task.Delay(1); return a; }
        public string MethodSync(string value) => value;


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
