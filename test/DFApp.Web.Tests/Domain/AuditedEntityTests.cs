using DFApp.Web.Domain;
using Xunit;

namespace DFApp.Web.Tests.Domain;

/// <summary>
/// 审计实体单元测试
/// </summary>
public class AuditedEntityTests
{
    /// <summary>
    /// 测试整数主键审计实体初始化
    /// </summary>
    [Fact]
    public void AuditedEntity_WithIntKey_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var entity = new TestAuditedEntity();

        // Assert
        entity.Id.Should().Be(0);
        entity.CreationTime.Should().Be(default);
        entity.CreatorId.Should().BeNull();
        entity.LastModificationTime.Should().BeNull();
        entity.LastModifierId.Should().BeNull();
    }

    /// <summary>
    /// 测试GUID主键审计实体初始化
    /// </summary>
    [Fact]
    public void AuditedEntity_WithGuidKey_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var entity = new TestAuditedEntityWithGuid();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.CreationTime.Should().Be(default);
        entity.CreatorId.Should().BeNull();
        entity.LastModificationTime.Should().BeNull();
        entity.LastModifierId.Should().BeNull();
    }

    /// <summary>
    /// 测试设置创建时间
    /// </summary>
    [Fact]
    public void AuditedEntity_ShouldSetCreationTime()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var expectedTime = DateTime.UtcNow;

        // Act
        entity.CreationTime = expectedTime;

        // Assert
        entity.CreationTime.Should().Be(expectedTime);
    }

    /// <summary>
    /// 测试设置创建者ID
    /// </summary>
    [Fact]
    public void AuditedEntity_ShouldSetCreatorId()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var expectedCreatorId = Guid.NewGuid();

        // Act
        entity.CreatorId = expectedCreatorId;

        // Assert
        entity.CreatorId.Should().Be(expectedCreatorId);
    }

    /// <summary>
    /// 测试设置最后修改时间
    /// </summary>
    [Fact]
    public void AuditedEntity_ShouldSetLastModificationTime()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var expectedTime = DateTime.UtcNow;

        // Act
        entity.LastModificationTime = expectedTime;

        // Assert
        entity.LastModificationTime.Should().Be(expectedTime);
    }

    /// <summary>
    /// 测试设置最后修改者ID
    /// </summary>
    [Fact]
    public void AuditedEntity_ShouldSetLastModifierId()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var expectedModifierId = Guid.NewGuid();

        // Act
        entity.LastModifierId = expectedModifierId;

        // Assert
        entity.LastModifierId.Should().Be(expectedModifierId);
    }

    /// <summary>
    /// 测试完整审计属性
    /// </summary>
    [Fact]
    public void AuditedEntity_ShouldSetAllAuditProperties()
    {
        // Arrange
        var entity = new TestAuditedEntity();
        var creationTime = DateTime.UtcNow.AddHours(-1);
        var creatorId = Guid.NewGuid();
        var lastModificationTime = DateTime.UtcNow;
        var lastModifierId = Guid.NewGuid();

        // Act
        entity.CreationTime = creationTime;
        entity.CreatorId = creatorId;
        entity.LastModificationTime = lastModificationTime;
        entity.LastModifierId = lastModifierId;

        // Assert
        entity.CreationTime.Should().Be(creationTime);
        entity.CreatorId.Should().Be(creatorId);
        entity.LastModificationTime.Should().Be(lastModificationTime);
        entity.LastModifierId.Should().Be(lastModifierId);
    }
}

/// <summary>
/// 测试用审计实体类（整数主键）
/// </summary>
public class TestAuditedEntity : AuditedEntity<int>;

/// <summary>
/// 测试用审计实体类（GUID主键）
/// </summary>
public class TestAuditedEntityWithGuid : AuditedEntity<Guid>;
