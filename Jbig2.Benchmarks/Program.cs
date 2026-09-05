using BenchmarkDotNet.Running;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<DecodeJbig2CoreBenchmarks>(args: args);
    }
}
