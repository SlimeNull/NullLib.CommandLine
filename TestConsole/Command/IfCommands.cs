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

                public static bool CheckIf(ObjectComparison comparision, string param)
                {
                    return comparision switch
                    {
                        ObjectComparison.ET => File.Exists(param),
                        _ => false
                    };
                }
                public static bool CheckIf(int num1, ObjectComparison comparision, int num2)
                {
                    return comparision switch
                    {
                        ObjectComparison.EQ => num1 == num2,
                        ObjectComparison.NE => num1 != num2,
                        ObjectComparison.GT => num1 > num2,
                        ObjectComparison.LT => num1 < num2,
                        ObjectComparison.GE => num1 >= num2,
                        ObjectComparison.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(float num1, ObjectComparison comparision, float num2)
                {
                    return comparision switch
                    {
                        ObjectComparison.EQ => num1 == num2,
                        ObjectComparison.NE => num1 != num2,
                        ObjectComparison.GT => num1 > num2,
                        ObjectComparison.LT => num1 < num2,
                        ObjectComparison.GE => num1 >= num2,
                        ObjectComparison.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(double num1, ObjectComparison comparision, double num2)
                {
                    return comparision switch
                    {
                        ObjectComparison.EQ => num1 == num2,
                        ObjectComparison.NE => num1 != num2,
                        ObjectComparison.GT => num1 > num2,
                        ObjectComparison.LT => num1 < num2,
                        ObjectComparison.GE => num1 >= num2,
                        ObjectComparison.LE => num1 <= num2,
                        _ => false
                    };
                }
                public static bool CheckIf(string str1, ObjectComparison comparision, string str2)
                {
                    return comparision switch
                    {
                        ObjectComparison.EQ => str1 == str2,
                        ObjectComparison.NE => str1 != str2,
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
                                STDOUT = Self.ExecuteCommand(cmdline, true);
                            }
                            catch (CommandException)
                            {
                                STDOUT = Root.ExecuteCommand(cmdline, true);
                            }
                        }
                        while (!ToEndIf);
                    }
                    else
                    {
                        do
                        {
                            string cmdline = NextCommandString();
                            try
                            {
                                STDOUT = Self.ExecuteCommand(cmdline, true);
                            }
                            catch(CommandException)
                            {
                                if(!Root.CanExecuteCommand(cmdline, true))
                                    throw new CommandEntryPointNotFoundException();
                            }
                        }
                        while (!ToEndIf);
                    }
                }
                #region IfCommands_ElseIf
                [Command]
                public void ElseIf(ObjectComparison comparision, string param)
                {
                    ElseIfToDo(CheckIf(comparision, param));
                }
                [Command]
                public void ElseIf(int num1, ObjectComparison comparision, int num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(float num1, ObjectComparison comparision, float num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(double num1, ObjectComparison comparision, double num2)
                {
                    ElseIfToDo(CheckIf(num1, comparision, num2));
                }
                [Command]
                public void ElseIf(string str1, ObjectComparison comparision, string str2)
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
