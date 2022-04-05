using Xunit;

namespace CompareByte;

public class BytesCompareTests
{
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void For_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.ForCompare);
    }
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Ulong_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.UlongCompare);
    }
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Sse2_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.Sse2Compare);
    }
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Memcmp_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.MemcmpCompare);
    }
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Sequence_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.SequenceCompare);
    }
    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Avx2_Compare_Result_Should_Be_Equals_Except(byte[] x, byte[] y, bool except)
    {
        CompareInternal(x,y,except, BytesCompare.Avx2Compare);
    }

    private static void CompareInternal(byte[] x, byte[] y, bool except, Func<byte[],byte[],bool> compareMethod)
    {
        var result = compareMethod(x, y);
        Assert.Equal(except,result);
    }

    static IEnumerable<object[]> GetTestData()
    {
        yield return new object[]
        {
            null!,
            null!,
            true
        };

        yield return new object []
        {
            Enumerable.Range(0, 4096).Select(c => (byte) c).ToArray(),
            Enumerable.Range(0, 4096).Select(c => (byte) c).ToArray(),
            true
        };
        
        yield return new object []
        {
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            true
        };      
        
        yield return new object []
        {
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            Enumerable.Range(0, 42).Select(c => (byte) c).ToArray(),
            false
        };
        
        yield return new object []
        {
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            null!,
            false
        };
        
        yield return new object []
        {
            null!,
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            false
        };
        
        
        yield return new object []
        {
            Enumerable.Range(0, 4096).Select(c => (byte) c).ToArray(),
            Enumerable.Range(0, 4096).Select(c => c == 1248 ? (byte)0 : (byte) c).ToArray(),
            false
        };
        
        yield return new object []
        {
            Enumerable.Range(0, 43).Select(c => (byte) c).ToArray(),
            Enumerable.Range(0, 43).Select(c => c > 20 ? (byte)5 : (byte) c).ToArray(),
            false
        };
    }
}