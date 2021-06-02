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
            [Command(typeof(FloatArguConverter), typeof(FloatArguConverter))]      // the build-in ArgumentConverter in NullLib.CommandLine
            public float Plus(float a, float b)
            {
                return a + b;
            }
            [Command(typeof(FloatArguConverter))]        // if the following converters is same as the last one, you can ignore these
            public float Mul(float a, float b)
            {
                return a * b;
            }
            [Command(typeof(DoubleArguConverter))]
            public double Log(double n, double newBase = Math.E)    // you can also use optional parameter
            {
                return Math.Log(n, newBase);
            }
            [Command(typeof(ForeachArguConverter<FloatArguConverter>))]   // each string of array will be converted by FloatConverter
            public float Sum(params float[] nums)                 // variable length parameter method is supported
            {
                float result = 0;
                foreach (var i in nums)
                    result += i;
                return result;
            }
            [Command(typeof(ArguConverter))]        // if don't need to do any convertion, specify an 'ArgumentConverter'
            public void Print(string txt)
            {
                Console.WriteLine(txt);
            }
            [Command]                                   // the defualt converter is 'ArgumentConverter', you can ignore these
            public bool StringEquals(string txt1, string txt2)   // or specify 'null' to use the last converter (here is ArgumentConverter)
            {
                return txt1.Equals(txt2);
            }
            [Command(typeof(EnumArguConverter<ConsoleColor>))]   // EnumConverter helps convert string to Enum type automatically.
            public void SetBackground(ConsoleColor color)
            {
                Console.BackgroundColor = color;
            }

        }

        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();       // 创建一个命令行对象
            IArgumentParser[] parsers = new IArgumentParser[]
            {
                new PropertyArgumentParser("-"),
                new FieldArgumentParser(':'),
                new ArgumentParser(),
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
