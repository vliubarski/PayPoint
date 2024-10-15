using System;

namespace Asos.CodeTest.Exceptions;

public static class Guard
{
    public static T ThrowIfNull<T>(T value) => value ?? throw new ArgumentNullException($"Parameter of type {typeof(T).Name} is null");
}
