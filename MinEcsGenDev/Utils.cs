using System;
using System.Runtime.CompilerServices;

public static class Utils
{
    public static Exception InvalidCtor([CallerMemberName] string typeName = "") =>
        new InvalidOperationException($"{typeName} constructor is invalid");
}