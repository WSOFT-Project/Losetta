using System;
using System.Collections.Generic;

namespace AliceScript
{
    public class WordHint
    {
        string m_text;

        public int Id { get; }
        public string OriginalText { get; }
        public string Text { get { return m_text; } }

        public bool Equals(WordHint other)
        {
            return Id == other.Id;
        }
        public bool Exists(List<WordHint> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (Equals(others[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public WordHint(string word, int id)
        {
            OriginalText = word;
            Id = id;
            m_text = Utils.RemovePrefix(OriginalText);
        }
    }
    public class TrieCell
    {
        string m_name;
        WordHint m_wordHint;

        Dictionary<string, TrieCell> m_children = new Dictionary<string, TrieCell>();

        public int Level { get; set; }

        public TrieCell(string name = "", WordHint wordHint = null, int level = 0)
        {
            if (wordHint != null && wordHint.Text == name)
            {
                m_wordHint = wordHint;
            }

            m_name = name;
            Level = level;
        }

        public bool AddChild(WordHint wordHint)
        {
            if (!string.IsNullOrEmpty(m_name) &&
                !wordHint.Text.StartsWith(m_name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int newLevel = Level + 1;

            bool lastChild = newLevel >= wordHint.Text.Length;

            string newName = lastChild ? wordHint.Text :
                                         wordHint.Text.Substring(0, newLevel);

            TrieCell oldChild = null;
            if (m_children.TryGetValue(newName, out oldChild))
            {
                return oldChild.AddChild(wordHint);
            }

            TrieCell newChild = new TrieCell(newName, wordHint, newLevel);
            m_children[newName] = newChild;

            if (newLevel < wordHint.Text.Length)
            { // if there are still chars left, add a grandchild recursively.
                newChild.AddChild(wordHint);
            }

            return true;
        }
    }

}

