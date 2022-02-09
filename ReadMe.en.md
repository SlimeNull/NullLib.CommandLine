# NullLib.CommandLine

Easily calling methods defined in C# with a command string.

More information about using this Library, see [Manual](./Manual.en.md)

## Usage

First of all, the basic type in **NullLib.CommandLine** for calling methods is `CommandObject`, it contains methods' information, such as `MethodInfo`, `ParameterInfo`, and attributes.

Then, a class which contains methods for being called should be defined, in this class, each method which will be called must has a Command attribute, and then, we will initialize a `CommandObject` instance with this type. 

An example class:

```csharp
public class AppCommands
{
    [Command]
    public void HelloWorld()
    {
        Console.WriteLine("Hello world!");
    }
}
```

Initialize a `CommandObject` instance, and loop execute command.

```csharp
using System;
using NullLib.CommandLine;

class Program
{
    static CommandObject<AppCommands> AppCommandObject = new CommandObject<AppCommands>();   // new CommandObject instance
    static void Main(string[] args)
    {
        Console.WriteLine("Now input commands.");
        while (true)
        {
            Console.Write(">>> ");          // prompt
            string cmdline = Console.ReadLine();
            if (AppCommandObject.TryExecuteCommand(cmdline, out var result))
            {
                if (result != null)             // if a method has no return value, then result is null.
                    Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("Command execute failed.");
            }
        }
    }
}
```

Run application, and input command:

```txt
Now input commands.
>>> HelloWorld
Hello world!
```

To pass parameters to method, specify `ArguConverter` for each parameter.

Let's add these methods to `AppCommands`

```csharp
[Command(typeof(FloatArguConverter), typeof(FloatArguConverter))]      // the build-in ArguConverter in NullLib.CommandLine
public float Plus(float a, float b)
{
    return a + b;
}
[Command(typeof(FloatArguConverter))]        // if the following converters is same as the last one, you can ignore these
public float Mul(float a, float b)
{
    return a * b;
}
[Command(typeof(DoubleArguConverter))]
public double Log(double n, double newBase = Math.E)    // you can also use optional parameter
{
    return Math.Log(n, newBase);
}
[Command(typeof(ForeachArguConverter<FloatArguConverter>))]   // each string of array will be converted by FloatArguConverter
public float Sum(params float[] nums)                 // variable length parameter method is supported
{
    float result = 0;
    foreach (var i in nums)
        result += i;
    return result;
}
[Command(typeof(ArgutConverter))]        // if don't need to do any convertion, specify an 'ArguConverter'
public void Print(string txt)
{
    Console.WriteLine(txt);
}
[Command]                                   // the defualt converter is 'ArguConverter', you can ignore these
public bool StringEquals(string txt1, string txt2)   // or specify 'null' to use the last converter (here is ArguConverter)
{
    return txt1.Equals(txt2);
}
[Command(typeof(EnumArguConverter<ConsoleColor>))]   // EnumConverter helps convert string to Enum type automatically.
public void SetBackground(ConsoleColor color)
{
    Console.BackgroundColor = color;
}
```

Run and input:

```txt
Now input commands.
>>> Plus 1 1
2
>>> Mul 2 4
8
>>> Log 8 2
3
>>> Log 8
2.07944154167984
>>> Sum 1 2 3 4
10
>>> Print "some text`tescaped char is also supported"
some text       escaped char is also supported
>>> StringEquals qwq awa
False
>>> SetBackground White
>>> SetBackground 0
>>> Print "you can convert to a enum type by it's name or integer value"
you can convert to a enum type by it's name or integer value
```

Nested Commands:

```csharp
public class MyCommand
{
    [CommandHost]   // Add CommandHost Attribute for CommandObject Property member to use 'Nested Commands'
    public CommandObject<MathCommand> Math { get; } = new();   // 实例化成员

    [Command]
    public string Hello() => "Hello, world.";

    public class MathCommand
    {
        // implement Plus command in nested commands
        [Command(typeof(DoubleArguConverter))]
        public double Plus(double a, double b) => a + b;
    }
}

/*
    In codes above, we added a 'Nested Commands' command, and these commands will be supported:
    Math Plus a:Double b:Double
    Hello
*/
```

Intergrated commands overview text generation:

```csharp
CommandObject<AppCommand> AppCommandObject = new();
Console.WriteLine(AppCommandObject.GenCommandOverviewText());    // Show all available commands
Console.WriteLine(AppCommandObject.GenCommandDetailsText("Help", StringComparison.OrdinalIgnoreCase));  // See document of Help command
```

Get the CommandObject which is using the current custom Command class instance

```csharp
using System;
using NullLib.CommandLine;

