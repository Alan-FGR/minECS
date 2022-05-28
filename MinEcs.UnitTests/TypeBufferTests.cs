namespace MinEcs.UnitTests;

public unsafe class TypeBufferTests : IDisposable
{
    //public class ClassDataProvider : IEnumerable<object[]>
    //{
    //    public IEnumerator<object[]> GetEnumerator()
    //    {
    //        yield return new object[] {(byte) 0};
    //        yield return new object[] {(nuint) 0};
    //    }
    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    //}
    
    struct EmptyStruct { }

    struct PrimitivesStruct
    {
        public byte b;
        public nuint q, w, e, r;
        public ulong t, y, u, i;
    }

    struct LargeStruct
    {
        public PrimitivesStruct q, w, e, r, t, y, u, i;
    }

    record TestDataStructure(object typeHolder, object elementsToTest);

    static TestDataStructure[] AllTestData => new[]
    {
        new TestDataStructure((byte)0, Enumerable.Range(1,4).Select(x => (byte)x).ToList()),
        new TestDataStructure((uint)0, Enumerable.Range(1,4).Select(x => (uint)x).ToList()),
        new TestDataStructure(new EmptyStruct(), Enumerable.Range(1,4).Select(x => new EmptyStruct()).ToList()),
        new TestDataStructure(new PrimitivesStruct(), Enumerable.Range(1,400).Select(x => new PrimitivesStruct(){i = (ulong) x}).ToList()),
        new TestDataStructure(new LargeStruct(), Enumerable.Range(1,400).Select(x => new LargeStruct(){i = new PrimitivesStruct(){i = (ulong) x}}).ToList()),
    };


    TypeBuffer? typeBuffer;

    public TypeBufferTests()
    {
        // xUnit does not support ctor generic piping thus we use the Construction test
    }

    public static IEnumerable<object[]> ConstructionData => AllTestData.Select(x => new[] { x.typeHolder, ((IList)x.elementsToTest).Count });
    [Theory]
    [MemberData(nameof(ConstructionData))]
    public void Construction<T>(T _, int count) where T : unmanaged
    {
        typeBuffer?.Dispose();
        Assert.Throws<InvalidOperationException>(() => new TypeBuffer());
        var typeSize = (nuint)Marshal.SizeOf<T>();
        var validBuffer = new TypeBuffer(typeSize, (uint)count);
        Assert.True(validBuffer.TypeSize == typeSize);
        typeBuffer = validBuffer;
    }

    public static IEnumerable<object[]> SetData => AllTestData.Select(x => new[] { x.typeHolder, x.elementsToTest });
    [Theory]
    [MemberData(nameof(SetData))]
    public void Set<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Construction(typeForInference, asList.Count);
        for (var i = 0; i < asList.Count; i++)
        {
            T element = asList[i];
            typeBuffer?.Set((nuint)i, element);
            var allocatedMemoryStart = typeBuffer.GetAddress<T>();
            var destinationPosition = allocatedMemoryStart + i;
            T valueInDestination = *destinationPosition;
            Assert.Equal(element, valueInDestination);
        }
    }

    [Theory]
    [MemberData(nameof(SetData))]
    public void Get<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Set(typeForInference, asList);
        for (var i = 0; i < asList.Count; i++)
        {
            T element = asList[i];
            ref var retrieved = ref typeBuffer.Get<T>((nuint)i);
            Assert.Equal(element, retrieved);
        }
    }

    [Theory]
    [MemberData(nameof(SetData))]
    public void ToList<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Set(typeForInference, asList);
        var toList = typeBuffer.ToList<T>((nuint)asList.Count);
        Assert.Equal(asList, toList);
    }

    [Theory]
    [MemberData(nameof(SetData))]
    public void ResizeDown<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Set(typeForInference, asList);
        nuint newSize = (nuint)(asList.Count / 2);
        typeBuffer.Resize(newSize, newSize);
        Assert.Equal(asList.Take((int)newSize), typeBuffer.ToList<T>(newSize));
        newSize = 0;
        typeBuffer.Resize(newSize, newSize);
        Assert.Equal(asList.Take((int)newSize), typeBuffer.ToList<T>(newSize));
    }

    [Theory]
    [MemberData(nameof(SetData))]
    public void ResizeUp<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Set(typeForInference, asList);
        nuint originalSize = (nuint)asList.Count;
        typeBuffer.Resize(originalSize * 2, originalSize);
        Assert.Equal(asList.Take((int)originalSize), typeBuffer.ToList<T>(originalSize));
    }

    [Theory]
    [MemberData(nameof(SetData))]
    public void Copy<T>(T typeForInference, IEnumerable<T> elementsToTest) where T : unmanaged
    {
        var asList = elementsToTest.ToList();
        Set(typeForInference, asList);
        for (var i = 0; i < asList.Count; i++)
        {
            typeBuffer.Copy<T>((nuint)i, (nuint)(asList.Count - 1));
            ref var retrieved = ref typeBuffer.Get<T>((nuint)i);
            Assert.Equal(asList.Last(), retrieved);
        }
        Assert.All(typeBuffer.ToList<T>(asList.Count), e => Assert.Equal(asList.Last(), e));
    }

    public void Dispose()
    {
        typeBuffer?.Dispose();
        Assert.True(typeBuffer.GetAddress<byte>() == null);
    }
}