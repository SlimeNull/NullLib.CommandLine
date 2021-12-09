# NullLib.CommandLine Manual

这里包含了 NullLib.CommandLine 命令行的一些逻辑概述与通用规则

### 转义字符 / EscapeChar

NullLib.CommandLine 是支持转义字符的, 默认为 '\`' (与 PowerShell 一致), 当然, 如果要更改转义字符, 你可以使用 `CommandParser` 的静态属性 `EscapeChar`. 

当然, 不建议使用反斜杠作为转义字符, 因为在 Windows 系统中, 反斜杠是路径分隔符, 在使用时可能会导致一些逻辑上的问题.

转义字符的支持可以参考 C# 的转义字符, NullLib.CommandLine 包含了所有 C# 可用的转义字符, 例如换行符 '\n', 水平制表符 '\t', 换页符 '\v' 等.

### 分割指令 / SplitCommand

在执行命令时, 首先会调用 *CommandParser.SplitCommandLine* 来分割一个字符串为一个个的 `CommandSegment`, 大致是按照空格作为分隔符进行分割, 但是支持使用双引号来整合一段内容. 也就是说, 双引号包裹的内容即便包含空格, 也是一个整体.

每一个 `CommandSegment` 都指明了该部分的内容, 以及该内容是否是双引号包裹的. 因而, 在命令行中, 双引号是具有特殊意义, 如果要为内容指定一个双引号字符, 你可以使用转义字符将其转义.

### 参数分析器 / Argument Parser

在将命令行分割为一个个的 `CommandSegment` 后, 程序将继续使用 `IArguParser` 将这些 `CommandSegment` 进行分析, 进而得到所需参数.

当应用程序使用不同的 `IArguParser` 时, 那么所支持的指令参数指定方式也是不一样的

默认使用的 `IArguParser` 有 `FieldArguParser` 和 `ArguParser`

在 NullLib.CommandLine 中, 命令参数分为两种, 一种是 `位置参数`, 一种是 `名称参数`.

位置参数就是根据该参数在命令行中的位置, 自动应用到命令对应方法的对应参数. 而名称参数则是具有指定名称的参数, 无论它的位置在哪里, 都会对应命令对应方法的同名参数上.

同时, 可选参数, 可变长参数在 NullLib.CommandLine 中都是受支持的, 这意味着你可以为方法参数指定默认值, 或者为参数添加 `params` 关键字.

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

### 参数转换 / Argument Converter

在参数分析完毕后, 所有的参数都对应了命令方法的参数位置, 接下来要做的就是按照用户所指定的参数转换器 `IArguConverter` 将这些字符串值的参数转换为与方法所匹配的类型

用户只需要在方法前添加 `Command` 属性, 并指定对应转换器即可. 需要注意的是, 如果所有参数类型都是 `String`, 那么用户不必去手动指定转换器, NullLib.CommandLine 会使用默认的转换器(不进行任何处理, 直接传递原字符串)进行转换.

### 命令执行 / Command Execute

在一切都就绪后, 命令对应方法将被调用, 如果上述任何一个步骤进行失败, 例如参数转换器进行类型转换后, 得出的对象类型与方法参数所需类型不匹配, 那么执行失败.