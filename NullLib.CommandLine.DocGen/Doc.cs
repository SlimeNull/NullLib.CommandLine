using NullLib.ConsoleEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullLib.CommandLine
{
    public partial class DocGen
    {
        public abstract class Doc
        {
            public class DocFormatOption
            {
                public int BoundaryWidth;
                public bool IsSummary;
            }

            public static IEnumerable<string> ClipString(string src, int width)
            {
                if (width <= 0)
                {
                    yield return src;
                    yield break;
                }

                StringBuilder sb = new StringBuilder();
                int curLineLen = 0, curLen;
                foreach (char i in src)
                {
                    curLen = ConsoleText.CalcCharLength(i);

                    if (curLineLen + curLen > width)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                        sb.Append(i);
                        curLineLen = curLen;
                    }
                    else
                    {
                        sb.Append(i);
                        curLineLen += curLen;
                    }
                }

                if (sb.Length > 0)
                    yield return sb.ToString();
            }
            public static IEnumerable<string> ClipJoinString(IEnumerable<string> src, string separator, int width)
            {
                if (width <= 0)
                {
                    foreach (string str in src)
                        yield return str;
                    yield break;
                }

                IEnumerator<string> enumerator = src.GetEnumerator();
                StringBuilder sb = new StringBuilder();
                int separatorLen = ConsoleText.CalcStringLength(separator);
                int curLineLen = 0, curLen;

                if (!enumerator.MoveNext())
                    yield break;

                sb.Append(enumerator.Current);

                foreach (string i in src)
                {
                    curLen = ConsoleText.CalcStringLength(i) + separatorLen;

                    if (curLineLen + curLen > width)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                        sb.Append(i);
                        curLineLen = curLen;
                    }
                    else
                    {
                        sb.Append(i);
                        curLineLen += curLen;
                    }
                }

                if (sb.Length > 0)
                    yield return sb.ToString();
            }

            public static string PadStringRight(string src, int width)
            {
                return PadStringRight(src, ConsoleText.CalcStringLength(src), width);
            }
            public static string PadStringLeft(string src, int width)
            {
                return PadStringLeft(src, ConsoleText.CalcStringLength(src), width);
            }
            public static string PadStringRight(string src, int strLen, int width)
            {
                width -= strLen - src.Length;
                return src.PadRight(width);
            }
            public static string PadStringLeft(string src, int strLen, int width)
            {
                width -= strLen - src.Length;
                return src.PadLeft(width);
            }

            public override string ToString()
            {
                return ToString(new DocFormatOption());
            }

            public virtual string ToString(DocFormatOption formatOption)
            {
                return string.Join("\n", GetDocStrings(formatOption));
            }

            public virtual IEnumerable<string> GetDocStrings(DocFormatOption formatOption)
            {
                return ClipString(base.ToString(), formatOption.BoundaryWidth);
            }
        }


        public class CommandDoc : Doc
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public CommandArguDoc[] CommandArguDocs { get; set; }


            public class CommandArguDoc : Doc
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public Type ArguType { get; set; }

                public class CommandArguDocFormatOption : DocFormatOption
                {
                    public int DefinitionWidth;
                }

                public override string ToString(DocFormatOption formatOption)
                {

                }

                public string ToString(CommandArguDocFormatOption formatOption)
                {

                }

                public override IEnumerable<string> GetDocStrings(DocFormatOption formatOption)
                {
                    return base.GetDocStrings(formatOption);
                }

                public IEnumerable<string> GetDocStrings(CommandArguDocFormatOption formatOption)
                {
                    if (formatOption.IsSummary)
                    {

                    }
                    else
                    {
                        foreach (string line in ClipString($"{Name.PadRight(formatOption.DefinitionWidth)}  {Description}", formatOption.BoundaryWidth))
                            yield return line;
                    }
                }
            }
        }

        public class CommandHomeDoc : Doc
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public CommandHomeDoc[] CommandHostDocs { get; set; }

            public CommandDoc[] commandDocs { get; set; }

            public CommandHomeDoc(CommandObject cmdobj)
            {

            }

            public override string ToString(DocFormatOption formatOption)
            {
                if (formatOption is CommandHomeDocFormatOption _formatOption)
                    return ToString(_formatOption);

                return ToString(new CommandHomeDocFormatOption()
                {
                    BoundaryWidth = formatOption.BoundaryWidth,
                    IsSummary = formatOption.IsSummary
                });
            }

            public string ToString(CommandHomeDocFormatOption formatOption)
            {
            }

            public override IEnumerable<string> GetDocStrings(DocFormatOption formatOption)
            {
                if (formatOption is CommandHomeDocFormatOption _formatOption)
                    return GetDocStrings(_formatOption);

                return GetDocStrings(new CommandHomeDocFormatOption()
                {
                    IsSummary = formatOption.IsSummary,
                    BoundaryWidth = formatOption.BoundaryWidth,
                });
            }

            public IEnumerable<string> GetDocStrings(CommandHomeDocFormatOption formatOption)
            {
                string indent = formatOption.GetIndentString();
                int indentLen = ConsoleText.CalcStringLength(indent);

                if (formatOption.IsSummary)
                {

                }
                else
                {
                    // description
                    yield return $"{Name}:";
                    foreach (string line in ClipString(Description, formatOption.BoundaryWidth - indentLen))
                    {
                        yield return $"{indent}{line}";
                    }
                    yield return Environment.NewLine;

                    // commands
                    DocFormatOption cmdHomeFormatOption = new DocFormatOption()
                    {
                        IsSummary = true,
                        BoundaryWidth = formatOption.Indent - indentLen
                    };
                    foreach (CommandHomeDoc cmdhomedoc in CommandHostDocs)
                    {
                        foreach (string line in cmdhomedoc.GetDocStrings(cmdHomeFormatOption))
                        {

                        }
                    }
                }
            }


            public class CommandHomeDocFormatOption : DocFormatOption
            {
                public int Indent = 2;
                public string GetIndentString() => new string(' ', Indent);
            }
        }
    }
}