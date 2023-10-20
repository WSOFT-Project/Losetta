namespace AliceScript
{
    public enum Exceptions
    {
        /// <summary>
        /// 既定のエラーコード
        /// </summary>
        NONE = 0x000,
        /// <summary>
        /// 関数が見つかりません
        /// </summary>
        COULDNT_FIND_FUNCTION = 0x001,
        /// <summary>
        /// 配列が見つかりません
        /// </summary>
        COULDNT_FIND_ARRAY = 0x002,
        /// <summary>
        /// 変数が見つかりません
        /// </summary>
        COULDNT_FIND_VARIABLE = 0x004,
        /// <summary>
        /// アイテムが配列内に見つかりません
        /// </summary>
        COULDNT_FIND_ITEM = 0x024,
        /// <summary>
        /// 指定されたラベルは存在しません
        /// </summary>
        COULDNT_FIND_LABEL = 0x027,
        /// <summary>
        /// 関数内に指定されたラベルは存在しません
        /// </summary>
        COULDNT_FIND_LABEL_IN_FUNCTION = 0x028,
        /// <summary>
        /// 演算子が見つかりません
        /// </summary>
        COULDNT_FIND_OPERATOR = 0x02e,
        /// <summary>
        /// 無効な演算子です
        /// </summary>
        INVALID_OPERAND = 0x003,
        /// <summary>
        /// 指定された名前は予約されています
        /// </summary>
        ITS_RESERVED_NAME = 0x005,
        /// <summary>
        /// 不正な識別名です。
        /// </summary>
        ILLEGAL_IDENTIFIER = 0x006,
        /// <summary>
        /// 引数が不完全です
        /// </summary>
        INCOMPLETE_ARGUMENTS = 0x009,
        /// <summary>
        /// 関数の定義が不完全です
        /// </summary>
        INCOMPLETE_FUNCTION_DEFINITION = 0x00a,
        /// <summary>
        /// そのようなオブジェクトは存在しません
        /// </summary>
        OBJECT_DOESNT_EXIST = 0x00b,
        /// <summary>
        /// 変数または関数が存在しません
        /// </summary>
        PROPERTY_OR_METHOD_NOT_FOUND = 0x00c,
        /// <summary>
        /// 無効な引数です
        /// </summary>
        INVAILD_ARGUMENT = 0x00d,
        /// <summary>
        /// 関数内の引数が不完全です
        /// </summary>
        INVAILD_ARGUMENT_FUNCTION = 0x00e,
        /// <summary>
        /// 配列が必要です
        /// </summary>
        EXPECTED_ARRRAY = 0x00f,
        /// <summary>
        /// 数値は範囲外です
        /// </summary>
        NUMBER_OUT_OF_RANGE = 0x012,
        /// <summary>
        /// 型が一致しません
        /// </summary>
        TYPE_MISMATCH = 0x013,
        /// <summary>
        /// 引数が不足しています
        /// </summary>
        INSUFFICIENT_ARGUMETS = 0x014,
        /// <summary>
        /// パッケージファイルが壊れています
        /// </summary>
        BAD_PACKAGE = 0x015,
        /// <summary>
        /// 継承元クラスが存在しません
        /// </summary>
        NOT_EXISTS_INHERITANCE_BASE = 0x016,
        /// <summary>
        /// 配列に要素がありません
        /// </summary>
        ARRAY_IS_DOESNT_HAVE_TUPLE = 0x017,
        /// <summary>
        /// 変数がNullです
        /// </summary>
        VARIABLE_IS_NULL = 0x018,
        /// <summary>
        /// ユーザー定義の例外です
        /// </summary>
        USER_DEFINED = 0x019,
        /// <summary>
        /// parmsキーワードより後にパラメータを追加することはできません
        /// </summary>
        COULDNT_ADD_PARAMETERS_AFTER_PARMS_KEYWORD = 0x01a,
        /// <summary>
        /// ファイルが見つかりません
        /// </summary>
        FILE_NOT_FOUND = 0x01b,
        /// <summary>
        /// 名前空間が存在しません
        /// </summary>
        NAMESPACE_NOT_FOUND = 0x01c,
        /// <summary>
        /// 名前空間が読み込まれていません
        /// </summary>
        NAMESPACE_NOT_LOADED = 0x01d,
        /// <summary>
        /// 繰り返しが多すぎます
        /// </summary>
        TOO_MANY_REPETITIONS = 0x01e,
        /// <summary>
        /// Tryブロックにハンドラーがありません
        /// </summary>
        TRY_BLOCK_MISSING_HANDLERS = 0x01f,
        /// <summary>
        /// ブロックを実行できませんでした
        /// </summary>
        COULDNT_EXECUTE_BLOCK = 0x020,
        /// <summary>
        /// 括弧が必要です
        /// </summary>
        NEED_BRACKETS = 0x021,
        /// <summary>
        /// 括弧は不要です
        /// </summary>
        UNNEED_TO_BRACKETS = 0x022,
        /// <summary>
        /// その型の変数は使用できません
        /// </summary>
        WRONG_TYPE_VARIABLE = 0x023,
        /// <summary>
        /// その項目をクラス内で定義することはできません
        /// </summary>
        COULDNT_DEFINE_IN_CLASS = 0x025,
        /// <summary>
        /// 無効なトークンです
        /// </summary>
        INVALID_TOKEN = 0x026,
        /// <summary>
        /// 無効な構文です
        /// </summary>
        INVALID_SYNTAX = 0x029,
        /// <summary>
        /// 関数はすでに定義されています
        /// </summary>
        FUNCTION_IS_ALREADY_DEFINED = 0x02a,
        /// <summary>
        /// 有効な数値表現ではありません
        /// </summary>
        INVALID_NUMERIC_REPRESENTATION = 0x02b,
        /// <summary>
        /// このプロパティに代入することはできません
        /// </summary>
        COULDNT_ASSIGN_THIS_PROPERTY = 0x02c,
        /// <summary>
        /// 読み取り専用の値には代入できません
        /// </summary>
        CANT_ASSIGN_TO_READ_ONLY = 0x02f,
        /// <summary>
        /// 値を混合して取得/設定することはできません
        /// </summary>
        CANT_MIX_VALUE_AND_SET_GET = 0x030,
        /// <summary>
        /// インデックスが配列の境界外です
        /// </summary>
        INDEX_OUT_OF_RANGE = 0x031,
        /// <summary>
        /// 次の行を解析できません
        /// </summary>
        COULDNT_PARSE = 0x032,
        /// <summary>
        /// ファイルを読み込めませんでした
        /// </summary>
        COULDNT_READ_FILE = 0x033,
        /// <summary>
        /// 実装されていません
        /// </summary>
        NOT_IMPLEMENTED = 0x034,
        /// <summary>
        /// その変数を変換することができませんでした
        /// </summary>
        COULDNT_CONVERT_VARIABLE = 0x035,
        /// <summary>
        /// 波括弧が不均等です
        /// </summary>
        UNBALANCED_CURLY_BRACES = 0x036,
        /// <summary>
        /// 角括弧が不均等です
        /// </summary>
        UNBALANCED_SQUARE_BLACKETS = 0x037,
        /// <summary>
        /// 括弧が不均等です
        /// </summary>
        UNBALANCED_PARENTHESES = 0x038,
        /// <summary>
        /// クオーテーションが不均等です
        /// </summary>
        UNBALANCED_QUOTES = 0x039,
        /// <summary>
        /// パッケージが存在しません
        /// </summary>
        COULDNT_FIND_PACKAGE = 0x03b,
        /// <summary>
        /// 互換性のないパッケージです
        /// </summary>
        NOT_COMPATIBLE_PACKAGES = 0x03c,
        /// <summary>
        /// 変数はすでに定義されています
        /// </summary>
        VARIABLE_ALREADY_DEFINED = 0x03e,
        /// <summary>
        /// その関数はグローバル関数ではありません
        /// </summary>
        FUNCTION_NOT_GLOBAL = 0x03f,
        /// <summary>
        /// ライブラリで発生した例外です
        /// </summary>
        LIBRARY_EXCEPTION = 0x040,
        /// <summary>
        /// 引数が要求された個数よりも多いです
        /// </summary>
        TOO_MANY_ARGUREMENTS = 0x041,
        /// <summary>
        /// その操作は禁止されています
        /// </summary>
        FORBIDDEN_OPERATION = 0x042,
        /// <summary>
        /// この引数は特定のキーワードとともに渡す必要があります。
        /// </summary>
        ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD = 0x043,
        /// <summary>
        /// この引数に特定のキーワードを使用することはできません。
        /// </summary>
        ARGUMENT_CANT_USE_WITH_KEYWORD = 0x044,
        /// <summary>
        /// この変数で配列添え字演算子を使用することはできません。
        /// </summary>
        VARIABLE_CANT_USE_WITH_ARRAY_SUBSCRIPT = 0x045,
        /// <summary>
        /// 関数のシグネチャが不正か、処理できませんでした。
        /// </summary>
        INVALID_FUNCTION_SIGNATURE = 0x046,
        /// <summary>
        /// 基底クラスを取得できませんでした。
        /// </summary>
        COULDNT_EXTRACT_BASE_CLASSES = 0x047,
        /// <summary>
        /// 名前空間はネストできません。
        /// </summary>
        NAMESPACE_CANT_BE_NESTED = 0x048,
        /// <summary>
        /// クラスが見つかりません。
        /// </summary>
        COULDNT_FIND_CLASS = 0x049,
        /// <summary>
        /// caseまたはdefaultブロックにbreakがありません。
        /// </summary>
        CASE_BLOCK_MISSING_BREAK = 0x04a,
        /// <summary>
        /// 不明なエスケープ文字です
        /// </summary>
        UNKNOWN_ESCAPE_CHAR = 0x04b
    }
}
