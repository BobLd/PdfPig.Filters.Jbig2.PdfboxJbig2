using BenchmarkDotNet.Filters;
using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

internal static class Helpers
{
    private static readonly ParsingOptions ParsingOption = new ParsingOptions()
    {
        UseLenientParsing = true,
        SkipMissingFonts = true,
        FilterProvider = BenchmarkFilterProvider.Instance
    };

    /// <summary>
    /// Opens the PDF at <paramref name="path"/> and decodes every JBIG2 encoded image it contains,
    /// returning the resulting PNG bytes for each image found.
    /// </summary>
    public static IReadOnlyList<byte[]> ExtractAllJbig2Images(string path)
    {
        var images = new List<byte[]>();

        using var document = PdfDocument.Open(path, ParsingOption);

        for (var p = 1; p <= document.NumberOfPages; p++)
        {
            var page = document.GetPage(p);

            foreach (var image in page.GetImages())
            {
                if (IsJbig2(image.ImageDictionary) && image.TryGetPng(out var png))
                {
                    images.Add(png);
                }
            }
        }

        return images;
    }

    private static bool IsJbig2(DictionaryToken imageDictionary)
    {
        if (imageDictionary.TryGet(NameToken.Filter, out NameToken filter) &&
            filter.Data.Equals(NameToken.Jbig2Decode.Data))
        {
            return true;
        }

        if (imageDictionary.TryGet(NameToken.F, out NameToken f) &&
            f.Data.Equals(NameToken.Jbig2Decode.Data))
        {
            return true;
        }

        return false;
    }

    private sealed class BenchmarkFilterProvider : BaseFilterProvider
    {
        public static readonly IFilterProvider Instance = new BenchmarkFilterProvider();

        private BenchmarkFilterProvider() : base(GetDictionary())
        {
        }

        private static Dictionary<string, IFilter> GetDictionary()
        {
            var ascii85 = new Ascii85Filter();
            var asciiHex = new AsciiHexDecodeFilter();
            var ccitt = new CcittFaxDecodeFilter();
            var dct = new DctDecodeFilter();
            var flate = new FlateFilter();
            var jbig2 = new PdfboxJbig2DecodeFilter();
            var jpx = new JpxDecodeFilter();
            var runLength = new RunLengthFilter();
            var lzw = new LzwFilter();

            return new Dictionary<string, IFilter>
            {
                { NameToken.Ascii85Decode.Data, ascii85 },
                { NameToken.Ascii85DecodeAbbreviation.Data, ascii85 },
                { NameToken.AsciiHexDecode.Data, asciiHex },
                { NameToken.AsciiHexDecodeAbbreviation.Data, asciiHex },
                { NameToken.CcittfaxDecode.Data, ccitt },
                { NameToken.CcittfaxDecodeAbbreviation.Data, ccitt },
                { NameToken.DctDecode.Data, dct },
                { NameToken.DctDecodeAbbreviation.Data, dct },
                { NameToken.FlateDecode.Data, flate },
                { NameToken.FlateDecodeAbbreviation.Data, flate },
                { NameToken.Jbig2Decode.Data, jbig2 },
                { NameToken.JpxDecode.Data, jpx },
                { NameToken.RunLengthDecode.Data, runLength },
                { NameToken.RunLengthDecodeAbbreviation.Data, runLength },
                { NameToken.LzwDecode.Data, lzw },
                { NameToken.LzwDecodeAbbreviation.Data, lzw }
            };
        }
    }
}
