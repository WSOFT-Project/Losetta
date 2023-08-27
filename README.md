# Losetta

|English|[Japanese](README-ja.md)|
|-|-|

![AliceScript](https://wsoft.ws/products/AliceScript.svg)

![Build state](https://github.com/WSOFT-Project/Losetta/actions/workflows/codeql.yml/badge.svg)
[![LICENCE](https://img.shields.io/github/license/WSOFT-Project/Losetta)](LICENCE.md)
![Commit Activity](https://img.shields.io/github/commit-activity/y/WSOFT-Project/Losetta)
![Nuget](https://img.shields.io/nuget/dt/Losetta)

Losetta is a customizable scripting language that can be used from C# and other CLR languages; Losetta conforms to SAIM, an implementation of AliceScript, and is compatible with AliceScript and AliceSister.

Losetta is released under the MIT License and is free of charge and without warranty. In addition, Losetta is developed based on [CSCS](https://github.com/vassilych/cscs).

For more information, see [LICENCE](/LICENSE.txt).

## Features
- Syntax is based on C#, JavaScript, and Python.
- Identifiers such as variable and function names are case-insensitive.
- The `try.... .catch when`, `$"{}"`, and other modern syntaxes can be used.
- Variables, functions, etc. must have a scope and be unique within that scope.
- It has a simplified typing mechanism.
- It can use preprocessing directives to customize some of its syntax.
- Can be easily called from C# or other CLR languages.
- Can read and execute files in any character encoding, not just UTF-8.
- Use the rich API in `Losetta.Runtime`.
- Supports Windows, Mac, and Linux. However, not all APIs are available for all platforms.
## Download
The latest binaries are available at [Releases](https://github.com/WSOFT-Project/Losetta/releases) or [WSOFT Download Center](https://download.wsoft.ws/AliceScript/ Losetta).

The native version binaries (`alice-xxx-native`) are compiled to native code. Therefore, it is not possible to load or call libraries from .

The binaries are also available on NuGet.

- [Losetta](https://www.nuget.org/packages/Losetta)
- [Losetta.Runtime](https://www.nuget.org/packages/Losetta.Runtime)
- [Losetta.CLI](https://www.nuget.org/packages/Losetta.CLI)
- [alice-repl (v0.9.21以前)](https://www.nuget.org/packages/alice-repl)

## Documentation
- For information on how to use the software from the command line, please refer to [alice's README](./alice/README.md) for instructions on how to use it from the command line.
- For information on how to use it from the CLR language, see [Losetta's README](./Losetta/README-ja.md) for information on how to use it from the CLR language.
- To learn more about AliceScript syntax and other details, please refer to [AliceScriptDocs](https://docs.wsoft.ws/products/alice/).
- Available APIs can be found at [API Browser](https://docs.wsoft.ws/products/alice/api/).
- Please refer to [Issues](https://github.com/WSOFT-Project/Losetta/issues) for known bugs and planned new features.
- Want information on previous versions of AliceScript? Please refer to [AliceScriptWiki](https://alice.wsoft.ws/).
- Losetta's API is generated from source code comments.