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
    partial class Program
    {
        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();       // 创建一个命令行对象
            IArgumentParser[] parsers = new IArgumentParser[]
            {
                new PropertyArguParser("-"),
                new FieldArguParser(':'),
                new ArguParser(),
            };

            Console.WriteLine("Easy command. Copyright 2021 Null.\n");
            while (true)
            {
                string cmdline = MyCommands.NextCommandString();
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
