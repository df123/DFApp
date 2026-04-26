using DFApp.Web.Domain;
using Xunit;

namespace DFApp.Web.Tests.Domain;

/// <summary>
/// 完整审计实体单元测试
/// </summary>
public class FullAuditedEntityTests
{
    /// <summary>
    /// 测试整数主键完整审计实体初始化
    /// </summary>
    [Fact]
    public void FullAuditedEntity_WithIntKey_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var entity = new TestFullAuditedEntity();

        // Assert
        entity.Id.Should().Be(0);
        entity.CreationTime.Should().Be(default);
        entity.CreatorId.Should().BeNull();
        entity.LastModificationTime.Should().BeNull();
        entity.LastModifierId.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletionTime.Should().BeNull();
        entity.DeleterId.Should().BeNull();
    }

    /// <summary>
    /// 测试GUID主键完整审计实体初始化
    /// </summary>
    [Fact]
    public void FullAuditedEntity_WithGuidKey_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var entity = new TestFullAuditedEntityWithGuid();

        // Assert
        entity.Id.Should().Be(Guid.Empty);
        entity.CreationTime.Should().Be(default);
        entity.CreatorId.Should().BeNull();
        entity.LastModificationTime.Should().BeNull();
        entity.LastModifierId.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletionTime.Should().BeNull();
        entity.DeleterId.Should().BeNull();
    }

    /// <summary>
    /// 测试设置删除标记
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSetIsDeleted()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();

        // Act
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    /// <summary>
    /// 测试设置删除时间
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSetDeletionTime()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();
        var expectedTime = DateTime.UtcNow;

        // Act
        entity.DeletionTime = expectedTime;

        // Assert
        entity.DeletionTime.Should().Be(expectedTime);
    }

    /// <summary>
    /// 测试设置删除者ID
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSetDeleterId()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();
        var expectedDeleterId = Guid.NewGuid();

        // Act
        entity.DeleterId = expectedDeleterId;

        // Assert
        entity.DeleterId.Should().Be(expectedDeleterId);
    }

    /// <summary>
    /// 测试完整审计属性（包括软删除）
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSetAllAuditProperties()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();
        var creationTime = DateTime.UtcNow.AddHours(-2);
        var creatorId = Guid.NewGuid();
        var lastModificationTime = DateTime.UtcNow.AddHours(-1);
        var lastModifierId = Guid.NewGuid();
        var isDeleted = true;
        var deletionTime = DateTime.UtcNow;
        var deleterId = Guid.NewGuid();

        // Act
        entity.CreationTime = creationTime;
        entity.CreatorId = creatorId;
        entity.LastModificationTime = lastModificationTime;
        entity.LastModifierId = lastModifierId;
        entity.IsDeleted = isDeleted;
        entity.DeletionTime = deletionTime;
        entity.DeleterId = deleterId;

        // Assert
        entity.CreationTime.Should().Be(creationTime);
        entity.CreatorId.Should().Be(creatorId);
        entity.LastModificationTime.Should().Be(lastModificationTime);
        entity.LastModifierId.Should().Be(lastModifierId);
        entity.IsDeleted.Should().Be(isDeleted);
        entity.DeletionTime.Should().Be(deletionTime);
        entity.DeleterId.Should().Be(deleterId);
    }

    /// <summary>
    /// 测试软删除功能
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSupportSoftDelete()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();
        var deleterId = Guid.NewGuid();
        var deletionTime = DateTime.UtcNow;

        // Act
        entity.IsDeleted = true;
        entity.DeleterId = deleterId;
        entity.DeletionTime = deletionTime;

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.DeleterId.Should().Be(deleterId);
        entity.DeletionTime.Should().Be(deletionTime);
    }

    /// <summary>
    /// 测试恢复删除（软删除）
    /// </summary>
    [Fact]
    public void FullAuditedEntity_ShouldSupportRestore()
    {
        // Arrange
        var entity = new TestFullAuditedEntity();
        entity.IsDeleted = true;
        entity.DeleterId = Guid.NewGuid();
        entity.DeletionTime = DateTime.UtcNow;

        // Act
        entity.IsDeleted = false;
        entity.DeleterId = null;
        entity.DeletionTime = null;

        // Assert
        entity.IsDeleted.Should().BeFalse();
        entity.DeleterId.Should().BeNull();
        entity.DeletionTime.Should().BeNull();
    }
}

/// <summary>
/// 测试用完整审计实体类（整数主键）
/// </summary>
public class TestFullAuditedEntity : FullAuditedEntity<int>;

/// <summary>
/// 测试用完整审计实体类（GUID主键）
/// </summary>
public class TestFullAuditedEntityWithGuid : FullAuditedEntity<Guid>;
