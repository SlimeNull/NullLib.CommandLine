using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NullLib.ArgsParser;
using NullLib.CommandLine;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Binding;

namespace TestConsole
{
    static class StrEx
    {
        public static int CountSubstring(this string self, string substr)
        {
            int
                substrLen = substr.Length;
            int
                startIndex = 0,
                count = 0;

            int tmp;
            while ((tmp = self.IndexOf(substr, startIndex)) >= 0)
            {
                startIndex = tmp + substrLen;
                count++;
            }

            return count;
        }
    }
    partial class Program
    {
        static Program()
        {
            CommandParser.EscapeChar = '`';
            Console.WriteLine("QWQAWAQWQAWAWQWQ".CountSubstring("QWQ"));
        }

        static void Main(string[] args)
        {
            var rootCmd = new RootCommand()
            {
                new Option<string>("--fuck-option"),
            };

            rootCmd.SetHandler<string>(NewMethod);

            while (true)
            {
                CommandParser.SplitCommandLine(Console.ReadLine(), out var fuck);
                rootCmd.Invoke(fuck.Select(v => v.Content).ToArray());
            }


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

            if (args.Length > 0)
            {
                CommandParser.SplitCommandLineFromStartupArgs(out CommandSegment[] rst);
                obj.ExecuteCommand(rst);

            }

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
                    if (result != null)
                        Console.WriteLine(result);
                }
                catch (CommandException)
                {
                    Console.Error.WriteLine("Syntax error: can't execute command.");
                }
                catch (TargetInvocationException)
                {
                    Console.Error.WriteLine("Method error: exception thrown when execute method.");
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Unexpected exception");
                }
#endif
            }
        }

        private static void NewMethod(string fuckOption)
        {
            Console.WriteLine(fuckOption);
        }
    }
}
