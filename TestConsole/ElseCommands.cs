using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        partial class MyCommands
        {
            public partial class IfCommands
            {
                public class ElseCommands
                {
                    public CommandObject Root { get; }
                    public CommandObject<ElseCommands> Self { get; }
                    public bool Src { get; }
                    public bool ToEndIf { get; private set; }

                    public ElseCommands(CommandObject root, bool src)
                    {
                        Self = new CommandObject<ElseCommands>(this);
                        Root = root;
                        Src = src;
                    }

                    public void ProcessElse()
                    {
                        if (Src)
                        {
                            do
                            {
                                string cmdline = NextCommandString();
                                if (!Self.TryExecuteCommand(cmdline, true, out _) && !Root.CanExecuteCommand(cmdline, true))
                                    throw new SyntaxException();
                            }
                            while (!ToEndIf);
                        }
                        else
                        {
                            do
                            {
                                string cmdline = NextCommandString();
                                if (!Self.TryExecuteCommand(cmdline, true, out _))
                                    Root.ExecuteCommand(cmdline, true);
                            }
                            while (!ToEndIf);
                        }
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
