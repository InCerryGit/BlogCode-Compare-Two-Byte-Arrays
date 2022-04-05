using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CompareByte;

var summary = BenchmarkRunner.Run<BenchmarkCompareMethod>();

[MemoryDiagnoser]
[HtmlExporter]
public class BenchmarkCompareMethod
{
    private static readonly byte[] XBytes = Enumerable.Range(0, 4096000).Select(c => (byte) c).ToArray();
    private static readonly byte[] YBytes = Enumerable.Range(0, 4096000).Select(c => (byte) c).ToArray();

    public BenchmarkCompareMethod()
    {
        XBytes[4095999] = 1;
        YBytes[4095999] = 2;
    }

    [Benchmark(Baseline = true)]
    public void ForCompare()
    {
        BytesCompare.ForCompare(XBytes, YBytes);
    }

    [Benchmark]
    public void UlongCompare()
    {
        BytesCompare.UlongCompare(XBytes, YBytes);
    }
    
    [Benchmark]
    public void MemcmpCompare()
    {
        BytesCompare.MemcmpCompare(XBytes, YBytes);
    }
    
    [Benchmark]
    public void Sse2Compare()
    {
        BytesCompare.Sse2Compare(XBytes, YBytes);
    }
    
    [Benchmark]
    public void Avx2Compare()
    {
        BytesCompare.Avx2Compare(XBytes, YBytes);
    }
    
    [Benchmark]
    public void SequenceCompare()
    {
        BytesCompare.SequenceCompare(XBytes, YBytes);
    }
    
}