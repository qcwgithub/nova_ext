using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UpdateJavaScript
{
    class Program
    {
        static void CorrectJavaScriptYieldCode(string filePath)
        {
            string YIELD_DEF = "var $yield = [];"; // 删
            string YIELD_PUSH = "$yield.push"; // 替换为 "yield "
            string YIELD_RET = "return System.Array.toEnumerator($yield);"; // 删
            string FUN_DEC = "function ("; // 替换为 "function* ("

            StringBuilder sb = new StringBuilder();
            StringBuilder sbFail = new StringBuilder();

            bool suc = true;
            string str = File.ReadAllText(filePath);
            int lastIndex = 0, yildDefIndex, funStart = 0;
            int count = 0;
            while (true)
            {
                yildDefIndex = str.IndexOf(YIELD_DEF, lastIndex);
                if (yildDefIndex < 0) { break; }

                funStart = str.LastIndexOf(FUN_DEC, yildDefIndex);
                if (funStart < 0) { suc = false; break; }

                sb.Append(str.Substring(lastIndex, funStart - lastIndex));
                sb.Append("function* (");

                funStart += FUN_DEC.Length;
                lastIndex = str.IndexOf(YIELD_RET, yildDefIndex);
                if (lastIndex < 0) { suc = false; break; }
                lastIndex += YIELD_RET.Length;

                sb.Append(str.Substring(funStart, lastIndex - funStart).Replace(YIELD_DEF, "").Replace(YIELD_PUSH, "yield ").Replace(YIELD_RET, ""));
                count++;
            }
            if (suc)
            {
                if (count > 0)
                {
                    sb.Append(str.Substring(lastIndex));
                    File.WriteAllText(filePath, sb.ToString());
                }
            }
            else
            {
                sbFail.AppendLine();
                sbFail.Append(filePath);
            }

            if (sbFail.Length == 0)
                Console.WriteLine("成功，共替换" + count + "个函数");
            else
                Console.WriteLine("失败");
        }

        static void Main(string[] args)
        {
            string src = args[0];
            string dst = args[1];

            File.Copy(src, dst, true);
            CorrectJavaScriptYieldCode(dst);
        }
    }
}
