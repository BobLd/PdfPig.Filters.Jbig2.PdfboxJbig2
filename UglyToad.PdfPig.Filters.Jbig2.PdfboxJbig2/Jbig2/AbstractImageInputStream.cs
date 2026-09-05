namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal abstract class AbstractImageInputStream : IImageInputStream
    {
        private readonly Stack<(long streamPos, int bitOffset)> markedPositions = new Stack<(long, int)>();

        private int bitOffset;

        /// <summary>
        /// The byte the bit reader is currently handing out bits from, and the stream position it was
        /// read from. Reading the eight bits of one byte used to pull that byte from the underlying
        /// stream eight times, rewinding after each one; the stream content does not change while
        /// decoding, so remembering it makes seven of those reads unnecessary.
        /// </summary>
        private long cachedBytePosition = -1;

        private int cachedByte;

        /// <inheritdoc />
        public abstract long Length { get; }

        /// <inheritdoc />
        public abstract long Position { get; }

        /// <inheritdoc />
        public abstract void Seek(long pos);

        /// <inheritdoc />
        public abstract int Read();

        /// <inheritdoc />
        public abstract int Read(Span<byte> b, int off, int len);

        /// <inheritdoc />
        public int Read(Span<byte> b)
        {
            return Read(b, 0, b.Length);
        }

        /// <inheritdoc />
        public int ReadBit()
        {
            long position = Position;
            int offset = bitOffset;
            int b;

            if (offset != 0 && cachedBytePosition == position)
            {
                // Still working through a byte we have already read.
                b = cachedByte;
            }
            else
            {
                b = ReadByte(); // advances the stream past the byte
                cachedByte = b;
                cachedBytePosition = position;

                if (offset != 7)
                {
                    // More bits of this byte still to come, so leave the position on it.
                    Seek(position);
                }
            }

            if (offset == 7 && Position == position)
            {
                // Last bit of a cached byte: step over it.
                Seek(position + 1);
            }

            // Assigned after any Seek, because seeking is allowed to reset the bit offset.
            bitOffset = offset + 1 & 7;

            return b >> 7 - offset & 1;
        }

        /// <inheritdoc />
        public long ReadBits(int numBits)
        {
            if (numBits > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(numBits));
            }

            long accum = 0L;
            for (int i = 0; i < numBits; i++)
            {
                accum <<= 1; // Shift left one bit to make room
                var bit = (long)ReadBit();
                accum |= bit;
            }

            return accum;
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            var value = Read();
            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (byte)value;
        }

        /// <inheritdoc />
        public uint ReadUnsignedInt()
        {
            Span<byte> buffer = stackalloc byte[4];
            Read(buffer);

            buffer.Reverse();

#if NET
            return BitConverter.ToUInt32(buffer);
#else
            return BitConverter.ToUInt32(buffer.ToArray(), 0);
#endif
        }

        /// <inheritdoc />
        public void Mark()
        {
            markedPositions.Push((Position, bitOffset));
        }

        /// <inheritdoc />
        public void Reset()
        {
            if (markedPositions.Count == 0)
            {
                return;
            }

            var position = markedPositions.Pop();
            Seek(position.streamPos);
            bitOffset = position.bitOffset;
        }

        /// <inheritdoc />
        public long SkipBytes(int n)
        {
            var desiredPosition = Position + n;
            if (desiredPosition > Length)
            {
                Seek(Length);
                return desiredPosition - Length;
            }

            Seek(desiredPosition);
            return n;
        }

        /// <inheritdoc />
        public void SkipBits()
        {
            if (bitOffset == 0)
            {
                return;
            }

            bitOffset = 0;
            if (Position < Length)
            {
                Seek(Position + 1);
            }
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Sets the bit offset to an integer between 0 and 7, inclusive. The byte offset
        /// within the stream, as returned by getStreamPosition, is left unchanged.
        /// A value of 0 indicates the most-significant bit, and a value of 7 indicates
        /// the least significant bit, of the byte being read.
        /// </summary>
        /// <param name="bitOffset">the desired offset, as an int between 0 and 7, inclusive.</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown if bitOffset is not between 0 and 7, inclusive.</exception>
        protected void SetBitOffset(int bitOffset)
        {
            if (bitOffset < 0 || bitOffset > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(bitOffset), "must be betwwen 0 and 7!");
            }

            this.bitOffset = bitOffset;
        }

        protected bool IsAtEnd()
        {
            return Position == Length;
        }
    }
}