class MyCommands : CommandHome  // Inherit from CommandHome
{
    [Command]
    public void Hello()
    {
        Console.WriteLine(CommandObject.GenCommandOverviewText());  // Use the CommandObject property of CommandHome

        // If there is not CommandObject is using the current instance, it will returns null
    }
}
```

Change the escape char for your program

```csharp
CommandParser.EscapeChar = '^';  // Change escape char to '^' (default is '`')
```

## Types

1. CommandAttribute:
   
   Method that can be execute by command string must has a `CommandAttribute`

2. CommandObject:
   
   Class for invoking methods by command string.

3. CommandInvoker:
   
   Helps invoking methods by `IArguments`.

4. CommandParser:
   
   Helps to parse a command string for method calling.

5. ArguConverter:
   
   Implemented `IArguConverter` interface, abstract class, should be inherited by custom Converter.

6. ArguConverterManager:
   
   Helps create `ArguConverter` rapidly.

7. CommandLineSegment:
   
   Component of command line string.

8. ArguParser:
   
   Helps parse `CommandLineSegment` to `IArgument`

## CommandLineSegment

CommandLineSegment is a component of command line string.

For example, in `myprogram param1 "param2"`, there is three CommandLineSegment:

1. {Quoted: false, Content: "myprogram"}
2. {Quoted: false, Content: "param1"}
3. {Quoted: true, Content: "\\"param2\\""}

To split a command line string to CommandLineSegment[], use `CommandParser.SplitCommandLine(string str)`

## Argument

Argument of command, can have a name, implemented `IArgument`, when invoking method, will be passed.

## ArguParser

Here is all build-in `ArguParsers`:

1. ArguParser:
   
   It can parse any segment as an Argument.

2. IdentifierArguParser:
   
   It can parse an identifier segment as an `Argument`. The corresponding regex string is *"{A-Za-z\_}{A-Za-z0-9\_}\*"*

3. StringArguParser:
   
   It can parse any segments as an `Argument`, but the segment must be `Quoted`.

4. FieldArguParser:
   
   It can parse a segment like <u>*name=value*</u> or two segments like <u>*name= value*</u> to a `Argument`, also, you can specify the separator, default is '='.

5. PropertyArguParser:
   
   It can parse two segments like <u>*-name value*</u> to a `Argument`, and you can also specify the start string of <u>*name*</u>, default is "-".

## ArguConverter

Here is all build-in `ArguConverter`:

1. ArguConverter:
   
   The default `ArguConverter` which returns the source value without any conversion

2. BoolArguConverter:
   
   Helps convert to bool, for source value, if it's "true", then return true, if it's "false", then return false, otherwise, convert failed. (Cases is ignored).

3. CharArguConverter:
   
   Helps convert to char, only when source string has one char, returns the char, otherwise, convert failed.

4. ByteArguConverter:

5. ShortArguConverter:

6. IntArguConverter:

7. LongArguConverter:

8. FloatArguConverter:

9. DoubleArguConverter:

10. BigIntArguConverter:

11. DecimalArguConverter:
    
    The converters mentioned above all returns the corresponding number type, and all calls `Parse` and `TryParse` method of corresponding number type.

12. EnumArguConverter&lt;T&gt;:
    
    Helps convert to `Enum` type. T should specified a `Enum` type. It can convert from a name or number value of `Enum` type.

13. ForeachArguConverter&lt;TConverter&gt;:
    
    Helps convert to Array, only used for variable-length parameter (decorated by `params`), `TConverter` must implement `IArguConverter`, each value of source string array will be converted by the specified Converter.

14. CharArrayArguConverter:
    
    Helps convert to char array, it calls `string.ToCharArray()` to do conversion.

## About ArguParser

### Custom Parser:

To define custom `ArguParser`, your must follow these rules:

1. Implements `IArguParser`
2. After parsing, the reference parameter `index` must leave the parts of result(IArgument), for example, at the index 3, in your custom parser, it will return the result (result was parsed successfully), and the result is from two `CommandLineSegment`s, then the index must be 5 (out of 3 and 4).

## About ArguConverter

### Custom Converter:

The recommended way to define a custom `ArguConverter` is this:

```csharp
class MyConverter : ArguConverterBase<MyType>    // inherit ArguConverter<T> but not IArguConverter<T>
{
    public override MyType Convert(string argument)
    {
        // your code here
    }
    public override bool TryConvert(string argument, out MyType result)
    {
        // your code here
    }
}
```

The reason to inherit `ArguConverterBase<T>` but not `IArguConverter<T>` is, in `ArguConverterBase<T>`, all overloads calls two methods:

1. T Convert(string argument);
2. bool TryConverter(string argument, out T result);

So, if you inherit `ArguConverterBase<T>`, you just need to override these two methods.

### Tips:

1. Do NOT create a `ArguConverter` with new expression, use `ArguConverterManager.GetConverter<T>()`.

## FAQ

1. When I calling `CommandObject.ExecuteCommand(IArguParser[] parsers, string cmdline)`, but some of parsers don't work:
   
   ```csharp
   // you must specify parsers in a correct order, or it will like this:
   CommandObject<AppCommands> myCmds = new CommandObject<AppCommands>();
   myCmds.ExecuteCommand(new IArguParser[]
   {
       new ArguParser(),                // in this case, FieldArguParser and PropertyArguParser will not work.
       new FieldArguParser(),           // this is because that ArguParser can parse ANY CommandLineSegments
       new PropertyArguParser()         // so you should place ArguParser as the last parser.
   }, Console.ReadLine());
   ```