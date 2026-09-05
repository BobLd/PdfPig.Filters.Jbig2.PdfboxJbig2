namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests.Images;
    using Xunit;

    public class PatternDictionaryTest
    {
        [Fact]
        public void ParseHeaderTest()
        {
            var iis = new ImageInputStream(ImageHelpers.LoadFileBytes("sampledata.jb2").AsMemory());
            // Sixth Segment (number 5)
            var sis = new SubInputStream(iis, 245, 45);
            var pd = new PatternDictionary();
            pd.Init(null, sis);

            Assert.True(pd.IsMMREncoded);
            Assert.Equal(0, pd.HdTemplate);
            Assert.Equal(4, pd.HdpWidth);
            Assert.Equal(4, pd.HdpHeight);
            Assert.Equal(15, pd.GrayMax);
        }
    }
}
