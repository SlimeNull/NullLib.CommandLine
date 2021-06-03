# NullLib.CommandLine

通过命令行字符串来方便快捷的调用 C# 中定义的方法

## 使用方式 

首先, 在 **NullLib.CommandLine** 中用于调用方法的最基本类型是 `CommandObject`, 它包含了方法的各种信息, 例如 `MethodInfo`, `ParameterInfo`, 以及属性.

然后, 你需要定义一个包含要调用方法的类, 在这个类中, 每一个将被调用的方法都应该有一个 `Command` 属性, 之后我们将用这个类型实例化一个 `CommandObject` 实例.

一个示例类型:

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

实例化一个 `CommandObject` 对象, 然后循环执行指令.

```csharp
using System;
using NullLib.CommandLine;

class Program
{
    static CommandObject<AppCommands> AppCommandObject = new CommandObject<AppCommands>();   // 实例化一个 CommandObject 对象
    static void Main(string[] args)
    {
        Console.WriteLine("Now input commands.");
        while (true)
        {
            Console.Write(">>> ");          // 提示符
            string cmdline = Console.ReadLine();
            if (!AppCommandObject.TryExecuteCommand(cmdline, out var result))
            {
                if (result != null)             // 如果一个方法没有返回值, 则结果是 null.
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

运行程序, 并输入指令:

```txt
Now input commands.
>>> HelloWorld
Hello world!
```

为每一个参数指定 `ArgumentConverter` 来以传递参数到方法.

那么, 我们再试试将这些方法添加到 `AppCommands` 中

```csharp
[Command(typeof(FloatArguConverter), typeof(FloatArguConverter))]      // NullLib.CommandLine 中的内置 ArgumentConverter
public float Plus(float a, float b)
{
    return a + b;
}
[Command(typeof(FloatArguConverter))]        // 如果跟着的转换器与上一个是一样的, 那么你可以忽略它们
public float Mul(float a, float b)
{
    return a * b;
}
[Command(typeof(DoubleArguConverter))]
public double Log(double n, double newBase = Math.E)    // 你也可以使用可选参数
{
    return Math.Log(n, newBase);
}
[Command(typeof(ForeachArguConverter<FloatArguConverter>))]   // 数组中的每一个字符串都将被 FloatArguConverter 转换
public float Sum(params float[] nums)                 // 可变参数的方法也是受支持的
{
    float result = 0;
    foreach (var i in nums)
        result += i;
    return result;
}
[Command(typeof(ArgutConverter))]        // 如果不需要做任何转换, 则可以指定一个 'ArguConverter'
public void Print(string txt)
{
    Console.WriteLine(txt);
}
[Command]                                   // 默认的转换器是 'ArguConverter', 在这种情况下你可以忽略它们
public bool StringEquals(string txt1, string txt2)   // 或者指定 'null' 来使用上一个转换器(在这里指 ArguConverter)
{
    return txt1.Equals(txt2);
}
[Command(typeof(EnumArguConverter<ConsoleColor>))]   // EnumConverter 可以用来将字符串自动转换为枚举类型
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
>>> Print "一些文本\t转义字符也是受支持的"
一些文本	转义字符也是受支持的
>>> StringEquals qwq awa
False
>>> SetBackground White
>>> SetBackground 0
>>> Print "你可以通过一个枚举类型的名字或整数值来进行转换"
你可以通过一个枚举类型的名字或整数值来进行转换
```



## Types

1. CommandAttribute:

   可以通过命令行字符串执行的方法必须有一个 `CommandAttribute` 属性

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