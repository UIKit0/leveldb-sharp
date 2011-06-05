using System;
using System.Collections.Generic;
using System.Text;

namespace leveldb
{
    class SkipList<Key, Comparator>
    {

        class Node<Key>
        {
            public Node(Key key)
            {
                key_ = key;
                next_ = new Node<Key>[1];
            }

            public Node(Key key, int height)
            {
                key_ = key;
                next_ = new Node<Key>[height];
            }

            Key key_;
            Node<Key>[] next_;
        }

        const int kMaxHeight = 12;
        Comparator compare_;
        
    }
}
