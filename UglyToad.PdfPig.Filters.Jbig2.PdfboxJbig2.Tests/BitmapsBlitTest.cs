namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using System.Linq;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests.Images;
    using Xunit;

    public class BitmapsBlitTest
    {
        [Fact]
        public void CompleteBitmapTransferTest()
        {
            var iis = new ImageInputStream(ImageHelpers.LoadFileBytes("042_1.jb2").AsSpan());
            using var doc = new Jbig2Document(iis);

            Jbig2Bitmap src = doc.GetPage(1).GetBitmap();
            var dst = new Jbig2Bitmap(src.Width, src.Height);
            Jbig2Bitmaps.Blit(src, dst, 0, 0, CombinationOperator.REPLACE);

            Assert.True(src.ByteArray.AsSpan().SequenceEqual(dst.ByteArray));
        }

        [Fact]
        public void BlitAtNonByteAlignedOffsetTest()
        {
            var iis = new ImageInputStream(ImageHelpers.LoadFileBytes("042_1.jb2").AsSpan());
            using var doc = new Jbig2Document(iis);

            Jbig2Bitmap dst = doc.GetPage(1).GetBitmap();

            var roi = new Jbig2Rectangle(100, 100, 100, 100);
            var src = new Jbig2Bitmap(roi.Width, roi.Height);
            Jbig2Bitmaps.Blit(src, dst, roi.X, roi.Y, CombinationOperator.REPLACE);

            Jbig2Bitmap dstRegionBitmap = Jbig2Bitmaps.Extract(roi, dst);

            Assert.True(src.ByteArray.AsSpan().SequenceEqual(dstRegionBitmap.ByteArray));
        }
    }
}
