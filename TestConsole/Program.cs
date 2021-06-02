using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NullLib.ArgsParser;
using NullLib.CommandLine;

namespace TestConsole
{
    class Program
    {
        class MyCommands
        {
            [Command(typeof(FloatConverter))]      // 在这里添加属性以表示使用哪些转换器, 这里提供了一些基本类型的转换, 例如整数, 浮点数, 双精度浮点数, 枚举
            public float Plus(float a, float b = 5)
            {
                return a + b;
            }
            [Command(typeof(FloatConverter))]
            public float Plus(float a, float b = 5, float c = 6)
            {
                return a + b + c;
            }
            [Command(typeof(FloatConverter))]       // 转换器只需要继承 IArgumentConverter即可
            public float Mul(float a, float b)
            {
                return a * b;
            }
            [Command(typeof(FloatConverter))]
            public float Sub(float a, float b)
            {
                return a - b;
            }
            [Command(typeof(FloatConverter))]
            public float Div(float a, float b)
            {
                return a / b;
            }
            [Command(typeof(FloatConverter), null, null, typeof(ArgumentConverter))]
            public float Test(float a, float b, float c, string qwq)
            {
                Console.WriteLine(qwq);
                return a + b + c;
            }
            [Command(typeof(ForeachConverter<FloatConverter>))]
            public float Adds(params float[] nums)
            {
                return nums.Sum();
            }
        }

        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();       // 创建一个命令行对象
            IArgumentParser[] parsers = new IArgumentParser[]
            {
                new PropertyArgumentParser("-"),
                new FieldArgumentParser(':'),
                new StringArgumentParser(),
            };

            Console.WriteLine("Easy command. Copyright 2021 Null.\n");
            while (true)
            {
                Console.Write(">>> ");
                string cmdline = Console.ReadLine();
                if (cmdline == null)
                    return;
                if (string.IsNullOrWhiteSpace(cmdline))
                    continue;
                if (!obj.TryExecuteCommand(parsers, cmdline, true, out object result))
                    Console.Error.WriteLine("Syntax error: can't execute command.");
                if (result != null)
                    Console.WriteLine(result);
            }
        }
    }
}
