using System.Collections;
using System.Collections.Generic;

namespace AliceScript.Objects
{
    public class VariableCollection : IEnumerable<Variable>, IList<Variable>
    {
        public List<Variable> Tuple
        {
            get => m_tuple;
            set => m_tuple = value;
        }
        public TypeObject Type { get; set; }

        /// <summary>
        /// この配列にその項目を追加できるかを検証します
        /// </summary>
        /// <param name="item">対象の項目</param>
        /// <returns>追加できればTrue、それ以外の場合はFalse。</returns>
        private bool CanAdd(Variable item)
        {
            if (Type is not null && Type.Type == Variable.VarType.VOID && item is not null)
            {
                if (Type.Type != item.Type)
                {
                    return false;
                }
                else if (Type.Type == Variable.VarType.OBJECT && item.Object is AliceScriptClass c && Type.ClassType.Equals(c))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanAdd(IEnumerable<Variable> items)
        {
            foreach (Variable item in items)
            {
                if (!CanAdd(item))
                {
                    return false;
                }
            }
            return true;
        }

        public int Count => m_tuple.Count;

        public bool IsReadOnly => false;

        public Variable this[int index] { get => m_tuple[index]; set => Insert(index, value); }

        private List<Variable> m_tuple = new List<Variable>();
        public IEnumerator<Variable> GetEnumerator()
        {
            return m_tuple.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_tuple.GetEnumerator();
        }

        public int IndexOf(Variable item)
        {
            return m_tuple.IndexOf(item);
        }

        public void Insert(int index, Variable item)
        {
            if (CanAdd(item))
            {
                m_tuple.Insert(index, item);
            }
            else
            {
                throw new ScriptException("現在の配列にはその型を代入することができません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
        }

        public void RemoveAt(int index)
        {
            m_tuple.RemoveAt(index);
        }

        public void Add(Variable item)
        {
            if (item is null)
            {
                return;
            }
            if (CanAdd(item))
            {
                m_tuple.Add(item);
            }
            else
            {
                throw new ScriptException("現在の配列にはその型を代入することができません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
        }
        public void AddRange(IEnumerable<Variable> items)
        {
            if (CanAdd(items))
            {
                m_tuple.AddRange(items);
            }
            else
            {
                throw new ScriptException("現在の配列にはその型を代入することができません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
        }
        public void Clear()
        {
            m_tuple.Clear();
        }

        public bool Contains(Variable item)
        {
            return m_tuple.Contains(item);
        }

        public void CopyTo(Variable[] array, int arrayIndex)
        {
            if (CanAdd(array))
            {
                m_tuple.CopyTo(array, arrayIndex);
            }
            else
            {
                throw new ScriptException("現在の配列にはその型を代入することができません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
        }

        public bool Remove(Variable item)
        {
            return m_tuple.Remove(item);
        }
    }
}
