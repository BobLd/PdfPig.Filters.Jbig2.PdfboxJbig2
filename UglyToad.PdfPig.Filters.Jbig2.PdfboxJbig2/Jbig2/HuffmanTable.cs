namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This abstract class is the base class for all types of Huffman tables.
    /// </summary>
    internal abstract class HuffmanTable
    {
        private readonly InternalNode rootNode = new InternalNode();

        /// <summary>
        ///  This inner class represents a code for use in Huffman tables.
        /// </summary>
        internal sealed class Code
        {
            public int PrefixLength { get; }
            public int RangeLength { get; }
            public int RangeLow { get; }
            public bool IsLowerRange { get; }

            public int Value { get; set; } = -1;

            public Code(int prefixLength, int rangeLength, int rangeLow, bool isLowerRange)
            {
                PrefixLength = prefixLength;
                RangeLength = rangeLength;
                RangeLow = rangeLow;
                IsLowerRange = isLowerRange;
            }

            public override string ToString()
            {
                return (Value != -1 ? ValueNode.BitPattern(Value, PrefixLength) : "?") + "/"
                        + PrefixLength + "/" + RangeLength + "/" + RangeLow;
            }
        }

        public void InitTree(List<Code> codeTable)
        {
            PreprocessCodes(codeTable);

            foreach (var c in codeTable)
            {
                rootNode.Append(c);
            }
        }

        public long Decode(IImageInputStream iis)
        {
            return rootNode.Decode(iis);
        }

        public override sealed string ToString()
        {
            return rootNode + "\n";
        }

        public static string CodeTableToString(List<Code> codeTable)
        {
            var sb = new StringBuilder();

            foreach (var c in codeTable)
            {
                sb.Append(c.ToString()).Append("\n");
            }

            return sb.ToString();
        }

        private static void PreprocessCodes(List<Code> codeTable)
        {
            // Annex B.3 1) - build the histogram
            int maxPrefixLength = 0;

            foreach (Code c in codeTable)
            {
                maxPrefixLength = Math.Max(maxPrefixLength, c.PrefixLength);
            }

            // Prefix lengths are at most 32 in a well formed table, but the value is read straight from
            // the stream, so only take the stack path when it is actually within that bound.
            const int MaxStackPrefixLength = 32;
            bool onStack = maxPrefixLength <= MaxStackPrefixLength;

            Span<int> lenCountBuffer = onStack
                ? stackalloc int[MaxStackPrefixLength + 1]
                : new int[maxPrefixLength + 1];
            Span<int> lenCount = lenCountBuffer.Slice(0, maxPrefixLength + 1);
            lenCount.Clear();

            foreach (Code c in codeTable)
            {
                lenCount[c.PrefixLength]++;
            }

            int curCode;
            Span<int> firstCodeBuffer = onStack
                ? stackalloc int[MaxStackPrefixLength + 2]
                : new int[lenCount.Length + 1];
            Span<int> firstCode = firstCodeBuffer.Slice(0, lenCount.Length + 1);
            firstCode.Clear();

            lenCount[0] = 0;

            // Annex B.3 3)
            for (int curLen = 1; curLen <= lenCount.Length; curLen++)
            {
                firstCode[curLen] = firstCode[curLen - 1] + lenCount[curLen - 1] << 1;
                curCode = firstCode[curLen];
                foreach (var code in codeTable)
                {
                    if (code.PrefixLength == curLen)
                    {
                        code.Value = curCode;
                        curCode++;
                    }
                }
            }
        }
    }
}
