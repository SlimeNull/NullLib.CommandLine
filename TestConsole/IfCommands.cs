using System.IO;
using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        partial class MyCommands
        {
            public partial class IfCommands
            {
                public CommandObject Root { get; }
                public CommandObject<IfCommands> Self { get; }
                public bool Src { get; }
                public bool ToEndIf { get; private set; }

                public IfCommands(CommandObject root, bool value)
                {
                    Self = new CommandObject<IfCommands>(this);
                    Root = root;
                    Src = value;
                }

                public static bool CheckIf(ObjectComparision comparision, string param)
                {
                    return comparision switch
                    {
                        ObjectComparision.ET => File.Exists(param),
                        _ => false
                    };
                }
                public static bool CheckIf(int num1, ObjectComparision comparision, int num2)
                {
                    return comparision switch
                    {
                        ObjectComparision.EQ => num1 == num2,
                        ObjectComparision.NE => num1 != num2,
                        ObjectComparision.GT => num1 > num2,
                        ObjectComparision.LT => num1 < num2,
                        ObjectComparision.GE => num1 >= num2,
                        ObjectComparision.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(float num1, ObjectComparision comparision, float num2)
                {
                    return comparision switch
                    {
                        ObjectComparision.EQ => num1 == num2,
                        ObjectComparision.NE => num1 != num2,
                        ObjectComparision.GT => num1 > num2,
                        ObjectComparision.LT => num1 < num2,
                        ObjectComparision.GE => num1 >= num2,
                        ObjectComparision.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(double num1, ObjectComparision comparision, double num2)
                {
                    return comparision switch
                    {
                        ObjectComparision.EQ => num1 == num2,
                        ObjectComparision.NE => num1 != num2,
                        ObjectComparision.GT => num1 > num2,
                        ObjectComparision.LT => num1 < num2,
                        ObjectComparision.GE => num1 >= num2,
                        ObjectComparision.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(string str1, ObjectComparision comparision, string str2)
                {
                    return comparision switch
                    {
                        ObjectComparision.EQ => str1 == str2,
                        ObjectComparision.NE => str1 != str2,
                        _ => false
                    };
                }

                public void ElseIfToDo(bool value)
                {
                    ElseIfCommands elseIfCommands = new ElseIfCommands(Root, Src, value);
                    elseIfCommands.ProcessElseIf();
                    ToEndIf = true;
                }
                public void ElseToDo()
                {
                    ElseCommands elseCommands = new ElseCommands(Root, Src);
                    elseCommands.ProcessElse();
                    ToEndIf = true;
                }

                public void ProcessIf()
                {
                    if (Src)
                    {
                        do
                        {
                            string cmdline = NextCommandString();
                            try
                            {
                                Self.TryExecuteCommand(cmdline, true, out var rst);
                                STDOUT = rst;
                            }
                            if (!Self.TryExecuteCommand(cmdline, true, out _))
                                Root.ExecuteCommand(cmdline, true);
                        }
                        while (!ToEndIf);
                    }
                    else
                    {
                        do
                        {
                            string cmdline = NextCommandString();
                            if (!Self.TryExecuteCommand(cmdline, true, out _) && !Root.CanExecuteCommand(cmdline, true))
                                throw new SyntaxException();
                        }
                        while (!ToEndIf);
                    }
                }
                #region IfCommands_ElseIf
                [Command]
                public void ElseIf(ObjectComparision comparision, string param)
                {
                    ElseIfToDo(CheckIf(comparision, param));
                }
                [Command]
                public void ElseIf(int num1, ObjectComparision comparision, int num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(float num1, ObjectComparision comparision, float num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(double num1, ObjectComparision comparision, double num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(string str1, ObjectComparision comparision, string str2)
                {
                    ElseIfToDo(CheckIf(str1, comparision, str2));
                }
                #endregion
                [Command]
                public void Else()
                {
                    ElseToDo();
                }
                [Command]
                public void EndIf()
                {
                    ToEndIf = true;
                }
            }
        }
    }
}
