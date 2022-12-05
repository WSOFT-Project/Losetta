# Losetta
Losettaは、オープンソースのSAIM（標準AliceScript実装）の実装です。.NET6.0で開発されているため、WindowsをはじめmacOSやLinuxなどでも使用でき、さらにAliceScriptをあなたの.NETアプリに埋め込むこともできます。

LosettaはMITライセンスで公開されているため、無料勝つ無保証で使用できます。詳細については[LICENCE](/LICENSE.txt)を参照してください。

Losettaは、[CSCS](https://github.com/vassilych/cscs)に触発され開発されました。

## 開発方針
Losettaは以下の方針で開発されています。

- メインライブラリには最低限必要な関数のみを実装し、それ以外の関数はRuntimeに実装する
- なるべくクラスを`public`にし、使用者がソースを変更しなくてもカスタマイズ可能にする
- ソースをすべてオープンソースで公開し、バージョン管理にGitを使用する

## 使用法
LosettaはSAIMに準拠しているため、使用方法については[AliceScriptDocs](https://docs.wsoft.ws/products/alice)が参考になります。以前のバージョンの情報をお求めの方は[AliceScriptWiki](https://alice.wsoft.ws/)が役立つ可能性があります。