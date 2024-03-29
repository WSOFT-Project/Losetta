﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Xml;

namespace AliceScript.Extra
{
    //The code from WSOFTConfigManager and WebSailing.
    //Allrights reserved WSOFT 2020.

    /// <summary>
    /// XMLファイルを管理するクラスです
    /// </summary>
    internal sealed class XMLConfig
    {


        private string m_default = "<?xml version=\"1.0\" encoding=\"utf-8\"?><package></package>";
        /// <summary>
        /// 読み込み時のファイルパスです。Save()のパスを省略した時の保存先でもあります
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 指定されたファイルを使って読み込みます
        /// </summary>
        /// <param name="path">XMLファイルのパス</param>
        public XMLConfig(string path)
        {
            try { xmlText = File.ReadAllText(FilePath); }
            catch { xmlText = m_default; }

        }
        /// <summary>
        /// 規定のXMLを使って開始します
        /// </summary>
        public XMLConfig()
        {
            xmlText = m_default;
        }


        /// <summary>
        /// XMLファイルに保存します
        /// </summary>
        /// <param name="filepath">XMLファイルの保存先(省略すると、読み込み時のパスを使用します)</param>
        /// <returns>成功した場合はTrue、失敗した場合はFalseを返します</returns>
        public bool Save(string filepath = "")
        {

            if (filepath == "") { filepath = FilePath; }

            try { File.WriteAllText(filepath, xmlText); return true; } catch { return false; }

        }

