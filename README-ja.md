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

## 特徴
- 文法はC#、JavaScript、Pythonを参考にしています。
- 変数名や関数名などの識別子は、大文字と小文字を区別しません。
- `try...catch when`や`$"{}"`などの最新の構文が使用できます。
- 変数や関数などはスコープを持ち、その範囲内で唯一である必要があります。
- 簡易的な型付けシステムを持ちます。
- 前処理指令を使用することで、一部の文法をカスタマイズできます。
- C#や他のCLR言語から簡単に呼び出すことができます。
- UTF-8だけでなく、あらゆる文字コードのファイルを読み込んで実行できます。
- `Losetta.Runtime`にある豊富なAPIを使用できます。
- Windows、Mac、Linuxをサポートします。ただし、すべてのAPIがすべてのプラットフォームで使用できるとは限りません。
## ダウンロード
最新版のバイナリは[Releases](https://github.com/WSOFT-Project/Losetta/releases)または[WSOFTダウンロードセンター](https://download.wsoft.ws/AliceScript/Losetta)で公開しています。

ネイティブ版のバイナリ(`alice-xxx-native`)は、ネイティブコードにコンパイルされています。そのため、.NETのライブラリを読み込むことも、.NETから呼び出すこともできません。

バイナリはNuGetでも公開していています。

- [Losetta](https://www.nuget.org/packages/Losetta)
- [Losetta.Runtime](https://www.nuget.org/packages/Losetta.Runtime)
- [Losetta.CLI](https://www.nuget.org/packages/Losetta.CLI)
- [alice-repl (v0.9.21以前)](https://www.nuget.org/packages/alice-repl)

## ドキュメント
- コマンドラインから使用する方法については[aliceのREADME](./alice/README-ja.md)を参照してください。
- CLR言語から使用する方法については[LosettaのREADME](./Losetta/README-ja.md)を参照してください。
- AliceScriptの文法などについて詳しく知るには、[AliceScriptDocs](https://docs.wsoft.ws/products/alice/)を参照してください。
- 使用可能なAPIは、[APIブラウザ](https://docs.wsoft.ws/products/alice/api/)で確認できます。
- 既知の不具合や計画されている新機能については、[Issues](https://github.com/WSOFT-Project/Losetta/issues)を参照してください。
- 旧バージョンのAliceScriptに関する情報をお求めですか？[AliceScriptWiki](https://alice.wsoft.ws/)をご参照ください。
- LosettaのAPIは、ソースコードのコメントから生成されます。
