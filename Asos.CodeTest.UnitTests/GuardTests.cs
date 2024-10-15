using Asos.CodeTest.Exceptions;
using Asos.CodeTest.Interfaces;
using NUnit.Framework;
using System;

namespace Asos.CodeTest.UnitTests;

[TestFixture]
public class GuardTests
{
    [Test]
    public void ThrowIfNull_ShouldReturnSameValue_WhenNotNull()
    {
        // Arrange
        var expectedValue = new object();

        // Act
        var result = Guard.ThrowIfNull(expectedValue);

        // Assert
        Assert.That(result, Is.EqualTo(expectedValue));

    }

    [Test]
    public void ThrowIfNull_ShouldThrowArgumentNullException_WhenNull()
    {
        // Arrange
        IAppSettings settings = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNull(settings));
        Assert.That(ex.ParamName, Is.EqualTo($"Parameter of type {typeof(IAppSettings).Name} is null"));
    }
}