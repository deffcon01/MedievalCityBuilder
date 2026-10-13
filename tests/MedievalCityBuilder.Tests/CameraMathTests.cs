using MedievalCityBuilder.Core;
using Xunit;

namespace MedievalCityBuilder.Tests;

public class CameraMathTests
{
    [Fact]
    public void ClampZoom_BelowMin_ReturnsMin()
    {
        float result = CameraMath.ClampZoom(5f, 10f, 50f);
        Assert.Equal(10f, result);
    }

    [Fact]
    public void ClampZoom_AboveMax_ReturnsMax()
    {
        float result = CameraMath.ClampZoom(100f, 10f, 50f);
        Assert.Equal(50f, result);
    }

    [Fact]
    public void ClampZoom_InsideRange_ReturnsValue()
    {
        float result = CameraMath.ClampZoom(25f, 10f, 50f);
        Assert.Equal(25f, result);
    }

    [Fact]
    public void ClampToBounds_Inside_ReturnsSame()
    {
        var (x, z) = CameraMath.ClampToBounds(50f, 50f, 0f, 100f, 0f, 100f);
        Assert.Equal(50f, x);
        Assert.Equal(50f, z);
    }

    [Fact]
    public void ClampToBounds_AtMinX_ReturnsMinX()
    {
        var (x, z) = CameraMath.ClampToBounds(-10f, 50f, 0f, 100f, 0f, 100f);
        Assert.Equal(0f, x);
        Assert.Equal(50f, z);
    }

    [Fact]
    public void ClampToBounds_AtMaxX_ReturnsMaxX()
    {
        var (x, z) = CameraMath.ClampToBounds(150f, 50f, 0f, 100f, 0f, 100f);
        Assert.Equal(100f, x);
        Assert.Equal(50f, z);
    }

    [Fact]
    public void ClampToBounds_AtMinZ_ReturnsMinZ()
    {
        var (x, z) = CameraMath.ClampToBounds(50f, -10f, 0f, 100f, 0f, 100f);
        Assert.Equal(50f, x);
        Assert.Equal(0f, z);
    }

    [Fact]
    public void ClampToBounds_AtMaxZ_ReturnsMaxZ()
    {
        var (x, z) = CameraMath.ClampToBounds(50f, 150f, 0f, 100f, 0f, 100f);
        Assert.Equal(50f, x);
        Assert.Equal(100f, z);
    }

    [Fact]
    public void ClampToBounds_OutsideBothAxes_ReturnsClamped()
    {
        var (x, z) = CameraMath.ClampToBounds(-10f, 150f, 0f, 100f, 0f, 100f);
        Assert.Equal(0f, x);
        Assert.Equal(100f, z);
    }

    [Fact]
    public void WrapYaw_Zero_ReturnsZero()
    {
        float result = CameraMath.WrapYaw(0f);
        Assert.Equal(0f, result);
    }

    [Fact]
    public void WrapYaw_180_Returns180()
    {
        float result = CameraMath.WrapYaw(180f);
        Assert.Equal(180f, result);
    }

    [Fact]
    public void WrapYaw_Negative180_Returns180()
    {
        float result = CameraMath.WrapYaw(-180f);
        Assert.Equal(180f, result);
    }

    [Fact]
    public void WrapYaw_190_ReturnsNegative170()
    {
        float result = CameraMath.WrapYaw(190f);
        Assert.Equal(-170f, result);
    }

    [Fact]
    public void WrapYaw_Negative190_Returns170()
    {
        float result = CameraMath.WrapYaw(-190f);
        Assert.Equal(170f, result);
    }

    [Fact]
    public void WrapYaw_360_ReturnsZero()
    {
        float result = CameraMath.WrapYaw(360f);
        Assert.Equal(0f, result);
    }

    [Fact]
    public void WrapYaw_540_Returns180()
    {
        float result = CameraMath.WrapYaw(540f);
        Assert.Equal(180f, result);
    }
}