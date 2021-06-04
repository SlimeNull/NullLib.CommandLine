# NullLib.CommandLine

## Index / 索引

1. This document has two version, English and Chinese.

   这个文档有两个版本, 英文和中文.

2. The following content is English version, to read Chinese version, scroll down.

   下面的是英文版本, 如果要阅读中文版本, 请往下翻.

3. To read whole document, open file in source repository which name like "readme.language.md". readme.en-us.md for English

   这不是全部, 阅读完整版文档, 请打开本仓库中名字像 "readme.语言.md" 的文件. 例如, 中文文档是 readme.zh-cn.md
   
4. For more information, go to [Github](https://github.com/SlimeNull/NullLib.CommandLine)

   更多信息, 请转到 [Github](https://github.com/SlimeNull/NullLib.CommandLine)

## EN-US

Easily calling methods defined in C# with a command string.

### Usage 

First of all, the basic type in **NullLib.CommandLine** for calling methods is `CommandObject`, it contains methods' information, such as `MethodInfo`, `ParameterInfo`, and attributes.

Then, a class which contains methods for being called should be defined, in this class, each method which will be called must has a `Command` attribute, and then, we will initialize a `CommandObject` instance with this type. 

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
[Command(typeof(ForeachArguConverter<FloatArguConverter>))]   // each string of array will be converted by FloatConverter
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
[Command]                                // the defualt converter is 'ArgumentConverter', you can ignore these
public bool StringEquals(string txt1, string txt2)   // or specify 'null' to use the last converter (here is ArgumentConverter)
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
```

## ZH-CN

通过命令行字符串来方便快捷的调用 C# 中定义的方法

### 使用方式 

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

为每一个参数指定 `ArgumentConverter` 以传递参数到方法.

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
[Command]                                // 默认的转换器是 'ArguConverter', 在这种情况下你可以忽略它们
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
```