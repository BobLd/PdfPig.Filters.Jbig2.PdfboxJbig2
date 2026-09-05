using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Benchmarks;

internal class NuGetPackageConfig : ManualConfig
{
    public NuGetPackageConfig()
    {
        var localJob = Job.Default
            .WithMsBuildArguments("/p:Jbig2FilterVersion=Local")
            .WithId("Local");

        var latestJob = Job.Default
            .WithMsBuildArguments("/p:Jbig2FilterVersion=Latest")
            .WithId("Latest")
            .AsBaseline();

        AddJob(localJob.WithRuntime(CoreRuntime.Core80));
        AddJob(latestJob.WithRuntime(CoreRuntime.Core80));
    }
}
