using System;
using System.IO;
using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        partial class MyCommands
        {
            public CommandObject<MyCommands> Self { get; }
            public static object STDOUT
            {
                set
                {
                    if (value != null)
                        Console.WriteLine(value);
                }
            }

            public MyCommands()
            {
                Self = new CommandObject<MyCommands>(this);
            }

            public static string NextCommandString()
            {
                Console.Write(">>> ");
                return Console.ReadLine();
            }
            public enum ObjectComparison
            {
                EQ, NE, GT, LT, GE, LE, ET
            }

            [Command(typeof(FloatArguConverter), typeof(FloatArguConverter), Description = "")]      // the build-in ArgumentConverter in NullLib.CommandLine
            public float Plus(float a, float b)
            {
                return a + b;
            }
            [Command(typeof(FloatArguConverter))]        // if the following converters is same as the last one, you can ignore these
            public float Mul(float a, float b)
            {
                return a * b;
            }
            [Command(typeof(FloatArguConverter))]
            public float Div(
                [CommandArgu(CommandArguAlias = "awa")]
                float a,
                [CommandArgu(CommandArguAlias = "qwq")]
                float b)
            {
                return a / b;
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
                // 在这里, 这个方法有一个 Command 特性, 我有没有办法在这个方法体内直接获得这个特性的实例?
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


            void IfToDo(bool ok)
            {
                IfCommands ifCommands = new(Self, ok);
                ifCommands.ProcessIf();
            }
            public void If(ObjectComparison comparision, string param)
            {
                IfToDo(IfCommands.CheckIf(comparision, param));
            }
            [Command(typeof(IntArguConverter), typeof(EnumArguConverter<ObjectComparison>), typeof(IntArguConverter))]
            public void If(int num1, ObjectComparison comparision, int num2)
            {
                IfToDo(IfCommands.CheckIf(num1, comparision, num2));
            }
            [Command(typeof(FloatArguConverter), typeof(EnumArguConverter<ObjectComparison>), typeof(FloatArguConverter))]
            public void If(float num1, ObjectComparison comparision, float num2)
            {
                IfToDo(IfCommands.CheckIf(num1, comparision, num2));
            }
            [Command(typeof(DoubleArguConverter), typeof(EnumArguConverter<ObjectComparison>), typeof(DoubleArguConverter))]
            public void If(double num1, ObjectComparison comparision, double num2)
            {
                IfToDo(IfCommands.CheckIf(num1, comparision, num2));
            }
            [Command(typeof(ArguConverter), typeof(EnumArguConverter<ObjectComparison>), typeof(ArguConverter))]
            public void If(string str1, ObjectComparison comparision, string str2)
            {
                IfToDo(IfCommands.CheckIf(str1, comparision, str2));
            }
            [Command(typeof(ForeachArguConverter<ArguConverter>))]
            public void Test(params string[] objs)
            {
                foreach (var i in objs)
                    Console.WriteLine(i);
            }
            [Command(typeof(IntArguConverter))]
            public void Exit(int exitCode = 0)
            {
                Environment.Exit(exitCode);
            }

            [Command(CommandName = "$", CommandAlias = "#")]
            public void FuckYouWorld()
            {
                Console.WriteLine("Fuck you world");
            }
        }
    }
}
