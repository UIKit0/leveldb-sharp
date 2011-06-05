using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leveldb
{
    class ByteBuffer
    {
        public void Extend(int size)
        {
            if (size > data_.Length)
            {
                int newLen = size > data_.Length * 2 ? size : data_.Length * 2;
                byte[] newArr = new byte[newLen];
                Array.Copy(data_, 0, newArr, 0, used_);
                data_ = newArr;
            }
            used_ = size;
        }

        byte[] data_;
        int used_;
    }
}
