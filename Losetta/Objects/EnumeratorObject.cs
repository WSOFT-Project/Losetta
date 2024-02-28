using AliceScript.Binding;
using System.Collections.Generic;

namespace AliceScript.Objects
{
    [AliceObject(Name = "Enumerator")]
    public class EnumeratorObject
    {
        public EnumeratorObject(IEnumerator<object> enumerator)
        {
            Enumerator = enumerator;
        }

        public IEnumerator<object> Enumerator { get; set; }
        public object Current => Enumerator.Current;
        public bool MoveNext()
        {
            return Enumerator.MoveNext();
        }
        public void Reset()
        {
            Enumerator.Reset();
        }
    }
}
