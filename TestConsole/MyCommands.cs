using System;
using System.IO;
using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        class MyCommands
        {
            public CommandObject<MyCommands> CommandObject = new CommandObject<MyCommands>();

            public static string NextCommandString()
            {
                Console.Write(">>> ");
                return Console.ReadLine();
            }

            public class IfCommands
            {
                public CommandObject<IfCommands> CommandObject = new CommandObject<IfCommands>();
                public bool Src = false;
                public bool ToEndIf = false;

                public IfCommands(bool value)
                {
                    Src = value;
                }

                public void ElseIfToDo(bool value)
                {
                    if (Src)
                    {

                    }
                    else
                    {

                    }
                }

                [Command]
                public void ElseIf(ObjectComparision comparision, string param)
                {
                    ElseIfCommands.CommandObject.TargetInstance.ElseIf(comparision, param);
                }
                [Command]
                public void ElseIf(int num1, ObjectComparision comparision, int num2)
                {
                    ElseIfCommands.CommandObject.TargetInstance.ElseIf(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(float num1, ObjectComparision comparision, float num2)
                {
                    ElseIfCommands.CommandObject.TargetInstance.ElseIf(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(double num1, ObjectComparision comparision, double num2)
                {
                    ElseIfCommands.CommandObject.TargetInstance.ElseIf(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(string str1, ObjectComparision comparision, string str2)
                {
                    ElseIfCommands.CommandObject.TargetInstance.ElseIf(str1, comparision, str2);
                }
                [Command]
                public void Else()
                {
                    ElseCommands.CommandObject.TargetInstance.Else();
                }
                [Command]
                public void EndIf()
                {
                    ToEndIf = true;
                }
            }
            public class ElseIfCommands
            {
                public CommandObject<ElseIfCommands> CommandObject = new CommandObject<ElseIfCommands>();
                public bool Src;
                public bool ToEndIf;


                public void ElseIfToDo(bool ok)
                {
                    CommandObject.TargetInstance.Src = ok;
                    CommandObject.TargetInstance.ToEndIf = false;
                    if (ok)
                    {
                        do
                        {
                            string cmdline = NextCommandString();
                            if (!CommandObject.TryExecuteCommand(cmdline, true, out _))
                                CommandObject.ExecuteCommand(cmdline);
                        }
                        while (!CommandObject.TargetInstance.ToEndIf);
                    }
                    else
                    {
                        do
                        {
                            CommandObject.ExecuteCommand(NextCommandString(), true);
                        }
                        while (!CommandObject.TargetInstance.ToEndIf);
                    }
                }
                [Command]
                public void ElseIf(ObjectComparision comparision, string param)
                {
                    MyCommands.CommandObject.TargetInstance.If(comparision, param);
                }
                [Command]
                public void ElseIf(int num1, ObjectComparision comparision, int num2)
                {
                    MyCommands.CommandObject.TargetInstance.If(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(float num1, ObjectComparision comparision, float num2)
                {
                    MyCommands.CommandObject.TargetInstance.If(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(double num1, ObjectComparision comparision, double num2)
                {
                    MyCommands.CommandObject.TargetInstance.If(num1, comparision, num2);
                }
                [Command]
                public void ElseIf(string str1, ObjectComparision comparision, string str2)
                {
                    MyCommands.CommandObject.TargetInstance.If(str1, comparision, str2);
                }
                [Command]
                public void Else()
                {

                }
            }
            public class ElseCommands
            {
                public static CommandObject<ElseCommands> CommandObject = new CommandObject<ElseCommands>();
                public bool Src = false;
                public bool ToEndIf = false;

                [Command]
                public void EndIf()
                {
                    ToEndIf = true;
                }
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
                IfCommands.CommandObject.TargetInstance.Src = ok;
                IfCommands.CommandObject.TargetInstance.ToEndIf = false;
                if (ok)
                {
                    do
                    {
                        string cmdline = NextCommandString();
                        if (!CommandObject.TryExecuteCommand(cmdline, true, out _))
                            IfCommands.CommandObject.ExecuteCommand(cmdline, true);
                    }
                    while (!IfCommands.CommandObject.TargetInstance.ToEndIf);
                }
                else
                {
                    do
                    {
                        IfCommands.CommandObject.ExecuteCommand(NextCommandString(), true);
                    }
                    while (!IfCommands.CommandObject.TargetInstance.ToEndIf);
                }
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
