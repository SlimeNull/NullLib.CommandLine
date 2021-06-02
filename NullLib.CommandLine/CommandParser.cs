using System;
using System.Collections.Generic;
using System.Text;

namespace NullLib.CommandLine
{
    public static class CommandParser
    {
        public static IArgumentParser[] DefaultParsers { get; private set; } = new IArgumentParser[]
        {
            new FieldArgumentParser(),
            new StringArgumentParser()
        };

        public static CommandLineSegment[] SplitCommandLine(string str)
        {
            List<CommandLineSegment> rstBulder = new List<CommandLineSegment>();
            StringBuilder temp = new StringBuilder();
            bool escape = false, quote = false;

            foreach (char i in str)
            {
                if (escape)
                {
                    escape = false;
                    temp.Append(i switch
                    {
                        'a' => '\a',
                        'b' => '\b',
                        'f' => '\f',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        _ => i
                    });
                }
                else
                {
                    if (i == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        if (quote)
                        {
                            if (i == '"')
                            {
                                rstBulder.Add(new CommandLineSegment(temp.ToString(), true));
                                temp.Clear();

                                quote = false;
                            }
                            else
                            {
                                temp.Append(i);
                            }
                        }
                        else
                        {
                            if (i == '"')
                            {
                                if (temp.Length > 0)
                                {
                                    rstBulder.Add(new CommandLineSegment(temp.ToString(), false));
                                    temp.Clear();
                                }

                                quote = true;
                            }
                            else if (i == ' ')
                            {
                                if (temp.Length > 0)
                                {
                                    rstBulder.Add(new CommandLineSegment(temp.ToString(), false));
                                    temp.Clear();
                                }
                            }
                            else
                            {
                                temp.Append(i);
                            }
                        }
                    }
                }
            }

            if (temp.Length > 0)
                rstBulder.Add(new CommandLineSegment(temp.ToString(), quote));

            return rstBulder.ToArray();
        }

        public static void SplitCommandInfo(CommandLineSegment[] segments, out string cmdname, out CommandLineSegment[] arguments)
        {
            if (segments.Length > 0)
            {
                cmdname = segments[0].Content;
                arguments = new CommandLineSegment[segments.Length - 1];
                Array.Copy(segments, 1, arguments, 0, arguments.Length);
            }
            else
            {
                cmdname = "";
                arguments = new CommandLineSegment[0];
            }
        }

        public static IArgument[] ParseArguments(IList<IArgumentParser> parsers, CommandLineSegment[] arguments)
        {
            List<IArgument> result = new List<IArgument>();
            for (int i = 0, iend = arguments.Length; i < iend;)
            {
                IArgument argu = null;
                foreach (IArgumentParser parser in parsers)
                    if (parser.TryParse(ref i, ref arguments, out argu))
                        break;

                if (argu == null)
                    throw new ArgumentOutOfRangeException("Not all arguments can be parsed");

                result.Add(argu);
            }

            return result.ToArray();
        }
        public static IArgument[] ParseArguments(CommandLineSegment[] arguments)
        {
            return ParseArguments(DefaultParsers, arguments);
        }
    }
}
