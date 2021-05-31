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
            while (true)
            {
                obj.TryExecuteCommand(parsers, Console.ReadLine(), true, out object result);       // 尝试执行
                Console.WriteLine($"Result: {result}\n");
            }
        }
    }
}
