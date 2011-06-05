using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leveldb
{
    public class Slice
    {
        public Slice()
        {
            size_ = off_ = 0;
        }

        public Slice(byte[] data, int size)
        {
            data_ = data;
            off_ = 0;
            size_ = size;
        }

        public int Size()
        {
            return size_;
        }

        public bool IsEmpty()
        {
            return size_ == 0;
        }

        public byte this[int i]
        {
            get
            {
                return data_[off_ + i];
            }
        }

        // Drop the first "n" bytes from this slice.
        public void RemovePrefix(int n)
        {
            off_ += n;
            size_ -= n;
        }

        byte[] data_;
        int off_;
        int size_;
    }
}
