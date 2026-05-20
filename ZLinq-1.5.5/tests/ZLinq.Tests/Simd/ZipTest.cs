#if NET8_0_OR_GREATER

using System.Numerics;
using System.Runtime.InteropServices;
using ZLinq.Simd;

namespace ZLinq.Tests.Simd;

public class ZipTest
{
    #region ZipVectorizable Tests

    [Fact]
    public void ZipVectorizable_BasicFunctionality()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var second = new[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        // Act
        var result = first.AsVectorizable().Zip(
            second, 
            (v1, v2) => v1 + v2, 
            (x, y) => x + y
        ).ToArray();

        // Assert
        var expected = new[] { 11, 22, 33, 44, 55, 66, 77, 88 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void ZipVectorizable_DifferentSizes_UsesSmallerSize()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30 };

        // Act
        var result = first.AsVectorizable().Zip(
            second, 
            (v1, v2) => v1 + v2, 
            (x, y) => x + y
        ).ToArray();

        // Assert
        var expected = new[] { 11, 22, 33 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void ZipVectorizable_EmptySource_ReturnsEmpty()
    {
        // Arrange
        var first = Array.Empty<int>();
        var second = new[] { 10, 20, 30 };

        // Act
        var result = first.AsVectorizable().Zip(
            second, 
            (v1, v2) => v1 + v2, 
            (x, y) => x + y
        ).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ZipVectorizable_EmptySecond_ReturnsEmpty()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = Array.Empty<int>();

        // Act
        var result = first.AsVectorizable().Zip(
            second, 
            (v1, v2) => v1 + v2, 
            (x, y) => x + y
        ).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ZipVectorizable_DifferentSelectors_WorkCorrectly()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var second = new[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        // Act - multiplication instead of addition
        var result = first.AsVectorizable().Zip(
            second, 
            (v1, v2) => v1 * v2, 
            (x, y) => x * y
        ).ToArray();

        // Assert
        var expected = new[] { 10, 40, 90, 160, 250, 360, 490, 640 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void ZipVectorizable_DifferentResultType()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var second = new[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        // Act - convert to float during operation
        var result = first.AsVectorizable().Zip<float>(
            second, 
            (v1, v2) => Vector.ConvertToSingle(v1) / Vector.ConvertToSingle(v2), 
            (x, y) => (float)x / y
        ).ToArray();

        // Assert
        var expected = new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
        for (int i = 0; i < expected.Length; i++)
        {
            result[i].ShouldBeInRange(expected[i] - 0.0001f, expected[i] + 0.0001f);
        }
    }

    [Fact]
    public void ZipVectorizable_CopyTo_ThrowsOnTooSmallDestination()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30, 40, 50 };

        Should.Throw<ArgumentException>(() =>
        {
            var zipVectorizable = first.AsVectorizable().Zip(
                second,
                (v1, v2) => v1 + v2,
                (x, y) => x + y
            );

            // Act & Assert
            var destination = new int[3]; // Too small
            zipVectorizable.CopyTo(destination);
        });
    }

    [Fact]
    public void ZipVectorizable_CopyTo_FillsDestination()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30, 40, 50 };
        var zipVectorizable = first.AsVectorizable().Zip(
            second,
            (v1, v2) => v1 + v2,
            (x, y) => x + y
        );

        // Act
        var destination = new int[5];
        zipVectorizable.CopyTo(destination);

        // Assert
        var expected = new[] { 11, 22, 33, 44, 55 };
        destination.ShouldBe(expected);
    }

    [Fact]
    public void ZipVectorizable_SmallArray_NoSIMD()
    {
        // Arrange - Array smaller than Vector<T>.Count to test non-SIMD path
        var first = new[] { 1, 2 };
        var second = new[] { 10, 20 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            (v1, v2) => v1 + v2,
            (x, y) => x + y
        ).ToArray();

        // Assert
        var expected = new[] { 11, 22 };
        result.ShouldBe(expected);
    }

    #endregion

    #region Zip3Vectorizable Tests

    [Fact]
    public void Zip3Vectorizable_BasicFunctionality()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var second = new[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        var third = new[] { 100, 200, 300, 400, 500, 600, 700, 800 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        var expected = new[] { 111, 222, 333, 444, 555, 666, 777, 888 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void Zip3Vectorizable_DifferentSizes_UsesSmallerSize()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30, 40 };
        var third = new[] { 100, 200, 300 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        var expected = new[] { 111, 222, 333 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void Zip3Vectorizable_EmptySource_ReturnsEmpty()
    {
        // Arrange
        var first = Array.Empty<int>();
        var second = new[] { 10, 20, 30 };
        var third = new[] { 100, 200, 300 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Zip3Vectorizable_EmptySecond_ReturnsEmpty()
    {
        // Arrange
        var first = new[] { 1, 2, 3 };
        var second = Array.Empty<int>();
        var third = new[] { 100, 200, 300 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Zip3Vectorizable_EmptyThird_ReturnsEmpty()
    {
        // Arrange
        var first = new[] { 1, 2, 3 };
        var second = new[] { 10, 20, 30 };
        var third = Array.Empty<int>();

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Zip3Vectorizable_DifferentSelectors_WorkCorrectly()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 10, 20, 30, 40 };
        var third = new[] { 100, 200, 300, 400 };

        // Act - combine with multiplication
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 * v2 * v3,
            (x, y, z) => x * y * z
        ).ToArray();

        // Assert
        var expected = new[] { 1000, 8000, 27000, 64000 };
        result.ShouldBe(expected);
    }

    [Fact]
    public void Zip3Vectorizable_DifferentResultType()
    {
        // Arrange
        var first = new[] { 10, 20, 30, 40 };
        var second = new[] { 2, 4, 5, 8 };
        var third = new[] { 5, 5, 6, 4 };

        // Act - convert result to float
        var result = first.AsVectorizable().Zip<float>(
            second,
            third,
            (v1, v2, v3) => Vector.ConvertToSingle(v1) / (Vector.ConvertToSingle(v2) * Vector.ConvertToSingle(v3)),
            (x, y, z) => (float)x / (y * z)
        ).ToArray();

        // Assert
        var expected = new float[] { 10f/(2f*5f), 20f/(4f*5f), 30f/(5f*6f), 40f/(8f*4f) };
        for (int i = 0; i < expected.Length; i++)
        {
            result[i].ShouldBeInRange(expected[i] - 0.0001f, expected[i] + 0.0001f);
        }
    }

    [Fact]
    public void Zip3Vectorizable_CopyTo_ThrowsOnTooSmallDestination()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30, 40, 50 };
        var third = new[] { 100, 200, 300, 400, 500 };

        Should.Throw<ArgumentException>(() =>
        {
            var zipVectorizable = first.AsVectorizable().Zip(
                second,
                third,
                (v1, v2, v3) => v1 + v2 + v3,
                (x, y, z) => x + y + z
            );

            // Act & Assert
            var destination = new int[3]; // Too small
            zipVectorizable.CopyTo(destination);
        });
    }

    [Fact]
    public void Zip3Vectorizable_CopyTo_FillsDestination()
    {
        // Arrange
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10, 20, 30, 40, 50 };
        var third = new[] { 100, 200, 300, 400, 500 };
        var zipVectorizable = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        );

        // Act
        var destination = new int[5];
        zipVectorizable.CopyTo(destination);

        // Assert
        var expected = new[] { 111, 222, 333, 444, 555 };
        destination.ShouldBe(expected);
    }

    [Fact]
    public void Zip3Vectorizable_SmallArray_NoSIMD()
    {
        // Arrange - Array smaller than Vector<T>.Count to test non-SIMD path
        var first = new[] { 1, 2 };
        var second = new[] { 10, 20 };
        var third = new[] { 100, 200 };

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        var expected = new[] { 111, 222 };
        result.ShouldBe(expected);
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public void LargeArrays_PerformsProperly()
    {
        // Arrange
        const int size = 10000;
        var first = new int[size];
        var second = new int[size];
        var third = new int[size];

        for (int i = 0; i < size; i++)
        {
            first[i] = i;
            second[i] = i * 2;
            third[i] = i * 3;
        }

        // Act - 2-way zip
        var result1 = first.AsVectorizable().Zip(
            second,
            (v1, v2) => v1 + v2,
            (x, y) => x + y
        ).ToArray();

        // Assert
        for (int i = 0; i < size; i++)
        {
            result1[i].ShouldBe(first[i] + second[i]);
        }

        // Act - 3-way zip
        var result2 = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        for (int i = 0; i < size; i++)
        {
            result2[i].ShouldBe(first[i] + second[i] + third[i]);
        }
    }

    [Theory]
    [InlineData(0, 0)] // Empty arrays
    [InlineData(1, 1)] // Single element
    //[InlineData(Vector<int>.Count - 1, Vector<int>.Count - 1)] // Just below SIMD threshold
    //[InlineData(Vector<int>.Count, Vector<int>.Count)] // At SIMD threshold
    //[InlineData(Vector<int>.Count + 1, Vector<int>.Count + 1)] // Just above SIMD threshold
    //[InlineData(Vector<int>.Count * 2, Vector<int>.Count * 2)] // Multiple of SIMD threshold
    //[InlineData(Vector<int>.Count * 2 + 1, Vector<int>.Count * 2 + 1)] // Multiple of SIMD threshold plus 1
    public void ZipVectorizable_VariousSizes_WorksCorrectly(int size1, int size2)
    {
        // Arrange
        var first = new int[size1];
        var second = new int[size2];

        for (int i = 0; i < size1; i++)
            first[i] = i + 1;

        for (int i = 0; i < size2; i++)
            second[i] = (i + 1) * 10;

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            (v1, v2) => v1 + v2,
            (x, y) => x + y
        ).ToArray();

        // Assert
        var minSize = Math.Min(size1, size2);
        result.Length.ShouldBe(minSize);

        for (int i = 0; i < minSize; i++)
        {
            result[i].ShouldBe(first[i] + second[i]);
        }
    }

    [Theory]
    [InlineData(0, 0, 0)] // Empty arrays
    [InlineData(1, 1, 1)] // Single element
    //[InlineData(Vector<int>.Count - 1, Vector<int>.Count - 1, Vector<int>.Count - 1)] // Just below SIMD threshold
    //[InlineData(Vector<int>.Count, Vector<int>.Count, Vector<int>.Count)] // At SIMD threshold
    //[InlineData(Vector<int>.Count + 1, Vector<int>.Count + 1, Vector<int>.Count + 1)] // Just above SIMD threshold
    public void Zip3Vectorizable_VariousSizes_WorksCorrectly(int size1, int size2, int size3)
    {
        // Arrange
        var first = new int[size1];
        var second = new int[size2];
        var third = new int[size3];

        for (int i = 0; i < size1; i++)
            first[i] = i + 1;

        for (int i = 0; i < size2; i++)
            second[i] = (i + 1) * 10;

        for (int i = 0; i < size3; i++)
            third[i] = (i + 1) * 100;

        // Act
        var result = first.AsVectorizable().Zip(
            second,
            third,
            (v1, v2, v3) => v1 + v2 + v3,
            (x, y, z) => x + y + z
        ).ToArray();

        // Assert
        var minSize = Math.Min(Math.Min(size1, size2), size3);
        result.Length.ShouldBe(minSize);

        for (int i = 0; i < minSize; i++)
        {
            result[i].ShouldBe(first[i] + second[i] + third[i]);
        }
    }
    
    #endregion
}

#endif

