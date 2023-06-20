using System;
using System.Collections.Generic;

namespace Ddon.Core.Utility
{
    public abstract class ConsoleMenu
    {
        public abstract MenuItem ChildMenu { get; }

        public void Show()
        {
            Show(ChildMenu);
        }

        private void Show(MenuItem menu)
        {
            string[,] menuTalbe = new string[menu.ChildMenu!.Count + 2, 2];
            menuTalbe[0, 0] = "*";
            menuTalbe[0, 1] = menu.Name;
            for (var i = 0; i < menu.ChildMenu.Count; i++)
            {
                menuTalbe[i + 1, 0] = (i + 1).ToString();
                menuTalbe[i + 1, 1] = menu.ChildMenu[i].Name;
            }
            menuTalbe[menu.ChildMenu.Count + 1, 0] = "Q";
            menuTalbe[menu.ChildMenu.Count + 1, 1] = "返回";

            bool decide = true;
            while (decide)
            {
                Console.Clear();
                ConsoleBox.WriteBoxTable(menuTalbe);
                var op = Read();
                if (op.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    decide = false;
                }
                else
                {
                    try
                    {
                        var index = Convert.ToInt32(op) - 1;
                        if (menu.ChildMenu[index].Action == null && menu.ChildMenu[index].ChildMenu == null)
                        {
                            ConsoleBox.WriteBoxList("无任何操作", ConsoleColor.Green, ConsoleColor.Green);
                            Continue();
                        }
                        else if (menu.ChildMenu[index].Action != null)
                        {
                            menu.ChildMenu[index].Action?.Invoke();
                            Continue();
                        }
                        else if (menu.ChildMenu[index].ChildMenu != null)
                            Show(menu.ChildMenu[index]);
                    }
                    catch
                    {
                        ConsoleBox.WriteBoxList("输入错误", ConsoleColor.Red, ConsoleColor.Red);
                        Continue();
                    }
                }
            }
        }

        public static string Read()
        {
            Console.Write("选择操作或输入命令 > ");
            return Console.ReadLine() ?? string.Empty;
        }

        public static void Continue()
        {
            ConsoleBox.WriteBoxList("按任意键继续...", ConsoleColor.Green, ConsoleColor.Green);
            Console.ReadKey();
            Console.Clear();
        }
    }

    public class MenuItem
    {
        public MenuItem(string name, Action? action = null)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; set; }

        public Action? Action { get; set; }

        public List<MenuItem>? ChildMenu { get; set; }
    }
}