        private string xmlText { get; set; }
        /// <summary>
        /// 現在のXMLファイルのテキストです
        /// </summary>
        public string XMLText
        {
            get => xmlText;
            set => xmlText = value;
        }
        private string[] CountSplit(string str, int count)
        {
            var list = new List<string>();
            int length = (int)Math.Ceiling((double)str.Length / count);

            for (int i = 0; i < length; i++)
            {
                int start = count * i;
                if (str.Length <= start)
                {
                    break;
                }
                if (str.Length < start + count)
                {
                    list.Add(str.Substring(start));
                }
                else
                {
                    list.Add(str.Substring(start, count));
                }
            }

            return list.ToArray();
        }
        /// <summary>
        /// 指定されたパスを読み込みます
        /// </summary>
        /// <param name="path">読み込みたいパス</param>
        /// <param name="defaultText">読み込みに失敗した場合に返されるテキスト(省略可)</param>
        /// <returns>指定されたパスに存在するテキスト</returns>
        public string Read(string path, string defaultText = "")
        {
            try
            {
                if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }

                if (!Exists(path))
                {
                    Write(path, defaultText);
                    return defaultText;
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlText);
                if (xml.SelectSingleNode("package/" + path) is not null)
                {
                    string str = xml.SelectSingleNode("package/" + path).InnerText;
                    return str;
                }
                else
                {
                    return defaultText;
                }
            }
            catch { return defaultText; }

        }
        /// <summary>
        /// 指定されたパスにある属性値を読み込みます
        /// </summary>
        /// <param name="path">読み込みたいパス</param>
        /// <param name="name">読み込みたい属性の名前</param>
        /// <param name="defaultText">読み込みに失敗した場合に返されるテキスト(省略可)</param>
        /// <returns>指定されたパスに存在する属性値</returns>
        public string ReadAttribute(string path, string name, string defaultText = "")
        {
            try
            {
                if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }

                if (!Exists(path))
                {
                    Write(path, defaultText);
                    return defaultText;
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlText);
                if (xml.SelectSingleNode("package/" + path) is not null)
                {
                    var node = xml.SelectSingleNode("package/" + path);
                    return ContainsAttribute(node, name) ? ((XmlElement)node).GetAttribute(name) : defaultText;
                }
                else
                {
                    return defaultText;
                }
            }
            catch { return defaultText; }
        }
        /// <summary>
        /// 指定されたパスに指定された属性があるかどうかを確認します
        /// </summary>
        /// <param name="path">存在を確認したいパス</param>
        /// <param name="name">存在を確認したい属性の名前</param>
        /// <returns>存在している場合はTrue、存在していない場合はFalseを返します</returns>
        public bool ExistsAttribute(string path, string name)
        {
            try
            {
                if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }

                if (!Exists(path))
                {
                    return false;
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlText);
                if (xml.SelectSingleNode("package/" + path) is not null)
                {
                    var node = xml.SelectSingleNode("package/" + path);
                    return ContainsAttribute(node, name);
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }
        private bool ContainsAttribute(XmlNode node, string name)
        {
            return ((XmlElement)node).HasAttribute(name);
        }
        /// <summary>
        /// 指定されたパスが存在するかどうか確認します
        /// </summary>
        /// <param name="path">存在を確認したいパス</param>
        /// <returns>存在している場合はTrue、存在していない場合はFalseを返します</returns>
        public bool Exists(string path)
        {
            try
            {

                if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlText);
                return xml.SelectSingleNode("package/" + path) is not null;
            }
            catch { return false; }
        }
        /// <summary>
        /// 書き込まれたときに発生するイベントです
        /// </summary>
        public WritedEventHandler Writed;
        /// <summary>
        /// 指定されたパスに書き込みます
        /// </summary>
        /// <param name="path">書き込みたいパス</param>
        /// <param name="value">書き込みたい内容</param>
        /// <returns>成功した場合はTrue、失敗した場合はFalseを返します</returns>
        public void Write(string path, string value)
        {
            path = SecurityElement.Escape(path);
            value = SecurityElement.Escape(value);


            if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }
            XmlDocument xml = new XmlDocument();

            xml.LoadXml(xmlText);

            if (xml.SelectSingleNode("package/" + path) is not null)
            {
                xml.SelectSingleNode("package/" + path).InnerText = SecurityElement.Escape(value); xmlText = xml.OuterXml; WritedEventArgs ws = new WritedEventArgs();
                ws.Path = path;
                ws.Value = value; Writed?.Invoke(null, ws); return;
            }
            else
            {

            }

            XmlNode nownode = xml.SelectSingleNode("package");
            string[] nodes = path.Split('/');
            XmlNode oldnode;

            foreach (string node in nodes)
            {
                oldnode = nownode;

                nownode = nownode.SelectSingleNode(node);

                if (nownode is null)
                {

                    oldnode.AppendChild(xml.CreateElement(node));

                    nownode = oldnode.SelectSingleNode(node);
                }
            }
            nownode.InnerText = SecurityElement.Escape(value);
            xmlText = xml.OuterXml;
            Task.Run(() =>
            {
                Save();
            });
            WritedEventArgs w = new WritedEventArgs();
            w.Path = path;
            w.Value = value;

            Writed?.Invoke(null, w);



        }
        /// <summary>
        /// 指定されたパスを削除します
        /// </summary>
        /// <param name="path">削除したいパス</param>
        /// <returns>成功した場合はTrue、失敗した場合はFalseを返します</returns>
        public bool Delete(string path)
        {
            try
            {
                if (path.EndsWith("/", StringComparison.Ordinal)) { path = path + "/default"; }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlText);
                xml.SelectSingleNode("package/" + path).ParentNode.RemoveChild(xml.SelectSingleNode("package/" + path));
                xmlText = xml.OuterXml;
                return true;
            }
            catch { return false; }
        }

    }

    /// <summary>
    /// 設定の変更時に発生するイベントの引数
    /// </summary>
    public class WritedEventArgs : EventArgs

    { /// <summary>
      /// 変更された設定のパス
      /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 変更された設定の中身(String)
        /// </summary>
        public string Value { get; set; }
    } /// <summary>
      /// 設定の変更時に発生するイベントハンドラーです
      /// </summary>
    public delegate void WritedEventHandler(object sender, WritedEventArgs e);
}
