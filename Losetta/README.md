# Losetta

|English|[Japanese](README-ja.md)|
|-|-|

![AliceScript](https://wsoft.ws/products/AliceScript.svg)

![Build state](https://github.com/WSOFT-Project/Losetta/actions/workflows/codeql.yml/badge.svg)
[![LICENCE](https://img.shields.io/github/license/WSOFT-Project/Losetta)](LICENCE.md)
![Commit Activity](https://img.shields.io/github/commit-activity/y/WSOFT-Project/Losetta)
![Nuget](https://img.shields.io/nuget/dt/Losetta)

AliceScript is a scripting language callable and customizable from C# and other CLR languages.
Losetta is the official language processor for AliceScript and is compatible with AliceScript and AliceSister.

Losetta is published under the MIT License and can be used in proprietary and free/open source applications. Losetta is based on [CSCS](https://github.com/vassilych/cscs).

See [LICENCE](/LICENSE.txt) for more information.

## How to use
### Initialization
From NuGet, install both Losetta and the Losetta.Runtime package in your project.

Applications that load AliceScript must initialize their APIs before running AliceScript.
To initialize all APIs available in Alice.Runtime, write

```cs
AliceScript.Runtime.Init();
```

Some applications may want to use only the basic AliceScript functions (`if` and `print`). To initialize only the basic API, write as follows

```cs
AliceScript.Runtime.InitBasicAPI();
```

It is also possible to initialize by namespace.
For example, if you want to initialize only the file processing API after initializing the basic API, write

```cs
AliceScript.NameSpaces.Alice_IO.Initer();
```

### Execute
To execute a script, use the methods of the `Alice` class in the `AliceScript` namespace.

To execute a script in string form, call `Alice.Execute`. Here is an example

```cs
var result = Alice.Execute("1+2");
Console.WriteLine(result);//出力:3
```

To execute a script from a file, call `Alice.ExecuteFile`. Here is an example

```cs
var result = Alice.ExecuteFile("myScript.txt");
```

The return value of the `Execute` and `ExecuteFile` methods is a `Variable` class, representing some value in AliceScript; you can convert this to use the value result in C# code or use a generic method. For example, the following example calculates `2+3` in AliceScript and stores it in a C# `int` type.

```cs
int result = Alice.Execute<int>("2+3");
```

### Usage for Custom Functions
In AliceScript, you can bind a C# function as is if it has only one function overload. The following example defines an AliceScript namespace called `MyFunctions` and a `Pow` method that returns a number squared when a single number is entered into it.

```cs
[AliceNameSpace]
public static class MyFunctions
{
    public static double Pow(double x)
    {
        return x * x;
    }
}
```

In order to actually call the defined namespace and functions from AliceScript, a bind registration is required. The following example registers ``MyFunctions`` defined earlier.

```cs
NameSpaceManerger.Add(typeof(MyFunctions));.
```

Another way to define custom functions in AliceScript is to define a class that extends the `FunctionBase` class.
The following example shows how to define a `MyFunction` function that has one argument and returns the first argument when called.

```cs
 public class MyFunction : FunctionBase
    {
        public MyFunction()
        {
            //The name of this function
            this.Name = "MyFunction";
            //Minimum number of arguments required to call this function
            this.MinimumArgCounts = 1;
            this.Run += MyFunction_Run;
        }

        private void MyFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            //Statement to be executed when the function is called
            e.Return = e.Args[0];
        }
    }
```

In order to actually call the defined function from AliceScript, it must be registered. In the following example, register `MyFunction` defined earlier.

```cs
FunctionBaseManerger.Add(new MyFunction());
```

Available APIs can be found in the [API browser](https://docs.wsoft.ws/products/alice/api/).

For detailed usage instructions, please refer to [AliceScriptDocs](https://docs.wsoft.ws/products/alice) (Japanese). For those seeking information on earlier versions, [AliceScriptWiki](https://alice.wsoft.ws/) may be useful.