using AliceScript.Binding;
using System;

namespace AliceScript.Objects
{
    [AliceObject(Name ="Range")]
    public struct RangeStruct
    {
        public RangeStruct(int start, int end)
        {
            m_fromStart = false;
            Start = start;
            End = end;
        }
        public RangeStruct(int start)
        {
            m_fromStart = true;
            Start = start;
            End = 0;
        }
        public RangeStruct ToActuallyRange(int count)
        {
            if (Start < 0)
            {
                Start += count;
            }
            if(m_fromStart)
            {
                End = count;
            }
            if (End < 0)
            {
                End += count;
            }
            var length = Math.Max(0, End - Start);
            return new RangeStruct(Start, length);
        }
        [AliceObjectOperator(Operator = ":")]
        public RangeStruct TwoRangeOperator(int i)
        {
            return new RangeStruct(Start, i);
        }
        public int Start { get; set; }
        public int End{get;set;}
        private bool m_fromStart{get;set;}
    }
}