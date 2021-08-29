using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NullLib.ArgsParser;
using NullLib.CommandLine;

namespace TestConsole
{
    partial class Program
    {
        static Program()
        {
            CommandParser.EscapeChar = '`';
        }
        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new();       // 创建一个命令行对象
            IArguParser[] parsers = new IArguParser[]
            {
                new PropertyArguParser("-"),
                new FieldArguParser(':'),
                new ArguParser(),
            };

            obj.CommandUnresolved += (_s, _e) =>
            {
                if (_e.CommandName != "FFF")
                    _e.Handled = true;
            };

            Console.WriteLine("Easy command. Copyright 2021 Null.\n");
            while (true)
            {
                string cmdline = MyCommands.NextCommandString();
                if (cmdline == null)
                    return;
                if (string.IsNullOrWhiteSpace(cmdline))
                    continue;
#if false
                var result = obj.ExecuteCommand(parsers, cmdline, true);
                if(result != null)
                    Console.WriteLine(result);
#else
                try
                {
                    var result = obj.ExecuteCommand(parsers, cmdline, true);
                    if(result != null)
                        Console.WriteLine(result);
                }
                catch(CommandException)
                {
                    Console.Error.WriteLine("Syntax error: can't execute command.");
                }
                catch(TargetInvocationException)
                {
                    Console.Error.WriteLine("Method error: exception thrown when execute method.");
                }
                catch(Exception)
                {
                    Console.Error.WriteLine("Unexpected exception");
                }
#endif
            }
        }
    }
}
