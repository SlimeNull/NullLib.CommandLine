using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        partial class MyCommands
        {
            public partial class IfCommands
            {
                public class ElseIfCommands
                {
                    public CommandObject Root { get; }
                    public CommandObject<ElseIfCommands> Self { get; }
                    public bool Src { get; }
                    public bool ExpSrc { get; }
                    public bool ToEndIf { get; private set; }

                    public ElseIfCommands(CommandObject root, bool src, bool expSrc)
                    {
                        Self = new CommandObject<ElseIfCommands>(this);
                        Root = root;
                        Src = src;
                        ExpSrc = expSrc;
                    }

                    public void ProcessElseIf()
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
                                    if (!Root.CanExecuteCommand(cmdline, true))
                                        throw new CommandEntryPointNotFoundException();
                                }
                            }
                            while (!ToEndIf);
                        }
                        else
                        {
                            if (ExpSrc)
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
                                    catch (CommandException)
                                    {
                                        if (!Root.CanExecuteCommand(cmdline, true))
                                            throw new CommandEntryPointNotFoundException();
                                    }
                                }
                                while (!ToEndIf);
                            }
                        }
                    }

                    public void ElseIfToDo(bool value)
                    {
                        ElseIfCommands elseIfCommands = new ElseIfCommands(Root, ExpSrc, value);
                        elseIfCommands.ProcessElseIf();
                        ToEndIf = true;
                    }

                    public void ElseToDo()
                    {
                        ElseCommands elseCommands = new ElseCommands(Root, Src);
                        elseCommands.ProcessElse();
                        ToEndIf = true;
                    }

                    #region ElseIfCommands_ElseIf
                    [Command]
                    public void ElseIf(ObjectComparison comparision, string param)
                    {
                        ElseIfToDo(IfCommands.CheckIf(comparision, param));
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
}
