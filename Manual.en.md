# NullLib.CommandLine Manual

Here are some logical overviews and common rules for the NullLib.CommandLine command line

### EscapeChar / 转义字符

NullLib.CommandLine support escape characters, default is '\`'(Same as PowerShell), and of course, if you want to change escape character, you can use the static Property `EscapeChar` of  `CommandParser`.

And, it's not recommend to use '\\' as escape character, because '\' is the path seperator in Windows OS, it may cause some logical mistakes when you are using it.

For more information about support of escape character, see C# escape characters. NullLib.CommandLine support all characters of available escape characters in C#. such as new line '\n', TAB '\t', and '\v'.

### SplitCommand / 分割指令

While executing command, first of all is call *CommandParser.SplitCommandLine* to split a string to serveral `CommandSegment` s, Roughly split by space, but it's supported to use `"` to combine some string including space character. That is to say, content quoted by double quotation marks are a whole, no matter if there is space character inside it.

Each `CommandSegment` show the content of this segment, and whether is's wrapped by double quotation marks. So, in command line, double quotation has special meaning, and if you want to use this character, you can use Escape character to escape it.

### Argument Parser / 参数分析器

After splitting it to serveral `CommandSegment`s, the program will parse these `CommandSegment`s with `IArguParser` to get required arguments.

When application use different `IArguParser`, supported command argument assign way is different, too.

Default using `IArguParser` are `FieldArguParser` and `ArguParser`.

There are two type of Command Argument, `Positional Argument` and `Named Argument`

Positional Argument is according the position of argument in the command line, automatic assign the argument to the corresponding argument of corresponding method. And Named Argument is a named argument, no matter where it is, always assign the same-name argument of corresponding method.

Also, optional parameter, length-variable parameter is supported in NullLib.CommandLine, that means you can specify a default value for some parameters, or use `params` keyword.

1. FieldArguParser
   
   该 Parser 用于分析形如 名:值 的名称参数, 例:
   
   ```c#
   TestCommand(string testArgu);
   ```
   
   ```txt
   TestCommand testArgu:"这里是testArgu参数的值"
   ```
   
   同时, `FieldArguParser` 的分隔符 ':' 也是可以由用户指定的, 只需要使用对应的构造方法即可. 当然, 为了保证命令风格一致, 不建议指定一些奇葩的字符.

2. PropertyArguParser
   
   与 `FieldArguParser` 一样, `PropertyArguParser` 同样用于分析名称参数, 它的形式和 PowerShell cmdlet 的参数很相似:
   
   ```C#
   TestCommand(string testArgu);
   ```
   
   ```txt
   TestCommand -testArgu "这里是testArgu参数的值"
   ```
   
   当然, `PropertyArguParser` 的前缀 '-' 也是可以由用户指定的, 并且你可以指定为任意字符串, 只要能够保证命令风格不会乱即可.

3. ArguParser
   
   `ArguParser` 是默认的, 适用于任何格式的, 用于获取位置参数的分析器.
   
   ```C#
   TestCommand(string testArgu);
   ```
   
   ```txt
   TestCommand "直接为参数指定值即可, 因为这是第一个参数, 将会应用到 testArgu"
   ```
   
   注意, `ArguParser` 会识别任何 `CommandSegment`, 请注意参数分析器的优先级, 如果 `ArguParser` 在其他分析器前执行, 那么其他分析器将不会起作用.

4. IdentifierArguParser
   
   标识符参数分析器用来分析不被双引号包裹, 内容是下划线, 字母, 以及数字构成的 `CommandSegment`, 它的作用与 `ArguParser` 类似, 只不过带有了这样的限制.
   
   对于内容的要求, 它与大多数编程语言的 "标识符" 要求一致, 必须以下划线或字母开头.
   
   建议将其用于枚举类型的参数.

5. StringArguParser
   
   字符串参数分析器与标识符分析器相反, 它只识别被双引号所包裹的 `CommandSegment`, 并且内容可以是任意值, 比起 `ArguParser`, 只不过是添加了双引号的限制罢了

你可以自定义参数分析器, 例如带有数字限制的数字分析器, 或者其他格式的名称参数分析器, 只需要继承 `IArguParser` 接口即可

### Argument Converter / 参数转换

在参数分析完毕后, 所有的参数都对应了命令方法的参数位置, 接下来要做的就是按照用户所指定的参数转换器 `IArguConverter` 将这些字符串值的参数转换为与方法所匹配的类型

用户只需要在方法前添加 `Command` 属性, 并指定对应转换器即可. 需要注意的是, 如果所有参数类型都是 `String`, 那么用户不必去手动指定转换器, NullLib.CommandLine 会使用默认的转换器(不进行任何处理, 直接传递原字符串)进行转换.

### Command Execute / 命令执行

在一切都就绪后, 命令对应方法将被调用, 如果上述任何一个步骤进行失败, 例如参数转换器进行类型转换后, 得出的对象类型与方法参数所需类型不匹配, 那么执行失败.