namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2
{
    /// <summary>
    /// CX represents the context used by arithmetic decoding and arithmetic integer decoding. It selects the probability
    /// estimate and statistics used during decoding procedure.
    /// </summary>
    internal sealed class CX
    {
        private readonly byte[] cx;
        private readonly byte[] mps;

        public int Index { get; set; }

        public int Cx
        {
            get => cx[Index] & 0x7f;
            set => cx[Index] = (byte)(value & 0x7f);
        }

        /// <summary>
        /// Returns the decision. Possible values are 0 or 1.
        /// </summary>
        public byte Mps => mps[Index];

        /// <summary>
        /// Creates a new <see cref="CX"/> instance
        /// </summary>
        /// <param name="size">Number of context values</param>
        /// <param name="index">Start index</param>
        public CX(int size, int index)
        {
            Index = index;
            cx = new byte[size];
            mps = new byte[size];
        }

        /// <summary>
        /// Flips the bit in actual "more predictable symbol" array element.
        /// </summary>
        public void ToggleMps()
        {
            mps[Index] ^= 1;
        }

        /// <summary>
        /// Creates and returns a deep copy of this <see cref="CX"/> instance.
        /// The new instance will have the same context values, probability estimates,
        /// and current index as this object, but will be a separate instance.
        /// Changes to the copied object will not affect the original, and vice versa.
        /// <para>
        /// This is required when reusing arithmetic coding contexts across segments,
        /// to avoid sharing mutable probability state between decoders.
        /// </para>
        /// </summary>
        public CX Copy()
        {
            var result = new CX(cx.Length, Index);
            System.Array.Copy(cx, result.cx, cx.Length);
            System.Array.Copy(mps, result.mps, mps.Length);
            return result;
        }
    }
}