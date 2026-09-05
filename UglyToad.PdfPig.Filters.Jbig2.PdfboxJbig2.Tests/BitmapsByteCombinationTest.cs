namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using Xunit;

    public class BitmapsByteCombinationTest
    {
        private const byte Value1 = 0xA;
        private const byte Value2 = 0xD;

        [Fact]
        public void CombineBytesOrTest()
        {
            Assert.Equal(0xF, Jbig2Bitmaps.CombineBytes(Value1, Value2, CombinationOperator.OR));
        }

        [Fact]
        public void CombineBytesAndTest()
        {
            Assert.Equal(0x8, Jbig2Bitmaps.CombineBytes(Value1, Value2, CombinationOperator.AND));
        }

        [Fact]
        public void CombineBytesXorTest()
        {
            Assert.Equal(0x7, Jbig2Bitmaps.CombineBytes(Value1, Value2, CombinationOperator.XOR));
        }

        [Fact]
        public void CombineBytesXnorTest()
        {
            Assert.Equal(0xF8, Jbig2Bitmaps.CombineBytes(Value1, Value2, CombinationOperator.XNOR));
        }

        [Fact]
        public void CombineBytesReplaceTest()
        {
            Assert.Equal(Value2, Jbig2Bitmaps.CombineBytes(Value1, Value2, CombinationOperator.REPLACE));
        }
    }
}
