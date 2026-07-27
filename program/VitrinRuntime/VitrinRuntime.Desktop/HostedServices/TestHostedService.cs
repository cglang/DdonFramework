using Ddon.LuaEngine;
using Microsoft.Extensions.Hosting;

namespace VitrinRuntime.Desktop.HostedServices
{
    public class TestHostedService : BackgroundService
    {
        private readonly ILuaScriptManager luaScriptManager;
        private readonly ILuaVmManager luaVmManager;

        public TestHostedService(ILuaScriptManager luaScriptManager, ILuaVmManager luaVmManager)
        {
            this.luaScriptManager = luaScriptManager;
            this.luaVmManager = luaVmManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //luaScriptManager.LoadScriptsFromDirectory("D:\\CGL\\source\\LuaScripts");

            //var vms = luaVmManager.GetAllVms();
            //foreach (var vm in vms)
            //{
            //    var lua = vm.Value;
            //    //注册C#对象给Lua
            //    var api = new GameApi();
            //    lua["api"] = api;

            //    // 调用Lua函数
            //    var result = lua.GetFunction("Add").Call(10, 20);
            //    Console.WriteLine("Add结果:" + result[0]);

            //    // 调用游戏启动
            //    lua.GetFunction("OnGameStart").Call();
            //}
        }
    }

    public class GameApi
    {
        public void Log(string message)
        {
            Console.WriteLine("[Lua Log] " + message);
        }


        public void ShowMessage(string message)
        {
            Console.WriteLine("[Message] " + message);
        }
    }

    public class Player
    {
        public string Name { get; set; }

        public int HP { get; set; }


        public void Print()
        {
            Console.WriteLine(
                $"Player:{Name}, HP:{HP}"
            );
        }
    }
}
