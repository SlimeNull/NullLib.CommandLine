using System;
using System.Linq;
using NullLib.ArgsParser;
using NullLib.CommandLine;

namespace TestConsole
{
    class Program
    {
        class MyCommands
        {
            [Command(typeof(FloatConverter), typeof(FloatConverter))]      // 在这里添加属性以表示使用哪些转换器, 这里提供了一些基本类型的转换, 例如整数, 浮点数, 双精度浮点数, 枚举
            public float Plus(float a, float b)
            {
                return a + b;
            }
            [Command(typeof(FloatConverter), typeof(FloatConverter), typeof(FloatConverter))]
            public float Plus(float a, float b, float c)
            {
                return a + b + c;
            }
            [Command(typeof(FloatConverter), typeof(FloatConverter))]       // 转换器只需要继承 IArgumentConverter即可
            public float Mul(float a, float b)
            {
                return a * b;
            }
            [Command(typeof(FloatConverter), typeof(FloatConverter))]
            public float Sub(float a, float b)
            {
                return a - b;
            }
            [Command(typeof(FloatConverter), typeof(FloatConverter))]
            public float Test(float a, float b = 2)
            {
                return a * b;
            }
            public float TTT(params float[] qwq)
            {
                return qwq.Sum();
            }
        }
        static void MyFunc(int c, int a = 0, int b = 0, params int[] qwq)
        {

        }
        static void Main(string[] args)
        {
            goto Main;
            Console.WriteLine(typeof(MyCommands).GetMethod("TTT").Invoke(new MyCommands(), new object[] { 12.1, 456.6, 894.456 }));

            while (true)
            {
                foreach (var i in CommandParser.ParseArguments(CommandParser.SplitCommandLine(Console.ReadLine())))
                {
                    if (i is NamedArgument named)
                        Console.WriteLine($"{i.GetType()}: {named.Name}: {named.Content}");
                    else if (i is NullLib.CommandLine.StringArgument awa)
                        Console.WriteLine($"{i.GetType()}: {awa.Content}");
                }
            }

            Main:
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();       // 创建一个命令行对象
            IArgumentParser[] parsers = new IArgumentParser[]
            {
                new PropertyArgumentParser("-"),
                new FieldArgumentParser(':'),
                new StringArgumentParser(),
            };
            while (true)
            {
                obj.TryExecuteCommand(parsers, Console.ReadLine(), true, out object result);       // 尝试执行
                Console.WriteLine($"Result: {result}\n");
            }
        }
    }
}
