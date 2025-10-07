using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public static class ByteOrder
    {
        //Switch Order of Short type
        public static short Flip(short num)
        {
            return (short)Flip((ushort)num);
        }

        //Switch Order of Unsigned Short type
        //i.e. ab becomes ba
        public static ushort Flip(ushort num)
        {
            return (ushort)(((num & 0xFF00) >> 8) | ((num & 0x00FF) << 8));
        }

        //Switch Order of Integer (32 bit) type
        public static int Flip(int num)
        {
            return (int)Flip((uint)num);
        }

        //Switch Order of Unsigned Int32 (32 bit) type
        // i.e. abcd becomes dcba
        public static uint Flip(uint num)
        {
            return ((num & 0xff000000) >> 24) | ((num & 0x00ff0000) >> 8) | ((num & 0x0000ff00) << 8) | ((num & 0x000000ff) << 24);
        }

        //Switch Order of Integer 64 (Int64) type
        public static long Flip(long num)
        {
            return (long)Flip((ulong)num);
        }


        //Switch Order of Unsigned Integer 64 (64 bit) type
        //i.e. abcdefgh becomes hgfedcba
        public static ulong Flip(ulong num)
        {
            return ((num & 0xFF00000000000000) >> 56) | ((num & 0x00FF000000000000) >> 40) |
                   ((num & 0x0000FF0000000000) >> 24) | ((num & 0x000000FF00000000) >> 8) |
                   ((num & 0x00000000FF000000) << 8) | ((num & 0x0000000000FF0000) << 24) |
                   ((num & 0x000000000000FF00) << 40) | ((num & 0x00000000000000FF) << 56);
        }
    }
}
