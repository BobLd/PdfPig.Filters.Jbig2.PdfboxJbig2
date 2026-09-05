namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using Xunit;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests.Images;

    public class HalftoneRegionTest
    {
        [Fact]
        public void ParseHeaderTest()
        {
            var iis = new ImageInputStream(ImageHelpers.LoadFileBytes("sampledata.jb2").AsMemory());
            // Seventh Segment (number 6)
            var sis = new SubInputStream(iis, 302, 87);
            var hr = new HalftoneRegion(sis);
            hr.Init(null, sis);

            Assert.True(hr.IsMMREncoded);
            Assert.Equal(0, hr.HTemplate);
            Assert.False(hr.HSkipEnabled);
            Assert.Equal(CombinationOperator.OR, hr.HCombinationOperator);
            Assert.Equal(0, hr.HDefaultPixel);
            Assert.Equal(8, hr.HGridWidth);
            Assert.Equal(9, hr.HGridHeight);
            Assert.Equal(0, hr.HGridX);
            Assert.Equal(0, hr.HGridY);
            Assert.Equal(1024, hr.HRegionX);
            Assert.Equal(0, hr.HRegionY);
        }

        [Fact]
        public void HighestOneBitTest()
        {
            for (int i = -65_536; i <= 65_536; ++i)
            {
                int h = i.HighestOneBit();
                int hExpected = HighestOneBitLocal(i);
                Assert.Equal(hExpected, h);
            }
        }

        public static int HighestOneBitLocal(int number)
        {
            // This is the original implementation
            return (int)Math.Pow(2, Convert.ToString(number, 2).Length - 1);
        }
    }
}
