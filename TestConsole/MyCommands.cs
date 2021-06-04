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
            public enum ObjectComparision
            {
                EQ, NE, GT, LT, GE, LE,  ET
            }

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


            void IfToDo(bool ok)
            {
                IfCommands ifCommands = new IfCommands(Self, ok);
                ifCommands.ProcessIf();
            }
            public void If(ObjectComparision comparision, string param)
            {
                IfToDo(comparision switch
                {
                    ObjectComparision.ET => File.Exists(param),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            [Command(typeof(IntArguConverter), typeof(EnumArguConverter<ObjectComparision>), typeof(IntArguConverter))]
            public void If(int num1, ObjectComparision comparision, int num2)
            {
                IfToDo(comparision switch
                {
                    ObjectComparision.EQ => num1 == num2,
                    ObjectComparision.NE => num1 != num2,
                    ObjectComparision.GT => num1 > num2,
                    ObjectComparision.LT => num1 < num2,
                    ObjectComparision.GE => num1 >= num2,
                    ObjectComparision.LE => num1 <= num2,
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            [Command(typeof(FloatArguConverter), typeof(EnumArguConverter<ObjectComparision>), typeof(FloatArguConverter))]
            public void If(float num1, ObjectComparision comparision, float num2)
            {
                IfToDo(comparision switch
                {
                    ObjectComparision.EQ => num1 == num2,
                    ObjectComparision.NE => num1 != num2,
                    ObjectComparision.GT => num1 > num2,
                    ObjectComparision.LT => num1 < num2,
                    ObjectComparision.GE => num1 >= num2,
                    ObjectComparision.LE => num1 <= num2,
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            [Command(typeof(DoubleArguConverter), typeof(EnumArguConverter<ObjectComparision>), typeof(DoubleArguConverter))]
            public void If(double num1, ObjectComparision comparision, double num2)
            {
                IfToDo(comparision switch
                {
                    ObjectComparision.EQ => num1 == num2,
                    ObjectComparision.NE => num1 != num2,
                    ObjectComparision.GT => num1 > num2,
                    ObjectComparision.LT => num1 < num2,
                    ObjectComparision.GE => num1 >= num2,
                    ObjectComparision.LE => num1 <= num2,
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            [Command(typeof(ArguConverter), typeof(EnumArguConverter<ObjectComparision>), typeof(ArguConverter))]
            public void If(string str1, ObjectComparision comparision, string str2)
            {
                IfToDo(comparision switch
                {
                    ObjectComparision.EQ => str1 == str2,
                    ObjectComparision.NE => str1 != str2,
                    _ => throw new ArgumentOutOfRangeException()
                });
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
        }
    }
}
