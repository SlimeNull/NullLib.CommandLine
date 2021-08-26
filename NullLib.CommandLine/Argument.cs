using System.Text.RegularExpressions;

namespace NullLib.CommandLine
{
    /// <summary>
    /// Argument of a command
    /// </summary>
    public interface IArgument
    {
        /// <summary>
        /// Name of this Argument, if not specified, then null
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// String content of this Argument, when set, ValueObj also change
        /// </summary>
        string Content { get; set; }
        /// <summary>
        /// Value object of this Argument, for converter to convert
        /// </summary>
        object ValueObj { get; set; }
    }
    /// <summary>
    /// Provide method for parsing commandline segments
    /// </summary>
    public interface IArgumentParser
    {
        /// <summary>
        /// Try to parse an Argument from commandline segments
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="segments">Source segments</param>
        /// <param name="result">Result Argument</param>
        /// <returns>If this parsing was successed</returns>
        bool TryParse(ref int index, ref CommandSegment[] segments, out IArgument result);
        /// <summary>
        /// Format param name and param default value to a standard format of current parser
        /// </summary>
        /// <param name="name">Param name</param>
        /// <param name="defaultValue">Param default value</param>
        ///// <returns>Formated document string</returns>
        //string FormatArgu(string name, string defaultValue);
    }
    /// <summary>
    /// Commandline segment splited by CommandParser
    /// </summary>
    public class CommandSegment
    {
        /// <summary>
        /// If this segment was quoted
        /// </summary>
        public bool Quoted;
        /// <summary>
        /// String content of this segment
        /// </summary>
        public string Content;

        /// <summary>
        /// Initialize an instance
        /// </summary>
        /// <param name="content">String content of segment</param>
        /// <param name="quoted">If the segment was quoted</param>
        public CommandSegment(string content, bool quoted)
        {
            Content = content;
            Quoted = quoted;
        }
    }
    /// <summary>
    /// Commandline argument
    /// </summary>
    public class Argument : IArgument
    {
        private string name;
        private string content;
        private object valueObj;

        /// <summary>
        /// Name of this Argument, if not specified, then null
        /// </summary>
        public string Name { get => name; set => name = value; }
        /// <summary>
        /// StringContent of this Argument
        /// </summary>
        public string Content { get => content; set => valueObj = content = value; }

        /// <summary>
        /// Value object of this Argument, String or String[]
        /// </summary>
        public object ValueObj { get => valueObj; set => valueObj = value; }

        /// <summary>
        /// Initialize an instance, with null Name, null Content
        /// </summary>
        public Argument() { }

        /// <summary>
        /// Initialize an instance with specified content and null Name
        /// </summary>
        /// <param name="content">Argument content</param>
        public Argument(string content)
        {
            Content = content;
        }
        /// <summary>
        /// Initialize an instance with specified name and content
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="content">Argument content</param>
        public Argument(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
