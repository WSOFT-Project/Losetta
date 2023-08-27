# Losetta

|English|[Japanese](README-ja.md)|
|-|-|

![AliceScript](https://wsoft.ws/products/AliceScript.svg)

![Build state](https://github.com/WSOFT-Project/Losetta/actions/workflows/codeql.yml/badge.svg)
[![LICENCE](https://img.shields.io/github/license/WSOFT-Project/Losetta)](LICENCE.md)
![Commit Activity](https://img.shields.io/github/commit-activity/y/WSOFT-Project/Losetta)
![Nuget](https://img.shields.io/nuget/dt/Losetta.CLI)

Losetta is a customizable scripting language that can be used from C# and other CLR languages; Losetta conforms to SAIM, an implementation of AliceScript, and is compatible with AliceScript and AliceSister.

Losetta is released under the MIT License and is free of charge and without warranty. In addition, Losetta is developed based on [CSCS](https://github.com/vassilych/cscs).

For more information, see [LICENCE](/LICENSE.txt).

## How to use
Download and run the appropriate binary for your environment from Release.

When the program is launched for the first time, a .alice directory is created in the directory where the binaries are located, and startup scripts, etc. are installed.

If invoked without any arguments, the interactive execution mode (REPL) is invoked.

If a file name is specified as an argument, the file is read and executed.

For example, to run the script file test.txt, do the following.

```sh
alice test.txt
```

The script file is in the following format You may also use Shibang.

```cs
print("Hello,World!");

print("What your name?");
write("Name>>");

var name = read();

print("Hello,"+name+"!");
```

It can also be described in a more sophisticated way.

```cs
write("What your name?\r\nName>>");
print("Hello,{0}!",read());
```
Ah yeah, if you consider the order in which they are executed, you can write them on a single line.

```cs
print("Hello,{1}!",write("What your name?\r\nName>>"),read());
```

Access to the API is done after declaring it with the using directive.

```cs
using Alice.IO;

// Write "Hello" in test.txt.
file_write_text("test.txt","Hello");
```

It can also be called directly by specifying a namespace.

```cs
Alice.IO.file_write_text("test.txt","Hello");
```

Available APIs can be found in the [API browser](https://docs.wsoft.ws/products/alice/api/).

For detailed usage instructions, please refer to [AliceScriptDocs](https://docs.wsoft.ws/products/alice) (Japanese). For those seeking information on earlier versions, [AliceScriptWiki](https://alice.wsoft.ws/) may be useful.