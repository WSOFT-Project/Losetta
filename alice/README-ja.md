# Losetta

|[English](README.md)|Japanese|
|-|-|

![AliceScript](https://wsoft.ws/products/AliceScript.svg)

![Build state](https://github.com/WSOFT-Project/Losetta/actions/workflows/codeql.yml/badge.svg)
[![LICENCE](https://img.shields.io/github/license/WSOFT-Project/Losetta)](LICENCE.md)
![Commit Activity](https://img.shields.io/github/commit-activity/y/WSOFT-Project/Losetta)
![Nuget](https://img.shields.io/nuget/dt/Losetta.CLI)

AliceScriptはC#や他のCLR言語から呼び出し可能かつカスタマイズ可能なスクリプト言語です。
Losettaは、AliceScriptの公式の言語処理系であり、AliceScriptやAliceSisterと互換性があります。

LosettaはMITライセンスで公開されているため、プロプライエタリとフリー・オープンソースのアプリケーションで使用できます。Losettaは[CSCS](https://github.com/vassilych/cscs)をベースに開発しています。

詳しくは、[LICENCE](/LICENSE.txt)をご覧ください。

## 使用法
このリポジトリのReleaseから、実行環境に応じたバイナリをダウンロードし、実行してください。

プログラムをはじめて起動すると、バイナリのあるディレクトリに`.alice`ディレクトリが作成され、起動スクリプトなどがコピーされます。

プログラムを引数なしで起動した場合、対話型実行モード(REPL)が起動します。

ファイル名を引数に指定すると、そのファイルを読み込んで実行します。
たとえば、`test.txt`を実行するには、以下のようにします。

```sh
alice test.txt
```

スクリプトファイルは下記のような形式です。
Unix環境ではシバンも使用できます。

```cs
print("Hello,World!");

print("What your name?");
write("Name>>");

var name = read();

print("Hello,"+name+"!");
```

`$`から始まる文字列挿入を使用すれば、より簡潔に記述することもできます。
```cs
write("What your name?\r\nName>>");
print($"Hello,{read()}!");
```

ちなみに上記の例は、次のように一行でも記述できます。

```cs
print($"Hello,{write('What your name?\r\nName>>')+read()}!");
```
Alice.RuntimeにあるAPIは、`using`ディレクティブを使用したあと使用できます。

```cs
using Alice.IO;

// Write "Hello" in test.txt.
file_write_text("test.txt","Hello");
```

`using`を使用せずに、名前空間を指定して直接呼び出すこともできます。

```cs
Alice.IO.file_write_text("test.txt","Hello");
```

使用可能なAPIは、[APIブラウザ](https://docs.wsoft.ws/products/alice/api/)で確認できます。

より詳しい使用方法については、[AliceScriptDocs](https://docs.wsoft.ws/products/alice)を参照してください。