using System.Diagnostics;
using BenchmarkDotNet.Running;
using UglyToad.PdfPig.Filters;
using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--profile-docs")
        {
            ProfileDocuments();
            return;
        }

        BenchmarkRunner.Run<DecodeJbig2Benchmarks>(args: args);
    }

    /// <summary>
    /// Splits the wall clock time of the document level benchmark into the JBIG2 decode itself versus
    /// everything else it does (PDF parsing, image extraction and PNG encoding).
    /// </summary>
    private static void ProfileDocuments()
    {
        string[] documents =
        [
            "Documents/MOZILLA-8932-0.pdf",
            "Documents/GHOSTSCRIPT-690360-0.pdf",
            "Documents/GHOSTSCRIPT-688477-0.pdf",
            "Documents/jury-summons-guide-eng.pdf",
            "Documents/GHOSTSCRIPT-690151-0.pdf",
            "Documents/ag46.pdf",
            "Documents/GHOSTSCRIPT-687530-0.pdf",
            "Documents/MOZILLA-11518-0.pdf",
        ];

        var timing = new TimingJbig2Filter();
        var options = new ParsingOptions
        {
            UseLenientParsing = true,
            SkipMissingFonts = true,
            FilterProvider = new ProfilingFilterProvider(timing)
        };

        var asm = typeof(PdfboxJbig2DecodeFilter).Assembly;
        var type = asm.GetType("UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2.Jbig2Bitmaps")!;
        string[] names = ["InstrUnshiftedBytes", "InstrByPixelPixels", "InstrInvertedBytes", "InstrReadBitCalls"];
        var fields = names
            .Select(n => type.GetField(n, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!)
            .ToArray();

        Console.WriteLine($"{"document",-30}{"total ms",10}{"jbig2 ms",10}{"jbig2 %",9}{"unshiftB",11}{"byPixelPx",12}{"invertB",11}{"readBit",10}");

        foreach (var path in documents)
        {
            // Warm up so JIT and file cache costs are not attributed to the measured pass.
            RunOnce(path, options, out _, out _);

            timing.Reset();
            foreach (var f in fields)
            {
                f.SetValue(null, 0L);
            }

            var sw = Stopwatch.StartNew();
            RunOnce(path, options, out _, out _);
            sw.Stop();

            double totalMs = sw.Elapsed.TotalMilliseconds;
            double jbigMs = timing.Elapsed.TotalMilliseconds;
            double pct = totalMs > 0 ? jbigMs / totalMs * 100 : 0;
            var v = fields.Select(f => (long)f.GetValue(null)!).ToArray();

            Console.WriteLine($"{Path.GetFileName(path),-30}{totalMs,10:F1}{jbigMs,10:F1}{pct,9:F1}{v[0],11:N0}{v[1],12:N0}{v[2],11:N0}{v[3],10:N0}");
        }
    }

    private static void RunOnce(string path, ParsingOptions options, out int images, out long pngBytes)
    {
        images = 0;
        pngBytes = 0;

        using var document = PdfDocument.Open(path, options);
        for (int p = 1; p <= document.NumberOfPages; p++)
        {
            foreach (var image in document.GetPage(p).GetImages())
            {
                if (image.TryGetPng(out var png))
                {
                    images++;
                    pngBytes += png.Length;
                }
            }
        }
    }

    /// <summary>
    /// Wraps the real filter and accumulates how long the JBIG2 decode itself takes.
    /// </summary>
    private sealed class TimingJbig2Filter : IFilter
    {
        private readonly PdfboxJbig2DecodeFilter inner = new PdfboxJbig2DecodeFilter();
        private readonly Stopwatch stopwatch = new Stopwatch();

        public bool IsSupported => inner.IsSupported;

        public TimeSpan Elapsed => stopwatch.Elapsed;

        public void Reset() => stopwatch.Reset();

        public Memory<byte> Decode(Memory<byte> input, DictionaryToken streamDictionary,
            IFilterProvider filterProvider, int filterIndex)
        {
            stopwatch.Start();
            try
            {
                return inner.Decode(input, streamDictionary, filterProvider, filterIndex);
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }

    private sealed class ProfilingFilterProvider : BaseFilterProvider
    {
        public ProfilingFilterProvider(IFilter jbig2) : base(GetDictionary(jbig2))
        {
        }

        private static Dictionary<string, IFilter> GetDictionary(IFilter jbig2)
        {
            var ascii85 = new Ascii85Filter();
            var asciiHex = new AsciiHexDecodeFilter();
            var ccitt = new CcittFaxDecodeFilter();
            var dct = new DctDecodeFilter();
            var flate = new FlateFilter();
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
                { NameToken.LzwDecode.Data, lzw }
            };
        }
    }
}
