using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NullLib.ArgsParser;
using NullLib.CommandLine;

namespace TestConsole
{
    class Program
    {
        class MyCommands
        {
            [NCommand(typeof(IntegerConverter), typeof(IntegerConverter))]
            public int Plus(int a, int b)
            {
                return a + b;
            }
        }
        static void Main(string[] args)
        {
            CommandObject<MyCommands> obj = new CommandObject<MyCommands>();
            while (true)
            {
                string[] commandLine = ArgsParser.SplitArgs(Console.ReadLine());
                obj.TryExecuteCommand(commandLine, out object result);
                Console.WriteLine($"Result: {result}");
            }
        }
    }
}
