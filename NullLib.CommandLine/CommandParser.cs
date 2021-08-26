using System;
using System.Collections.Generic;
using System.Text;

namespace NullLib.CommandLine
{
    /// <summary>
    /// Provide methods for parsing command
    /// </summary>
    public static class CommandParser
    {
        private static char escapeChar = '\\';

        /// <summary>
        /// EscapeChar for string parsing, defualt is '\\'
        /// </summary>
        public static char EscapeChar { get => escapeChar; set => escapeChar = value; }
        /// <summary>
        /// Default parsers for parse commandline string.
        /// </summary>
        public static IArgumentParser[] DefaultParsers { get; private set; } = new IArgumentParser[]
        {
            new FieldArguParser(),
            new ArguParser()
        };

        /// <summary>
        /// Split commandline string to CommandLineSegment[]
        /// </summary>
        /// <param name="str">commandline string</param>
        /// <param name="result">Return result</param>
        /// <returns>Splitting result</returns>
        public static void SplitCommandLine(string str, out CommandSegment[] result)
        {
            List<CommandSegment> rstBulder = new();
            StringBuilder temp = new();
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
                    if (i == escapeChar)
                    {
                        escape = true;
                    }
                    else
                    {
                        if (quote)
                        {
                            if (i == '"')
                            {
                                rstBulder.Add(new CommandSegment(temp.ToString(), true));
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
                                    rstBulder.Add(new CommandSegment(temp.ToString(), false));
                                    temp.Clear();
                                }

                                quote = true;
                            }
                            else if (char.IsWhiteSpace(i))
                            {
                                if (temp.Length > 0)
                                {
                                    rstBulder.Add(new CommandSegment(temp.ToString(), false));
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
                rstBulder.Add(new CommandSegment(temp.ToString(), quote));

            result = rstBulder.ToArray();
        }

        public static void SplitCommandLineFromStartupArgs(out CommandSegment[] result)
        {
            SplitCommandLine(Environment.CommandLine, out var tmprst);
            result = new CommandSegment[tmprst.Length - 1];
            Array.Copy(tmprst, 1, result, 0, result.Length);
        }

        /// <summary>
        /// Seperate command name and command arguments from commandline segements.
        /// </summary>
        /// <param name="segments">Source commandline segments</param>
        /// <param name="cmdname">Commandline name</param>
        /// <param name="arguments">Commandline arguments</param>
        public static void SplitCommandInfo(CommandSegment[] segments, out string cmdname, out CommandSegment[] arguments)
        {
            if (segments.Length > 0)
            {
                cmdname = segments[0].Content;
                arguments = new CommandSegment[segments.Length - 1];
                Array.Copy(segments, 1, arguments, 0, arguments.Length);
            }
            else
            {
                cmdname = "";
                arguments = new CommandSegment[0];
            }
        }

        public static void SplitCommandInfo(IArgument[] cmdline, out string cmdname, out IArgument[] arguments)
        {
            if (cmdline.Length > 0)
            {
                cmdname = cmdline[0].Content;
                arguments = new IArgument[cmdline.Length - 1];
                Array.Copy(cmdline, 1, arguments, 0, arguments.Length);
            }
            else
            {
                cmdname = "";
                arguments = new IArgument[0];
            }
        }

        public static IArgument[] ParseArguments(IList<IArgumentParser> parsers, CommandSegment[] arguments)
        {
            List<IArgument> result = new();
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
        public static IArgument[] ParseArguments(CommandSegment[] arguments)
        {
            return ParseArguments(DefaultParsers, arguments);
        }
    }
    public class ArguParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref CommandSegment[] arguments, out IArgument result)
        {
            result = new Argument(arguments[index++].Content);
            return true;
        }

        //public string FormatArgu(string name, string defaultValue)
        //{
        //    return defaultValue == null ? $"<{name}>" : $"[{name}({defaultValue})]";
        //}
    }
    public class IdentifierArguParser : IArgumentParser
    {
        public bool IsIdentifier(string str)
        {
            if(str.Length < 1)
                return false;

            char curChar = str[0];
            if(char.IsLetter(curChar) || curChar.Equals('_'))
            {
                bool result = true;
                for(int i = 1, end = str.Length; i < end; i++)
                {
                    curChar = str[i];
                    result &= char.IsLetterOrDigit(curChar) || curChar.Equals('_');
                }
                return result;
            }
            return false;
        }
        public bool TryParse(ref int index, ref CommandSegment[] arguments, out IArgument result)
        {
            result = null;
            CommandSegment curSegment = arguments[index];
            if(curSegment.Quoted)
                return false;
            if(!IsIdentifier(curSegment.Content))
                return false;
            result = new Argument(curSegment.Content);
            index++;
            return true;
        }

        //public string FormatArgu(string name, string defaultValue)
        //{
        //    return defaultValue == null ? $"<{name}>" : $"[{name}({defaultValue})]";
        //}
    }
    public class StringArguParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref CommandSegment[] arguments, out IArgument result)
        {
            CommandSegment curSegment = arguments[index];
            if(curSegment.Quoted)
            {
                result = new Argument(curSegment.Content);
                index++;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        //public string FormatArgu(string name, string defaultValue)
        //{
        //    return defaultValue == null ? $"<{name}>" : $"[{name}({defaultValue})]";
        //}
    }
    public class FieldArguParser : IArgumentParser
    {
        private char separator;

