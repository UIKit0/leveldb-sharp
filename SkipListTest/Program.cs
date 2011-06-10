using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using leveldb;

namespace SkipListTest
{
    class Program
    {

        class IntComparator : IComparator<UInt64>
        {
            public int Cmp(UInt64 a, UInt64 b)
            {
                if (a < b)
                {
                    return -1;
                }
                else if (a > b)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        void TestEmpty()
        {
            IntComparator cmp = new IntComparator();
            var list = new SkipList<UInt64, IntComparator>(cmp);
            Trace.Assert(!list.Contains(10));
            var iter = list.GetIterator();
            Trace.Assert(!iter.Valid());
            iter.SeekToFirst();
            Trace.Assert(!iter.Valid());
            iter.Seek(100);
            Trace.Assert(!iter.Valid());
            iter.SeekToLast();
            Trace.Assert(!iter.Valid());
        }

        static Tuple<IEnumerator<ulong>, bool> LowerBoundEnum(SortedSet<ulong> ss, ulong n)
        {
            var iter = ss.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current >= n)
                {
                    return new Tuple<IEnumerator<ulong>, bool>(iter, true);
                }
            }
            return new Tuple<IEnumerator<ulong>, bool>(iter, false);
        }

        void TestInsertAndLookup()
        {
            const int N = 2000;
            const int R = 5000;
            var rnd = new leveldb.Random(1000);
            var keys = new SortedSet<UInt64>();
            var cmp = new IntComparator();
            var list = new SkipList<UInt64, IntComparator> (cmp);
            for (int i = 0; i < N; i++) {
                UInt64 key = rnd.Next() % R;
                if (!keys.Contains(key))
                {
                    keys.Add(key);
                    list.Insert(key);
                }
            }

            for (UInt64 i = 0; i < R; i++) {
                if (list.Contains(i)) {
                    Trace.Assert(keys.Contains(i));
                } else {
                    Trace.Assert(!keys.Contains(i));
                }
            }

            // Simple iterator tests
            {
                var iter = list.GetIterator();
                Trace.Assert(!iter.Valid());

                iter.Seek(0);
                Trace.Assert(iter.Valid());
                var keysEnum = keys.GetEnumerator();
                keysEnum.MoveNext();
                Trace.Assert(keysEnum.Current == iter.key());

                iter.SeekToFirst();
                Trace.Assert(iter.Valid());
                Trace.Assert(keysEnum.Current == iter.key());

                iter.SeekToLast();
                Trace.Assert(iter.Valid());
                var keysEnum2 = keys.Reverse().GetEnumerator();
                keysEnum2.MoveNext();
                Trace.Assert(keysEnum2.Current == iter.key());
            }

            // Forward iteration test
            for (ulong i = 0; i < R; i++) {
                var iter = list.GetIterator();
                iter.Seek(i);

                // Compare against model iterator
                var modelIterTuple = LowerBoundEnum(keys, i);
                if (!modelIterTuple.Item2)
                {
                    Trace.Assert(!iter.Valid());
                    continue;
                }
                var modelIter = modelIterTuple.Item1;
                for (int j = 0; j < 3; j++) {
                    Trace.Assert(iter.Valid());
                    Trace.Assert(modelIter.Current == iter.key());
                    iter.Next();
                    if (!modelIter.MoveNext())
                    {
                        Trace.Assert(!iter.Valid());
                        break;
                    }
                }
            }

            // Backward iteration test
            {
                var iter = list.GetIterator();
                iter.SeekToLast();
                // Compare against model iterator
                foreach (var k in keys.Reverse())
                {
                    Trace.Assert(iter.Valid());
                    Trace.Assert(k == iter.key());
                    iter.Prev();
                }
                Trace.Assert(!iter.Valid());
            }
        }

        static void Main(string[] args)
        {
            Program t = new Program();
            t.TestEmpty();
            t.TestInsertAndLookup();
        }
    }
}
