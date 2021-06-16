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
        public static void SplitCommandLine(string str, out CommandLineSegment[] result)
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

            result = rstBulder.ToArray();
        }

        /// <summary>
        /// Seperate command name and command arguments from commandline segements.
        /// </summary>
        /// <param name="segments">Source commandline segments</param>
        /// <param name="cmdname">Commandline name</param>
        /// <param name="arguments">Commandline arguments</param>
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
    public class ArguParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref CommandLineSegment[] arguments, out IArgument result)
        {
            result = new Argument(arguments[index++].Content);
            return true;
        }
        public bool TryFormat(string name, string defaultValue, out string result)
        {
            result = $"<{name}>";
            return true;
        }
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
        public bool TryParse(ref int index, ref CommandLineSegment[] arguments, out IArgument result)
        {
            result = null;
            CommandLineSegment curSegment = arguments[index];
            if(curSegment.Quoted)
                return false;
            if(!IsIdentifier(curSegment.Content))
                return false;
            result = new Argument(curSegment.Content);
            index++;
            return true;
        }
        public bool TryFormat(string name, string defaultValue, out string result)
        {
            result = $"<{name}>";
            return true;
        }
    }
    public class StringArguParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref CommandLineSegment[] arguments, out IArgument result)
        {
            CommandLineSegment curSegment = arguments[index];
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
        public bool TryFormat(string name, string defaultValue, out string result)
        {
            result = $"<{name}>";
            return true;
        }
    }
    public class FieldArguParser : IArgumentParser
    {
        private char triggerChar;

        public char TriggerChar { get => triggerChar; set => triggerChar = value; }
        public FieldArguParser()
        {
            TriggerChar = '=';
        }
        public FieldArguParser(char triggerChar)
        {
            TriggerChar = triggerChar;
        }
        public bool TryParse(ref int index, ref CommandLineSegment[] arguments, out IArgument result)
        {
            result = null;

            if(index < arguments.Length)
            {
                CommandLineSegment name = arguments[index];
                int eqindex = name.Content.IndexOf(triggerChar);   // the index of symbol 'equals': '='
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
                        CommandLineSegment content = arguments[index];
                        result = new Argument(name.Content.Substring(0, name.Content.Length - 1), content.Content);
                        index++;
                        return true;
                    }
                }
            }

            return false;
        }
    }
    public class PropertyArguParser : IArgumentParser
    {
        private string triggerString;

        public string TriggerString { get => triggerString; set => triggerString = value; }
        public PropertyArguParser()
        {
            this.TriggerString = "-";
        }
        public PropertyArguParser(string triggerString)
        {
            this.TriggerString = triggerString;
        }
        public bool TryParse(ref int index, ref CommandLineSegment[] arguments, out IArgument result)
        {
            result = null;

            if(index < arguments.Length)
            {
                CommandLineSegment name = arguments[index];
                if((!name.Quoted) && name.Content.StartsWith(triggerString))
                {
                    if(index + 1 >= arguments.Length)
                        return false;

                    index++;
                    CommandLineSegment content = arguments[index];
                    result = new Argument(name.Content.Substring(1), content.Content);
                    index++;
                    return true;
                }
            }

            return false;
        }
    }
}
