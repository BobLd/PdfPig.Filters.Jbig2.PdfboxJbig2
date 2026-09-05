namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2
{
    /// <summary>
    /// This class represents the arithmetic integer decoder, described in ISO/IEC 14492:2001 (Annex A).
    /// </summary>
    internal sealed class ArithmeticIntegerDecoder
    {
        private readonly ArithmeticDecoder decoder;

        public ArithmeticIntegerDecoder(ArithmeticDecoder decoder)
        {
            this.decoder = decoder;
        }

        /// <summary>
        /// Arithmetic Integer Decoding Procedure, Annex A.2.
        /// </summary>
        /// <param name="cxIAx">cxIAx to be decoded</param>
        /// <returns>Decoded value.</returns>
        public long Decode(CX cxIAx)
        {
            // A.2.
            // CX is identified by … the rightmost 9 bits of PREV
            // ... Thus, PREV always contains the values of the eight most-recently-decoded bits,
            // plus a leading 1 bit, which is used to indicate the number of bits decoded so far.
            int prev = 1;

            int v = 0;
            int d, s;

            int bitsToRead;
            int offset;

            if (cxIAx is null)
            {
                cxIAx = new CX(512, 1);
            }

            cxIAx.Index = prev & 0x1FF;
            s = decoder.Decode(cxIAx);
            prev = SetPrev(prev, s);

            cxIAx.Index = prev & 0x1FF;
            d = decoder.Decode(cxIAx);
            prev = SetPrev(prev, d);

            if (d == 1)
            {
                cxIAx.Index = prev & 0x1FF;
                d = decoder.Decode(cxIAx);
                prev = SetPrev(prev, d);

                if (d == 1)
                {
                    cxIAx.Index = prev & 0x1FF;
                    d = decoder.Decode(cxIAx);
                    prev = SetPrev(prev, d);

                    if (d == 1)
                    {
                        cxIAx.Index = prev & 0x1FF;
                        d = decoder.Decode(cxIAx);
                        prev = SetPrev(prev, d);

                        if (d == 1)
                        {
                            cxIAx.Index = prev & 0x1FF;
                            d = decoder.Decode(cxIAx);
                            prev = SetPrev(prev, d);

                            if (d == 1)
                            {
                                bitsToRead = 32;
                                offset = 4436;
                            }
                            else
                            {
                                bitsToRead = 12;
                                offset = 340;
                            }
                        }
                        else
                        {
                            bitsToRead = 8;
                            offset = 84;
                        }
                    }
                    else
                    {
                        bitsToRead = 6;
                        offset = 20;
                    }
                }
                else
                {
                    bitsToRead = 4;
                    offset = 4;
                }
            }
            else
            {
                bitsToRead = 2;
                offset = 0;
            }

            for (int i = 0; i < bitsToRead; i++)
            {
                cxIAx.Index = prev & 0x1FF;
                d = decoder.Decode(cxIAx);
                prev = SetPrev(prev, d);
                v = v << 1 | d;
            }

            v += offset;

            if (s == 0)
            {
                return v;
            }

            if (s == 1 && v > 0)
            {
                return -v;
            }

            return long.MaxValue;
        }

        /// <summary>
        /// The IAID decoding procedure, Annex A.3.
        /// </summary>
        /// <param name="cxIAID">The contexts and statistics for decoding procedure.</param>
        /// <param name="symCodeLen">Symbol code length</param>
        /// <returns>The decoded value</returns>
        public int DecodeIAID(CX cxIAID, long symCodeLen)
        {
            // A.3 1)
            long prev = 1;

            // A.3 2)
            // The spec says: "the rightmost SBSYMCODELEN + 1 bits of PREV are used"
            // But also: "The number of contexts required is 2^SBSYMCODELEN"
            // The resolution: the leading 1 bit is not used for context
            // identification—only the lower N bits are.
            long mask = (1L << (int)symCodeLen) - 1;

            for (int i = 0; i < symCodeLen; i++)
            {
                cxIAID.Index = (int)(prev & mask);
                prev = prev << 1 | decoder.Decode(cxIAID);
            }

            // A.3 3) & 4)
            return (int)(prev - (1L << (int)symCodeLen));
        }

        private static int SetPrev(int prev, int bit)
        {
            if (prev < 256)
            {
                prev = (prev << 1 | bit) & 0x1ff;
            }
            else
            {
                prev = ((prev << 1 | bit) & 511 | 256) & 0x1ff;
            }

            return prev;
        }
    }
}
