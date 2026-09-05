using BenchmarkDotNet.Attributes;
using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

/// <summary>
/// Benchmarks <see cref="PdfboxJbig2DecodeFilter"/> directly against raw JBIG2 streams taken from
/// UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Tests, bypassing PDF parsing and PNG encoding so each
/// case measures only the JBIG2 decode itself. Much faster per case than
/// <see cref="DecodeJbig2Benchmarks"/>, which exercises the full PdfDocument -&gt; image -&gt; PNG path.
/// </summary>
[Config(typeof(NuGetPackageConfig))]
[MemoryDiagnoser(displayGenColumns: false)]
public class DecodeJbig2CoreBenchmarks
{
    private static readonly DictionaryToken NoGlobalsDictionary = new DictionaryToken(new Dictionary<NameToken, IToken>
    {
        { NameToken.Filter, NameToken.Jbig2Decode }
    });

    private byte[] sampledataPage1 = null!;
    private byte[] num003 = null!;
    private byte[] amb2 = null!;
    private byte[] num042_11 = null!;
    private byte[] unitizedPageIi = null!;
    private byte[] imgRefsGlobals = null!;
    private DictionaryToken imgRefsGlobalsDictionary = null!;
    private byte[] github21 = null!;
    private DictionaryToken github21Dictionary = null!;

    [GlobalSetup]
    public void Setup()
    {
        sampledataPage1 = File.ReadAllBytes("Assets/sampledata_page1.jb2");
        num003 = File.ReadAllBytes("Assets/003.jb2");
        amb2 = File.ReadAllBytes("Assets/amb_2.jb2");
        num042_11 = File.ReadAllBytes("Assets/042_11.jb2");
        unitizedPageIi = File.ReadAllBytes("Assets/unitized_page_ii.jb2");

        imgRefsGlobals = File.ReadAllBytes("Assets/img-refs-globals.jb2");
        imgRefsGlobalsDictionary = WithGlobalsDictionary(File.ReadAllBytes("Assets/globals.jb2"));

        github21 = File.ReadAllBytes("Assets/github-21.jb2");
        github21Dictionary = WithGlobalsDictionary(File.ReadAllBytes("Assets/github-21.glob"));
    }

    private static DictionaryToken WithGlobalsDictionary(byte[] globalsBytes)
    {
        return new DictionaryToken(new Dictionary<NameToken, IToken>
        {
            { NameToken.Filter, NameToken.Jbig2Decode },
            {
                NameToken.DecodeParms, new DictionaryToken(new Dictionary<NameToken, IToken>
                {
                    { NameToken.Jbig2Globals, new StreamToken(new DictionaryToken(new Dictionary<NameToken, IToken>()), globalsBytes) }
                })
            }
        });
    }

    [Benchmark]
    public Memory<byte> sampledata_page1()
    {
        return new PdfboxJbig2DecodeFilter().Decode(sampledataPage1, NoGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> _003()
    {
        return new PdfboxJbig2DecodeFilter().Decode(num003, NoGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> amb_2()
    {
        return new PdfboxJbig2DecodeFilter().Decode(amb2, NoGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> _042_11()
    {
        return new PdfboxJbig2DecodeFilter().Decode(num042_11, NoGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> unitized_page_ii()
    {
        return new PdfboxJbig2DecodeFilter().Decode(unitizedPageIi, NoGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> img_refs_globals()
    {
        return new PdfboxJbig2DecodeFilter().Decode(imgRefsGlobals, imgRefsGlobalsDictionary, NullFilterProvider.Instance, 0);
    }

    [Benchmark]
    public Memory<byte> github_21()
    {
        return new PdfboxJbig2DecodeFilter().Decode(github21, github21Dictionary, NullFilterProvider.Instance, 0);
    }
}
