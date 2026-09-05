# UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2

Port to C# and PdfPig of the Java ImageIO plugin for the JBIG2 image format (see https://github.com/apache/pdfbox-jbig2). This port was last compared against upstream [apache/pdfbox-jbig2](https://github.com/apache/pdfbox-jbig2) at commit [`57b5f93`](https://github.com/apache/pdfbox-jbig2/commit/57b5f93ba1c23767df2828a3c2682143995757ad) (2026-06-29).

Original port done by @kasperdaff as part of a PR for PdfPig, see https://github.com/UglyToad/PdfPig/pull/338 and then https://github.com/UglyToad/PdfPig/pull/631

## Other filters
- DCT filter available here: https://github.com/BobLd/UglyToad.PdfPig.Filters.Dct.JpegLibrary
- JPX filter available here: https://github.com/BobLd/UglyToad.PdfPig.Filters.Jpx.OpenJpegDotNet

## Usage
```csharp
// Create your filter provider
public sealed class MyFilterProvider : BaseFilterProvider
{
    /// <summary>
    /// The single instance of this provider.
    /// </summary>
    public static readonly IFilterProvider Instance = new MyFilterProvider();

    /// <inheritdoc/>
    private MyFilterProvider() : base(GetDictionary())
    {
    }

    private static Dictionary<string, IFilter> GetDictionary()
    {
        var ascii85 = new Ascii85Filter();
        var asciiHex = new AsciiHexDecodeFilter();
        var ccitt = new CcittFaxDecodeFilter();
        var dct = new DctDecodeFilter(); // new filter
        var flate = new FlateFilter();
        var jbig2 = new PdfboxJbig2DecodeFilter(); // new filter
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
            { NameToken.DctDecode.Data, dct }, // new filter
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



var parsingOption = new ParsingOptions()
{
	UseLenientParsing = true,
	SkipMissingFonts = true,
	FilterProvider = MyFilterProvider.Instance
};

using (var doc = PdfDocument.Open("test.pdf", parsingOption))
{
	int i = 0;
	foreach (var page in doc.GetPages())
	{
		foreach (var pdfImage in page.GetImages())
		{
			Assert.True(pdfImage.TryGetPng(out var bytes));

			File.WriteAllBytes($"image_{i++}.png", bytes);
		}
	}
}
```
