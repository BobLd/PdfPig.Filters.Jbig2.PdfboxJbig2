namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests
{
    using System;
    using System.Security.Cryptography;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2;
    using UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests.Images;
    using Xunit;

    /// <summary>
    /// Collection of tests for https://github.com/levigo/jbig2-imageio/issues.
    /// Files: github-21.jb2/github-21.glob, ported from https://github.com/apache/pdfbox-jbig2.
    /// </summary>
    public class GithubIssuesTest
    {
        [Fact]
        public void Issue21()
        {
            const string expectedMd5 = "534ABBC4869D157E8D0D096BE1934D89";

            var globalsBytes = ImageHelpers.LoadFileBytes("github-21.glob");
            var imageBytes = ImageHelpers.LoadFileBytes("github-21.jb2");

            using var globalsDocument = new Jbig2Document(new ImageInputStream(globalsBytes.AsSpan()));

            using var doc = new Jbig2Document(new ImageInputStream(imageBytes.AsSpan()), globalsDocument.GlobalSegments);

            Jbig2Bitmap bitmap = doc.GetPage(1).GetBitmap();

            using var md5 = MD5.Create();
            byte[] digest = md5.ComputeHash(bitmap.ByteArray);

            Assert.Equal(expectedMd5, ToHexString(digest));
        }

        private static string ToHexString(byte[] bytes)
        {
            var chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = bytes[i].ToString("X2");
                chars[i * 2] = hex[0];
                chars[i * 2 + 1] = hex[1];
            }
            return new string(chars);
        }
    }
}
