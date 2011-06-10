using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace leveldb
{
    public interface IComparator<T>
    {
        int Cmp(T a, T b);
    }

    public class SkipList<Key, Comparator> where Comparator:class, IComparator<Key>
    {
        const int kMaxHeight = 12;
        Comparator compare_;
        Node head_;
        int maxHeight_;
        Random rnd_;

        public SkipList(Comparator cmp)
        {
            compare_ = cmp;
            head_ = new Node(default(Key), kMaxHeight);
            maxHeight_ = 1;
            rnd_ = new leveldb.Random(0xdeadbeef);
        }

        public void Insert(Key key)
        {
            var prev = new Node[kMaxHeight];
            Node x = FindGreaterOrEqual(key, prev);

            // Our data structure does not allow duplicate insertion
            Debug.Assert(x == null || !Equal(key, x.key));

            int height = RandomHeight();
            if (height > GetMaxHeight())
            {
                for (int i = GetMaxHeight(); i < height; i++)
                {
                    prev[i] = head_;
                }

                // It is ok to mutate max_height_ without any synchronization
                // with concurrent readers.  A concurrent reader that observes
                // the new value of max_height_ will see either the old value of
                // new level pointers from head_ (NULL), or a new value set in
                // the loop below.  In the former case the reader will
                // immediately drop to the next level since NULL sorts after all
                // keys.  In the latter case the reader will use the new node.

                maxHeight_ = height; // TODO: locking
            }

            x = new Node(key, height);
            for (int i = 0; i < height; i++)
            {
                // NoBarrier_SetNext() suffices since we will add a barrier when
                // we publish a pointer to "x" in prev[i].
                x.NoBarrier_SetNext(i, prev[i].NoBarrier_Next(i));
                prev[i].SetNext(i, x);
            }
        }

        public bool Contains(Key key)
        {
            Node x = FindGreaterOrEqual(key, null);
            return (x != null) && Equal(key, x.key);
        }

        public Iterator GetIterator()
        {
            return new Iterator(this);
        }

        public class Iterator
        {
            SkipList<Key, Comparator> list_;
            Node node_;

            internal Iterator(SkipList<Key, Comparator> list)
            {
                list_ = list;
                node_ = null;
            }

            public bool Valid()
            {
                return node_ != null;
            }

            public Key key()
            {
                Debug.Assert(Valid());
                return node_.key;
            }

            public void Next()
            {
                Debug.Assert(Valid());
                node_ = node_.Next(0);
            }

            public void Prev()
            {
                Debug.Assert(Valid());
                node_ = list_.FindLessThan(node_.key);
                if (node_ == list_.head_)
                {
                    node_ = null;
                }
            }

            public void Seek(Key target)
            {
                node_ = list_.FindGreaterOrEqual(target, null);
            }

            public void SeekToFirst()
            {
                node_ = list_.head_.Next(0);
            }

            public void SeekToLast()
            {
                node_ = list_.FindLast();
                if (node_ == list_.head_)
                {
                    node_ = null;
                }
            }
        }

        class Node
        {
            internal Node(Key k)
            {
                key = k;
                next_ = new Node[1];
            }

            internal Node(Key k, int height)
            {
                key = k;
                next_ = new Node[height];
            }

            // TODO: locking
            internal Node Next(int n)
            {
                return next_[n];
            }

            // TODO: locking
            internal void SetNext(int n, Node x)
            {
                next_[n] = x;
            }

            // TODO: locking
            internal Node NoBarrier_Next(int n)
            {
                return next_[n];
            }

            // TODO: locking
            internal void NoBarrier_SetNext(int n, Node x)
            {
                next_[n] = x;
            }

            internal Key key;
            Node[] next_;
        }

        int GetMaxHeight()
        {
            // TODO: locking
            return maxHeight_;
        }

        int RandomHeight()
        {
            // Increase height with probability 1 in kBranching
            const uint kBranching = 4;
            int height = 1;
            while (height < kMaxHeight && ((rnd_.Next() % kBranching) == 0)) {
                height++;
            }
            Debug.Assert(height > 0);
            Debug.Assert(height <= kMaxHeight);
            return height;
        }

        bool Equal(Key a, Key b)
        {
            return 0 == compare_.Cmp(a, b);
        }

        bool KeyIsAfterNode(Key key, Node n)
        {
            return (n != null) && (compare_.Cmp(n.key, key) < 0);
        }

        Node FindGreaterOrEqual(Key key, Node[] prev)
        {
            Node x = head_;
            int level = GetMaxHeight() - 1;
            while (true)
            {
                Node next = x.Next(level);
                if (KeyIsAfterNode(key, next))
                {
                    // Keep searching in this list
                    x = next;
                }
                else
                {
                    if (prev != null)
                    {
                        prev[level] = x;
                    }
                    if (level == 0)
                    {
                        return next;
                    }
                    else
                    {
                        // Switch to next list
                        level--;
                    }
                }
            }
        }

        Node FindLessThan(Key key)
        {
            Node x = head_;
            int level = GetMaxHeight() - 1;
            while (true)
            {
                Debug.Assert(x == head_ || compare_.Cmp(x.key, key) < 0);
                Node next = x.Next(level);
                if (next == null || compare_.Cmp(next.key, key) >= 0)
                {
                    if (level == 0)
                    {
                        return x;
                    }
                    else
                    {
                        // Switch to next list
                        level--;
                    }
                }
                else
                {
                    x = next;
                }
            }
        }

        Node FindLast()
        {
            Node x = head_;
            int level = GetMaxHeight() - 1;
            while (true)
            {
                Node next = x.Next(level);
                if (next == null)
                {
                    if (level == 0)
                    {
                        return x;
                    }
                    else
                    {
                        // Switch to next list
                        level--;
                    }
                }
                else
                {
                    x = next;
                }
            }
        }
    }
}
