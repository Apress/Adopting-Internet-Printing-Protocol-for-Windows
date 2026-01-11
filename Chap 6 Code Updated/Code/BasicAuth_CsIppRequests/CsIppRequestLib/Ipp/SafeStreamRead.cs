using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
    /// SafeStreamRead
    ///
    /// Because the originally used ReadAsync could "short read" (return fewer bytes than available) due to TCP driver
    /// delivery than requested, we need to loop until the full requested amount is read. This prevents some of the difficult
    /// to troubledhoot hangs and timeouts experienced in the original code. The following are exact-read helpers
    /// and a 1-byte pushback facility for more accurate stream reads.
    ///
    /// </summary>
    internal static class SafeStreamRead
    {
        private sealed class PushbackState
        {
            public bool AlreadyContains;
            public byte Value;
        }

        private static readonly ConditionalWeakTable<Stream, PushbackState> _pushback = new ConditionalWeakTable<Stream, PushbackState>();

        /// <summary>
        /// PushbackByte
        /// Push a single byte back onto the stream. Next read will consume it first.
        /// </summary>
        public static void PushbackByte(Stream s, byte b)
        {
            if (s == null)
            {
                throw new Exception("Null value found for: " + nameof(s));
            }

            var st = _pushback.GetOrCreateValue(s);
            if (st.AlreadyContains)
            {
                throw new Exception("Only 1-byte pushback supported.");
            }

            st.AlreadyContains = true;
            st.Value = b;
        }

        /// <summary>
        /// ReadExactlyWithPushbackAsync
        /// Read exactly N bytes into buffer, honoring a pending pushback byte first.
        /// </summary>
        private static async Task ReadExactlyWithPushbackAsync(Stream s, byte[] buf, int offset, int count, CancellationToken ct)
        {
            if (s == null)
            {
                throw new Exception("Null value found for: " + nameof(s));
            }
            if (buf == null)
            {
                throw new Exception("Null value found for byte buffer: " + nameof(buf));
            }
            if (offset < 0 || count < 0 || offset + count > buf.Length)
            {
                throw new Exception("The offset value found was invalid for: " + nameof(offset));
            }

            if (count == 0)
            {
                return;
            }

            var st = _pushback.GetOrCreateValue(s);

            // Consume the pushed byte first if present.
            if (st.AlreadyContains)
            {
                buf[offset] = st.Value;
                st.AlreadyContains = false;
                offset += 1;
                count -= 1;
                if (count == 0)
                {
                    return;
                }
            }

            await ReadExactlyAsync(s, buf, offset, count, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// ReadExactlyAsync
        /// Read exactly N bytes into buffer (no pushback handling).
        /// </summary>
        public static async Task ReadExactlyAsync(Stream s, byte[] buf, int offset, int count, CancellationToken ct = default)
        {
            if (s == null)
            {
                throw new Exception("Null value found for: " + nameof(s));
            }
            if (buf == null)
            {
                throw new Exception("Null value found for byte buffer: " + nameof(buf));
            }
            if (offset < 0 || count < 0 || offset + count > buf.Length)
            {
                throw new Exception("The offset value found was invalid for: " + nameof(offset));
            }

            int total = 0;
            while (total < count)
            {
                ct.ThrowIfCancellationRequested();

                int n = await s.ReadAsync(buf, offset + total, count - total, ct).ConfigureAwait(false);
                if (n == 0)
                {
                    throw new Exception($"Unexpected EOF. Needed {count} bytes, got {total}.");
                }

                total += n;
            }
        }

        /// <summary>
        /// ReadByteAsync
        /// Read 1 byte, honoring pushback. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<byte> ReadByteAsync(Stream s, CancellationToken ct = default)
        {
            var buf = new byte[1];
            await ReadExactlyWithPushbackAsync(s, buf, 0, 1, ct).ConfigureAwait(false);
            return buf[0];
        }

        /// <summary>
        /// ReadUInt16Async
        /// Read a UInt16 in network byte order (big-endian).
        /// </summary>
        public static async Task<ushort> ReadUInt16Async(Stream s, CancellationToken ct = default)
        {
            var buf = new byte[2];
            await ReadExactlyWithPushbackAsync(s, buf, 0, 2, ct).ConfigureAwait(false);
            return (ushort)((buf[0] << 8) | buf[1]);
        }

        /// <summary>
        /// ReadInt16Async
        /// Read an Int16 in network byte order (big-endian).
        /// </summary>
        public static async Task<short> ReadInt16Async(Stream s, CancellationToken ct = default)
        {
            ushort u = await ReadUInt16Async(s, ct).ConfigureAwait(false);
            return unchecked((short)u);
        }

        /// <summary>
        /// ReadInt32Async
        /// Read an Int32 in network byte order (big-endian).
        /// </summary>
        public static async Task<int> ReadInt32Async(Stream s, CancellationToken ct = default)
        {
            var buf = new byte[4];
            await ReadExactlyWithPushbackAsync(s, buf, 0, 4, ct).ConfigureAwait(false);

            return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | (buf[3]);
        }

        /// <summary>
        /// ReadBytesAsync
        /// Read exactly count bytes and return them, honoring pushback.
        /// </summary>
        /// 
        public static async Task<byte[]> ReadBytesAsync(Stream s, int count, CancellationToken ct = default)
        {
            if (count < 0)
            {
                throw new Exception("Negative length.");
            }
            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] buf = new byte[count];
            await ReadExactlyWithPushbackAsync(s, buf, 0, count, ct).ConfigureAwait(false);
            return buf;
        }
    }
}