        /// <summary>
        /// Field argument seperator, default is ':'
        /// </summary>
        public char Separator { get => separator; set => separator = value; }
        public FieldArguParser()
        {
            Separator = ':';
        }
        public FieldArguParser(char triggerChar)
        {
            Separator = triggerChar;
        }
        public bool TryParse(ref int index, ref CommandSegment[] arguments, out IArgument result)
        {
            result = null;

            if(index < arguments.Length)
            {
                CommandSegment name = arguments[index];
                int eqindex = name.Content.IndexOf(separator);   // the index of symbol 'equals': ':'
                if((!name.Quoted) && eqindex > 0)
                {
                    if(eqindex + 1 < name.Content.Length)
                    {
                        result = new Argument(name.Content.Substring(0, eqindex), name.Content.Substring(eqindex + 1));
                        index++;
                        return true;
                    }
                    else
                    {
                        if(index + 1 >= arguments.Length)
                            return false;

                        index++;
                        CommandSegment content = arguments[index];
                        result = new Argument(name.Content.Substring(0, name.Content.Length - 1), content.Content);
                        index++;
                        return true;
                    }
                }
            }

            return false;
        }

        //public string FormatArgu(string name, string defaultValue)
        //{
        //    return defaultValue == null ? $"<{name}{separator}value>" : $"[{name}{separator}value({defaultValue})]";
        //}
    }
    public class PropertyArguParser : IArgumentParser
    {
        private string prefix;

        public string Prefix { get => prefix; set => prefix = value; }
        public PropertyArguParser()
        {
            this.Prefix = "-";
        }
        public PropertyArguParser(string triggerString)
        {
            this.Prefix = triggerString;
        }
        public bool TryParse(ref int index, ref CommandSegment[] arguments, out IArgument result)
        {
            result = null;

            if(index < arguments.Length)
            {
                CommandSegment name = arguments[index];
                if((!name.Quoted) && name.Content.StartsWith(prefix))
                {
                    if(index + 1 >= arguments.Length)
                        return false;

                    index++;
                    CommandSegment content = arguments[index];
                    result = new Argument(name.Content.Substring(1), content.Content);
                    index++;
                    return true;
                }
            }

            return false;
        }

        //public string FormatArgu(string name, string defaultValue)
        //{
        //    return defaultValue == null ? $"<{prefix}{name} value>" : $"[{prefix}{name} value({defaultValue})]";
        //}
    }
}
