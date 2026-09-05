using BenchmarkDotNet.Attributes;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

[Config(typeof(NuGetPackageConfig))]
[MemoryDiagnoser(displayGenColumns: false)]
public class DecodeJbig2Benchmarks
{
    [Benchmark]
    public IReadOnlyList<byte[]> MOZILLA_8932_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/MOZILLA-8932-0.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> GHOSTSCRIPT_690360_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/GHOSTSCRIPT-690360-0.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> GHOSTSCRIPT_688477_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/GHOSTSCRIPT-688477-0.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> jury_summons_guide_eng()
    {
        return Helpers.ExtractAllJbig2Images("Documents/jury-summons-guide-eng.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> GHOSTSCRIPT_690151_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/GHOSTSCRIPT-690151-0.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> ag46()
    {
        return Helpers.ExtractAllJbig2Images("Documents/ag46.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> GHOSTSCRIPT_687530_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/GHOSTSCRIPT-687530-0.pdf");
    }

    [Benchmark]
    public IReadOnlyList<byte[]> MOZILLA_11518_0()
    {
        return Helpers.ExtractAllJbig2Images("Documents/MOZILLA-11518-0.pdf");
    }
}
