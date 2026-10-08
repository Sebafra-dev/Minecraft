using Minecraft.Source;
using Minecraft.Source.Structures;

namespace Minecraft.Tests;

public class BasicTests
{
    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(-5, 3, 1)]
    [InlineData(5, 5, 0)]
    [InlineData(-5, 5, 0)]
    [InlineData(0, 3, 0)]
    [InlineData(7, 2, 1)]
    [InlineData(-7, 2, 1)]
    public void ModTest(int num, int divisor, int expected)
    {
        var result = num.Mod(divisor);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetHashOnPositionTest()
    {
        var hash1 = Utils.GetHashOnPosition(0, 0);
        var hash2 = Utils.GetHashOnPosition(1, 0);
        var hash3 = Utils.GetHashOnPosition(0, 1);
        var hash4 = Utils.GetHashOnPosition(1, 1);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash1, hash4);
        Assert.NotEqual(hash2, hash3);
        Assert.NotEqual(hash2, hash4);
        Assert.NotEqual(hash3, hash4);
    }

    [Theory]
    [InlineData(
        0, 0, 
        0, 1, 
        0, 1)]
    [InlineData(
        0, 0,
        0, -1,
        0, -1)]
    [InlineData(
        0, 0,
        1, 0,
        1, 0)]
    [InlineData(
        1, 1,
        2, 2,
        3, 3)]
    public void ChunkPositionAddOperationTest(
     int x1, int y1,
     int x2, int y2,
     int expectedX, int expectedY)
    {
        var chunkPosition1 = new ChunkPosition(x1, y1);
        var chunkPosition2 = new ChunkPosition(x2, y2);
        var expected = new ChunkPosition(expectedX, expectedY);

        var result = chunkPosition1 + chunkPosition2;

        Assert.Equal(expected, result);
    }
}
