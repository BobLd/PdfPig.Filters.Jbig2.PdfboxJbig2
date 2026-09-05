using UglyToad.PdfPig.Tokenization.Scanner;
using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

/// <summary>
/// No-op filter provider for benchmarking <see cref="PdfboxJbig2DecodeFilter"/> directly against a raw
/// JBIG2 stream (no surrounding PDF). The globals stream referenced by DecodeParms carries no filters
/// of its own, so nothing here is ever actually invoked.
/// </summary>
internal sealed class NullFilterProvider : ILookupFilterProvider
{
    public static readonly NullFilterProvider Instance = new NullFilterProvider();

    public IReadOnlyList<IFilter> GetFilters(DictionaryToken dictionary) => [];

    public IReadOnlyList<IFilter> GetNamedFilters(IReadOnlyList<NameToken> names) => [];

    public IReadOnlyList<IFilter> GetAllFilters() => [];

    public IReadOnlyList<IFilter> GetFilters(DictionaryToken dictionary, IPdfTokenScanner scanner) => [];
}
