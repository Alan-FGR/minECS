


static class RegistryGenerator
{
    public static object GenerateNew<T>() { return new object(); }
}

partial class Program
{
    static void Main(string[] args)
    {
        var registry = RegistryGenerator.GenerateNew<SomeIdentifier>();
        
        HelloFrom("Generated Code");
    }

    static partial void HelloFrom(string name);
}