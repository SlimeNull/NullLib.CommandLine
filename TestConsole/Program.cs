using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NullLib.ArgsParser;
using NullLib.CommandLine;

namespace TestConsole
{
    class Program
    {
        class MyCommands
        {
            [CommandOption(typeof(FloatConverter), typeof(FloatConverter))]      // 在这里添加属性以表示使用哪些转换器, 这里提供了一些基本类型的转换, 例如整数, 浮点数, 双精度浮点数, 枚举
            public float Plus(float a, float b)
            {
                return a + b;
            }
            [CommandOption(typeof(FloatConverter), typeof(FloatConverter))]       // 转换器只需要继承 IArgumentConverter即可
            public float Times(float a, float b)
            {
                return a * b;
            }
        }
        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();       // 创建一个命令行对象
            while (true)
            {
                string[] commandLine = ArgsParser.SplitArgs(Console.ReadLine());     // 读取一行, 并分割

                obj.TryExecuteCommand(commandLine, true, out object result);       // 尝试执行
                Console.WriteLine($"Result: {result}");
            }
        }
    }
}
