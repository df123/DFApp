using DFApp.Web.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DFApp.Web.Tests.Infrastructure;

/// <summary>
/// 排序参数净化器单元测试
/// </summary>
public class SortingSanitizerTests
{
    /// <summary>
    /// 测试用实体
    /// </summary>
    private sealed class TestEntity
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public TestEntity? Child { get; set; }
    }

    [Fact]
    public void Sanitize_LegalFieldWithDirection_ShouldKeep()
    {
        var result = SortingSanitizer.Sanitize<TestEntity>("creationTime desc", "Id asc");
        result.Should().Be("CreationTime desc");
    }

    [Fact]
    public void Sanitize_LegalFieldWithoutDirection_ShouldDefaultAsc()
    {
        var result = SortingSanitizer.Sanitize<TestEntity>("Name", "Id asc");
        result.Should().Be("Name asc");
    }

    [Theory]
    [InlineData("Id; DROP TABLE Users")]
    [InlineData("Id asc, (SELECT CASE WHEN (1=1) THEN Id ELSE Name END)")]
    [InlineData("NonExistsField asc")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("Id ascending extra tokens")]
    public void Sanitize_IllegalInput_ShouldFallbackToDefault(string? sorting)
    {
        var result = SortingSanitizer.Sanitize<TestEntity>(sorting, "Id asc");
        result.Should().Be("Id asc");
    }

    [Fact]
    public void Sanitize_ComplexProperty_ShouldNotBeAllowed()
    {
        // 导航属性不允许参与排序
        var result = SortingSanitizer.Sanitize<TestEntity>("Child asc", "Id asc");
        result.Should().Be("Id asc");
    }

    [Fact]
    public void Sanitize_DirectionCaseInsensitive_ShouldNormalize()
    {
        var result = SortingSanitizer.Sanitize<TestEntity>("Name DESC", "Id asc");
        result.Should().Be("Name desc");
    }

    [Fact]
    public void Sanitize_IllegalDirection_ShouldFallback()
    {
        var result = SortingSanitizer.Sanitize<TestEntity>("Name sideways", "Id asc");
        result.Should().Be("Id asc");
    }
}
