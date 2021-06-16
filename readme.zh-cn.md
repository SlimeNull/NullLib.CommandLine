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

   用于使用命令行字符串调用方法的类

3. CommandInvoker:

   帮助通过 `IArguments` 调用方法

4. CommandParser:

   帮助分析命令行字符串以调用方法

5. ArgumentConverter:

   继承 `IArgumentConverter` 接口, 抽象类, 应该被自定义转换器继承

6. ArgumentConverterManager:

   帮助快速创建 `ArgumentConverter`

7. CommandLineSegment:

   命令行字符串的构成部分

8. ArgumentParser:

   帮助将 `CommandLineSegment` 分析为 `IArgument`



## CommandLineSegment

CommandLineSegment 是命令行字符串的组成部分

例如, 在 `myprogram param1 "param2"` 中, 有三个 CommandLineSegment:

1. {Quoted: false, Content: "myprogram"}
2. {Quoted: false, Content: "param1"}
3. {Quoted: true, Content: "\\"param2\\""}

将命令行字符串分割为 CommandLineSegment[], 使用 `CommandParser.SplitCommandLine(string str)`



## Argument

命令的参数, 可有名称, 继承 `IArgument`, 当调用方法时, 将会被传递



## ArgumentParser

下面是所有内置的 `ArgumentParsers`

1. ArguParser:

   可将任何部分分析为一个 `Argument`
   
2. IdentifierArguParser:

   可将一个标识符部分分析为一个 `Argument`. 对应的正则表达式是 *"{A-Za-z\_}{A-Za-z0-9\_}\*"*

3. StringArguParser:

   可以将任何 Quoted(被双引号包围的) 部分分析为 `Argument`

4. FieldArguParser:

   可以将像 <u>*name=value*</u> 的一个部分, 或者像 <u>*name= value*</u> 的两个部分分析为一个 `Argument`, 另外, 你可以指定分隔符, 默认分隔符是 '='

5. PropertyArguParser:

   可以将像 <u>*-name value*</u> 的两个部分分析为 `Argument`, 并且你也可以指定 <u>*name*</u> 的起始字符串, 默认是 "-"



## ArgumentConverter

Here is all build-in `ArgumentConverter`:

1. ArguConverter:

   不会做任何转换而直接返回源值的默认的 `ArgumentConverter`

2. BoolArguConverter:

   帮助转换到布尔值, 使用 bool.Parse 和 bool.TryParse

3. CharArguConverter:

   帮助转换到字符, 仅当字符串有唯一一个字符时, 返回这个字符, 否则转换失败

4.   ByteArguConverter:

5. ShortArguConverter:

6. IntArguConverter:

7. LongArguConverter:

8. FloatArguConverter:

9. DoubleArguConverter:

10. BigIntArguConverter:

11. DecimalArguConverter:

    上面提到的转换器均返回对应的数字类型, 并且它们都通过调用对应类型的 `Parse` 和 `TryParse` 方法实现转换

12. EnumArguConverter&lt;T&gt;:

    帮助转换到枚举类型, T 应该被指定为一个枚举类型. 它可以从枚举值的名称或数字值来转换

13. ForeachArguConverter&lt;TConverter&gt;:

    帮助转换到一个数组, 仅用于可变参数(使用 `params` 修饰), `TConverter` 必须实现 `IArgumentConverter` 接口, 每个值都将被指定的转换器转换, 最终得到一个对应类型的数组.

14. CharArrayArguConverter:

    帮助转换到字符数组, 它调用 `string.ToCharArray()` 来进行转换.


## About ArgumentParser

### Custom Parser:

定义自定义的 `ArgumentParser`, 你需要遵守下面的规则:

1. 实现 `IArgumentParser` 接口
2. 在分析完毕后, 引用参数 `index` 必须离开参数结果(IArgument)的区域, 例如, 在索引 3, 在你的自定义分析器中, 他将返回结果 (结果被成功分析), 并且分析结果来自于两个 `CommandLineSegment`, 所以 `index` 必须是 5 (在 3 和 4 之外).

## About ArgumentConverter

### 自定义 Converter:

定义自定义 `ArgumentConverter` 的推荐方式是这样:

```csharp
class MyConverter : ArgumentConverter<MyType>    // 继承 ArgumentConverter<T> 而不是 IArgumentConverter<T>
{
    public override MyType Convert(string argument)
    {
        // 你的代码
    }
    public override bool TryConvert(string argument, out MyType result)
    {
        // 你的代码
    }
}
```

继承 `ArgumentConverter<T>` 而不是 `IArgumentConverter<T>` 的原因是, 在 `ArgumentConverter<T>` 中, 所有方法重载均调用这两个方法:

1. T Convert(string argument);
2. bool TryConverter(string argument, out T result);

所以如果继承 `ArgumentConverter<T>`, 你只需要重写这两个方法.

### Tips:

1. 不要使用 new 表达式来创建一个 `ArgumentConverter` 实例, 请使用 `ArgumentConverterManager.GetConverter<T>()`.



## FAQ

1. 当我调用 `CommandObject.ExecuteCommand(IArgumentParser[] parsers, string cmdline)` 时, 某些分析器不能正常使用:

   ```csharp
   // 你必须以正确的顺序指定分析器, 否则就会这样:
   CommandObject<AppCommands> myCmds = new CommandObject<AppCommands>();
   myCmds.ExecuteCommand(new IArgumentParser[]
   {
       new ArguParser(),                // 在这种情况下, FieldArguParser 和 PropertyArguParser 将不起作用.
       new FieldArguParser(),           // 这是因为 ArguParser 可以分析任何 CommandLineSegments, 所以你应该
       new PropertyArguParser()         // 将 ArguParser 作为最后一个分析器, 这样分析的时候就会先使用前两个
   }, Console.ReadLine());
   ```