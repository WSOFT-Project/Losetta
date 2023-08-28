# Losetta

|[English](README.md)|Japanese|
|-|-|

![AliceScript](https://wsoft.ws/products/AliceScript.svg)

![Build state](https://github.com/WSOFT-Project/Losetta/actions/workflows/codeql.yml/badge.svg)
[![LICENCE](https://img.shields.io/github/license/WSOFT-Project/Losetta)](LICENCE.md)
![Commit Activity](https://img.shields.io/github/commit-activity/y/WSOFT-Project/Losetta)
![Nuget](https://img.shields.io/nuget/dt/Losetta)

AliceScriptはC#や他のCLR言語から呼び出し可能かつカスタマイズ可能なスクリプト言語です。
Losettaは、AliceScriptの公式の言語処理系であり、AliceScriptやAliceSisterと互換性があります。

LosettaはMITライセンスで公開されているため、プロプライエタリとフリー・オープンソースのアプリケーションで使用できます。Losettaは[CSCS](https://github.com/vassilych/cscs)をベースに開発しています。

詳しくは、[LICENCE](/LICENSE.txt)をご覧ください。

## 使用法
### 初期化
NuGetから、Losettaと、Losetta.Runtimeパッケージの両方をプロジェクトにインストールします。

AliceScriptを読み込むアプリケーションは、AliceScriptを実行する前に、必ずAPIを初期化する必要があります。
Alice.Runtimeで使用できるすべてのAPIを初期化するには、次のように記述します。

```cs
AliceScript.Runtime.Init();
```

アプリケーションによっては、AliceScriptの基本的な関数(`if`や`print`)のみを使用させたい場合もあります。基本的なAPIのみを初期化するには、次のように記述します。

```cs
AliceScript.Runtime.InitBasicAPI();
```

名前空間ごとに初期化することもできます。
たとえば、基本的なAPIを初期化した後ファイル処理のAPIのみを初期化したい場合、次のように記述します。

```cs
AliceScript.NameSpaces.Alice_IO.Initer();
```

### 実行
スクリプトを実行するには、`AliceScript`名前空間の`Alice`クラスのメソッドを使用します。

文字列形式のスクリプトを実行するには、`Alice.Execute`を呼び出します。次に例を示します。

```cs
var result = Alice.Execute("1+2");
Console.WriteLine(result);//出力:3
```

ファイルからスクリプトを実行するには、`Alice.ExecuteFile`を呼び出します。次に例を示します。

```cs
var result = Alice.ExecuteFile("myScript.txt");
```

`Execute`および`ExecuteFile`メソッドの戻り値は`Variable`クラスで、AliceScriptで何らかの値を表すものです。C#コードで値の結果を使用するにはこれを変換するか、ジェネリックメソッドを使用できます。たとえば、次の例では、AliceScriptで`2+3`を計算し、C#の`int`型に格納しています。

```cs
int result = Alice.Execute<int>("2+3");
```

### カスタム関数の使用
AliceScriptでは、関数のオーバーロードがひとつのみの場合、C#の関数をそのままバインドできます。次の例では、`MyFunctions`というAliceScriptの名前空間と、その中に数値をひとつ入力すると二乗した数を返す`Pow`メソッドを定義しています。

```cs
[AliceNameSpace]
public static class MyFunctions
{
    [AliceFunction]
    public static double Pow(double x)
    {
        return x * x;
    }
}
```

定義した名前空間と関数を実際にAliceScriptから呼び出すためには、バインド登録が必要です。次の例では、先ほど定義した`MyFunctions`を登録します。

```cs
NameSpaceManerger.Add(typeof(MyFunctions));
```

AliceScriptにカスタム関数を定義するもうひとつの方法は、`FunctionBase`クラスを継承したクラスを定義することです。
次の例では、引数を1つもち、呼び出すと引数の1番目を返す`MyFunction`関数を定義する例です。

```cs
 public class MyFunction : FunctionBase
    {
        public MyFunction()
        {
            //関数の名前
            this.Name = "MyFunction";
            //この関数の呼び出しに最低限必要な引数の個数
            this.MinimumArgCounts = 1;
            this.Run += MyFunction_Run;
        }

        private void MyFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            //関数が呼び出されたときに実行されるコード
            e.Return = e.Args[0];
        }
    }
```

定義した関数を実際にAliceScriptから呼び出すためには、登録が必要です。次の例では、先ほど定義した`MyFunction`を登録します。

```cs
FunctionBaseManerger.Add(new MyFunction());
```

使用可能なAPIは、[APIブラウザ](https://docs.wsoft.ws/products/alice/api/)で確認できます。

より詳しい使用方法については、[AliceScriptDocs](https://docs.wsoft.ws/products/alice)を参照してください。