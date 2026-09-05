namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests.Images;
    using Xunit;

    public class PageInformationTest
    {
        [Fact]
        public void ParseHeaderCompleteTest()
        {
            var iis = new ImageInputStream(ImageHelpers.LoadFileBytes("sampledata.jb2").AsSpan());
            // Second Segment (number 1)
            var sis = new SubInputStream(iis, 59, 19);
            var pi = new PageInformation();
            pi.Init(null, sis);

            Assert.Equal(64, pi.BitmapWidth);
            Assert.Equal(56, pi.BitmapHeight);
            Assert.Equal(0, pi.ResolutionX);
            Assert.Equal(0, pi.ResolutionY);
            Assert.True(pi.IsLossless);
            Assert.False(pi.MightContainRefinements);
            Assert.Equal(0, pi.DefaultPixelValue);
            Assert.Equal(CombinationOperator.OR, pi.CombinationOperator);
            Assert.False(pi.RequiresAuxiliaryBuffer);
            Assert.False(pi.IsCombinationOperatorOverrideAllowed);
            Assert.False(pi.IsStriped);
            Assert.Equal(0, pi.MaxStripeSize);
        }
    }
}
