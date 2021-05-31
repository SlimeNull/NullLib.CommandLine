using System.Text.RegularExpressions;

namespace NullLib.CommandLine
{
    public interface IArgument
    {
        string Content { get; set; }
        object ValueObj { get; set; }
    }
    public interface INamedArgument : IArgument
    {
        string Name { get; set; }
    }
    public interface IArgumentParser
    {
        bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result);
    }
    public struct Argument : IArgument
    {
        public string Content { get; set; }
        public object ValueObj { get; set; }

        public Argument(string content)
        {
            Content = content;
            ValueObj = null;
        }
    }
    public struct NamedArgument : INamedArgument
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public object ValueObj { get; set; }

        public NamedArgument(string name)
        {
            Name = name;
            Content = string.Empty;
            ValueObj = null;
        }
        public NamedArgument(string name, string content)
        {
            Name = name;
            Content = content;
            ValueObj = null;
        }
    }
    public struct ArgumentSegment
    {
        public bool Quoted;
        public string Content;

        public ArgumentSegment(string content, bool quoted)
        {
            Content = content;
            Quoted = quoted;
        }
    }
    public class StringArgumentParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result)
        {
            result = new Argument(arguments[index++].Content);
            return true;
        }
    }
    public class IdentifierArgumentParser : IArgumentParser
    {
        public bool IsIdentifier(string str)
        {
            if (str.Length < 1)
                return false;

            char curChar = str[0];
            if (char.IsLetter(curChar) || curChar.Equals('_'))
            {
                bool result = true;
                for (int i = 1, end = str.Length; i < end; i++)
                {
                    curChar = str[i];
                    result &= char.IsLetterOrDigit(curChar) || curChar.Equals('_');
                }
                return result;
            }
            return false;
        }
        public bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result)
        {
            result = null;
            ArgumentSegment curSegment = arguments[index];
            if (curSegment.Quoted)
                return false;
            if (!IsIdentifier(curSegment.Content))
                return false;
            result = new Argument(curSegment.Content);
            index++;
            return true;
        }
    }
    public class AbsouteStringArgumentParser : IArgumentParser
    {
        public bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result)
        {
            ArgumentSegment curSegment = arguments[index];
            if (curSegment.Quoted)
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
    }
    public class FieldArgumentParser : IArgumentParser
    {
        private char triggerChar;

        public char TriggerChar { get => triggerChar; set => triggerChar = value; }
        public FieldArgumentParser()
        {
            TriggerChar = '=';
        }
        public FieldArgumentParser(char triggerChar)
        {
            TriggerChar = triggerChar;
        }
        public bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result)
        {
            result = null;

            if (index < arguments.Length)
            {
                ArgumentSegment name = arguments[index];
                int eqindex = name.Content.IndexOf(triggerChar);   // the index of symbol 'equals': '='
                if ((!name.Quoted) && eqindex > 0)
                {
                    if (eqindex + 1 < name.Content.Length)
                    {
                        result = new NamedArgument(name.Content.Substring(0, eqindex), name.Content.Substring(eqindex + 1));
                        index++;
                        return true;
                    }
                    else
                    {
                        if (index + 1 >= arguments.Length)
                            return false;

                        index++;
                        ArgumentSegment content = arguments[index];
                        result = new NamedArgument(name.Content.Substring(0, name.Content.Length - 1), content.Content);
                        index++;
                        return true;
                    }
                }
            }

            return false;
        }
    }
    public class PropertyArgumentParser : IArgumentParser
    {
        private string triggerString;

        public string TriggerString { get => triggerString; set => triggerString = value; }
        public PropertyArgumentParser()
        {
            this.TriggerString = "-";
        }
        public PropertyArgumentParser(string triggerString)
        {
            this.TriggerString = triggerString;
        }
        public bool TryParse(ref int index, ref ArgumentSegment[] arguments, out IArgument result)
        {
            result = null;

            if (index < arguments.Length)
            {
                ArgumentSegment name = arguments[index];
                if ((!name.Quoted) && name.Content.StartsWith(triggerString))
                {
                    if (index + 1 >= arguments.Length)
                        return false;

                    index++;
                    ArgumentSegment content = arguments[index];
                    result = new NamedArgument(name.Content.Substring(1), content.Content);
                    index++;
                    return true;
                }
            }

            return false;
        }
    }
}
