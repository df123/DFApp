using DFApp.Web.Domain;
using Xunit;

namespace DFApp.Web.Tests.Domain;

/// <summary>
/// 实体基类单元测试
/// </summary>
public class EntityBaseTests
{
    /// <summary>
    /// 测试整数主键实体初始化
    /// </summary>
    [Fact]
    public void EntityBase_WithIntKey_ShouldInitializeWithId()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(0);
    }

    /// <summary>
    /// 测试设置整数主键
    /// </summary>
    [Fact]
    public void EntityBase_WithIntKey_ShouldSetId()
    {
        // Arrange
        var entity = new TestEntity();
        var expectedId = 123;

        // Act
        entity.Id = expectedId;

        // Assert
        entity.Id.Should().Be(expectedId);
    }

    /// <summary>
    /// 测试GUID主键实体初始化
    /// </summary>
    [Fact]
    public void EntityBase_WithGuidKey_ShouldInitializeWithEmptyGuid()
    {
        // Arrange & Act
        var entity = new TestEntityWithGuid();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
    }

    /// <summary>
    /// 测试设置GUID主键
    /// </summary>
    [Fact]
    public void EntityBase_WithGuidKey_ShouldSetId()
    {
        // Arrange
        var entity = new TestEntityWithGuid();
        var expectedId = Guid.NewGuid();

        // Act
        entity.Id = expectedId;

        // Assert
        entity.Id.Should().Be(expectedId);
    }

    /// <summary>
    /// 测试实体相等性（相同ID）
    /// </summary>
    [Fact]
    public void EntityBase_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 1 };

        // Assert
        entity1.Should().BeEquivalentTo(entity2, options => options
            .Including(x => x.Id));
    }

    /// <summary>
    /// 测试实体不等性（不同ID）
    /// </summary>
    [Fact]
    public void EntityBase_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };

        // Assert
        entity1.Should().NotBeEquivalentTo(entity2, options => options
            .Including(x => x.Id));
    }

    /// <summary>
    /// 测试设置并发标记
    /// </summary>
    [Fact]
    public void EntityBase_ShouldSetConcurrencyStamp()
    {
        // Arrange
        var entity = new TestEntity();
        var expectedStamp = Guid.NewGuid().ToString();

        // Act
        entity.ConcurrencyStamp = expectedStamp;

        // Assert
        entity.ConcurrencyStamp.Should().Be(expectedStamp);
    }

    /// <summary>
    /// 测试并发标记初始化
    /// </summary>
    [Fact]
    public void EntityBase_ShouldInitializeWithEmptyConcurrencyStamp()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.ConcurrencyStamp.Should().BeNull();
    }
}

/// <summary>
/// 测试用实体类（整数主键）
/// </summary>
public class TestEntity : EntityBase<int>;

/// <summary>
/// 测试用实体类（GUID主键）
/// </summary>
public class TestEntityWithGuid : EntityBase<Guid>;
