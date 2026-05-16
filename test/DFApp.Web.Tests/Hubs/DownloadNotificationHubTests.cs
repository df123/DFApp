using DFApp.Web.Hubs;
using FluentAssertions;
using Xunit;

namespace DFApp.Web.Tests.Hubs;

/// <summary>
/// DownloadNotificationHub 单元测试
/// </summary>
public class DownloadNotificationHubTests
{
    /// <summary>
    /// 测试 Hub URL 常量是否正确
    /// </summary>
    [Fact]
    public void HubUrl_ShouldBeCorrect()
    {
        // Act
        var hubUrl = DownloadNotificationHub.HubUrl;

        // Assert
        hubUrl.Should().Be("/hubs/download-notification");
    }

    /// <summary>
    /// 测试 Hub 类可以实例化
    /// </summary>
    [Fact]
    public void Hub_ShouldBeInstantiable()
    {
        // Act
        var hub = new DownloadNotificationHub();

        // Assert
        hub.Should().NotBeNull();
        hub.Should().BeAssignableTo<Microsoft.AspNetCore.SignalR.Hub>();
    }
}
