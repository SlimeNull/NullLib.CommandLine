# NullLib.CommandLine

Easily calling methods defined in C# with a command string.

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
            if (!AppCommandObject.TryExecuteCommand(cmdline, out var result))
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

To pass parameters to method, specify `ArgumentConverter` for each parameter.

Let's add these methods to `AppCommands`

```csharp
[Command(typeof(FloatArguConverter), typeof(FloatArguConverter))]      // the build-in ArgumentConverter in NullLib.CommandLine
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
[Command]                                   // the defualt converter is 'ArgumentConverter', you can ignore these
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
>>> Print "some text\tescaped char is also supported"
some text       escaped char is also supported
>>> StringEquals qwq awa
False
>>> SetBackground White
>>> SetBackground 0
>>> Print "you can convert to a enum type by it's name or integer value"
you can convert to a enum type by it's name or integer value
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

5. ArgumentConverter:

   Implemented `IArgumentConverter` interface, abstract class, should be inherited by custom Converter.

6. ArgumentConverterManager:

   Helps create `ArgumentConverter` rapidly.

7. CommandLineSegment:

   Component of command line string.

8. ArgumentParser:

   Helps parse `CommandLineSegment` to `IArgument`



## CommandLineSegment

CommandLineSegment is a component of command line string.

For example, in `myprogram param1 "param2"`, there is three CommandLineSegment:

1. {Quoted: false, Content: "myprogram"}
2. {Quoted: false, Content: "param1"}
3. {Quoted: true, Content: "\\"param2\\""}

To split a command line string to CommandLineSegment[], use `CommandParser.SplitCommandLine(string str)`



## Argument

1. Argument:

   The basic Argument type, with no name, implemented `IArgument`, when invoking method, will be passed one by one.

2. NamedArgument:

   Argument with name, when invoking method, will be passed according it's name to a correct position.



## ArgumentParser

Here is all build-in `ArgumentParsers`:

1. ArguParser:

   It can parse any segment as an Argument.
   
2. IdentifierArguParser:

   It can parse an identifier segment as an `Argument`. The corresponding regex string is *"{A-Za-z\_}{A-Za-z0-9\_}\*"*

3. StringArguParser:

   It can parse any segments as an `Argument`, but the segment must be `Quoted`.

4. FieldArguParser:

   It can parse a segment like <u>*name=value*</u> or two segments like <u>*name= value*</u> to a `NamedArgument`, also, you can specify the separator, default is '='.

5. PropertyArguParser:

   It can parse two segments like <u>*-name value*</u> to a NamedArgument, and you can also specify the start string of <u>*name*</u>, default is "-".



## ArgumentConverter

Here is all build-in `ArgumentConverter`:

1. ArguConverter:

   The default `ArgumentConverter` which returns the source value without any conversion

2. BoolArguConverter:

   Helps convert to bool, for source value, if it's "true", then return true, if it's "false", then return false, otherwise, convert failed. (Cases is ignored).

3. CharArguConverter:

   Helps convert to char, only when source string has one char, returns the char, otherwise, convert failed.

4.   ByteArguConverter:

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

    Helps convert to Array, only used for variable-length parameter (decorated by `params`), `TConverter` must implement `IArgumentConverter`, each value of source string array will be converted by the specified Converter.

14. CharArrayArguConverter:

    Helps convert to char array, it calls `string.ToCharArray()` to do conversion.
    


## About ArgumentParser

### Custom Parser:

To define custom `ArgumentParser`, your must follow these rules:

1. Implements `IArgumentParser`
2. After parsing, the reference parameter `index` must leave the parts of result(IArgument), for example, at the index 3, in your custom parser, it will return the result (result was parsed successfully), and the result is from two `CommandLineSegment`s, then the index must be 5 (out of 3 and 4).

## About ArgumentConverter

### Custom Converter:

The recommended way to define a custom `ArgumentConverter` is this:

```csharp
class MyConverter : ArgumentConverter<MyType>    // inherit ArgumentConverter<T> but not IArgumentConverter<T>
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

The reason to inherit `ArgumentConverter<T>` but not `IArgumentConverter<T>` is, in `ArgumentConverter<T>`, all overloads calls two methods:

1. T Convert(string argument);
2. bool TryConverter(string argument, out T result);

So, if you inherit `ArgumentConverter<T>`, you just need to override these two methods.

### Tips:

1. DO NOT create a `ArgumentConverter` with new expression, use `ArgumentConverterManager.GetConverter<T>()`.



## FAQ

1. When I calling `CommandObject.ExecuteCommand(IArgumentParser[] parsers, string cmdline)`, but some of parsers don't work:

   ```csharp
   // you must specify parsers in a correct order, for example:
   CommandObject<AppCommands> myCmds = new CommandObject<AppCommands>();
   myCmds.ExecuteCommand(new IArgumentParser[]
   {
       new ArguParser(),                // in this case, FieldArguParser and PropertyArguParser will not work.
       new FieldArguParser(),           // this is because that ArguParser can parse ANY CommandLineSegments
       new PropertyArguParser()         // so you should place ArguParser as the last parser.
   }, Console.ReadLine());
   ```


