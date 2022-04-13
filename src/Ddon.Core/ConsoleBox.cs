using System;
using System.Text;

namespace Ddon.Core
{
    public static class ConsoleBox
    {
        public static void WriteBoxList(
            string str,
            ConsoleColor borderColor = ConsoleColor.White,
            ConsoleColor fontColor = ConsoleColor.White)
        {
            WriteBoxList(new string[] { str }, true, borderColor, fontColor);
        }

        public static void WriteBoxList(
            string[] strs,
            bool bodyBorder = false,
            ConsoleColor borderColor = ConsoleColor.White,
            ConsoleColor fontColor = ConsoleColor.White)
        {
            WriteBoxTable(RowToArrT(strs, strs.Length), bodyBorder, borderColor, fontColor);
        }

        public static void WriteBoxTable(
            string[,] strTable,
            bool bodyBorder = false,
            ConsoleColor borderColor = ConsoleColor.White,
            ConsoleColor fontColor = ConsoleColor.White)
        {
            var columns = strTable.GetLength(0);
            var rows = strTable.Length / columns;

            int[] xMaxLength = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                for (int o = 0; o < columns; o++)
                {
                    var len = strTable[o, i].GetByteLength() + 1;
                    if (xMaxLength[i] < len) xMaxLength[i] = len;
                }
            }

            WriteLineColorChar(BorderLine("┌", "┬", "┐", xMaxLength), borderColor);
            for (int i = 0; i < columns; i++)
            {
                WriteColorChar("│", borderColor);
                for (int o = 0; o < rows; o++)
                {
                    WriteColorChar($" {strTable[i, o]}{new string(' ', xMaxLength[o] - strTable[i, o].GetByteLength() - 1)}", fontColor);
                    WriteColorChar("│", borderColor);
                }
                Console.WriteLine();

                if (i != columns - 1 && bodyBorder || i == 0 && columns > 1)
                {
                    WriteLineColorChar(BorderLine("├", "┼", "┤", xMaxLength), borderColor);
                }
            }
            WriteLineColorChar(BorderLine("└", "┴", "┘", xMaxLength), borderColor);
        }

        private static string BorderLine(string start, string middle, string end, int[] maxLength)
        {
            StringBuilder line = new(start);
            for (int i = 0; i < maxLength.Length; i++)
            {
                line.Append($"{new string('─', maxLength[i])}");
                line.Append(i != maxLength.Length - 1 ? middle : end);
            }
            return line.ToString();
        }

        private static void WriteColorChar(string str, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void WriteLineColorChar(string str, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 字符串长度(按字节算)
        /// </summary>
        private static int GetByteLength(this string str)
        {
            int len = 0;
            byte[] b;

            for (int i = 0; i < str.Length; i++)
            {
                b = Encoding.Default.GetBytes(str.Substring(i, 1));
                len += b.Length > 1 ? 2 : 1;
            }

            return len;
        }

        /// <summary>
        /// 一维数组转二维数组
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="vec">一维数组</param>
        /// <param name="row">二维数组行数</param>
        private static T[,] RowToArrT<T>(T[] vec, int row)
        {
            if (vec.Length % row != 0) return new T[0, 0];
            int col = vec.Length / row;
            T[,] ret = new T[row, col];
            for (int i = 0; i < vec.Length; i++)
            {
                ret[i / col, i % col] = vec[i];
            }
            return ret;
        }
    }
}
