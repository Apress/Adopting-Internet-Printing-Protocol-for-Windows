using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
    /// SafeStreamRead
    /// 
    /// Created to address stream read hangs due to stream byte delivery issues. This was due primarily to use of ReadAsync calls that did not guarantee full byte reads.
    /// Stream read helpers: ALWAYS read exactly N bytes or throw (try to eliminate hangs). 
    /// 
    /// Using ReadAsync, a partial read can make variable len too large as the actual strema bytes are less than requested. Thus, the result is waiting trying to read 
    /// nameBuffer or the value bytes that will never arrive.

    /// </summary>
    public static class SafeStreamRead
    {
        /// <summary>
        /// ReadExactlyAsync
        /// 
        /// The lynchpin of this class. Reads exactly count bytes from stream s into buf at offset. TCP delivers a continuous, ordered stream of bytes that
        /// isn't guaranteed to arrive in the same chunks as requested (re-assembly?). Thus, multiple reads may be required to get the full count of bytes requested.
        /// 
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"></exception>
        public static async Task ReadExactlyAsync(Stream s, byte[] buf, int offset, int count, CancellationToken ct = default)
        {
            int total = 0;
            while (total < count)
            {
                int n = await s.ReadAsync(buf, offset + total, count - total, ct).ConfigureAwait(false);
                if (n == 0)
                    throw new EndOfStreamException($"Unexpected EOF. Needed {count} bytes, got {total}.");
                total += n;
            }
        }

        public static async Task<byte> ReadByteAsync(Stream s, CancellationToken ct = default)
        {
            var b = new byte[1];
            await ReadExactlyAsync(s, b, 0, 1, ct).ConfigureAwait(false);
            return b[0];
        }

        public static async Task<ushort> ReadUInt16Async(Stream s, CancellationToken ct = default)
        {
            var b = new byte[2];
            await ReadExactlyAsync(s, b, 0, 2, ct).ConfigureAwait(false);
            return (ushort)((b[0] << 8) | b[1]);
        }

        public static async Task<int> ReadInt32Async(Stream s, CancellationToken ct = default)
        {
            var b = new byte[4];
            await ReadExactlyAsync(s, b, 0, 4, ct).ConfigureAwait(false);
            return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
        }
    }
}
