using System.Net;
using DFApp.Web.Infrastructure;
using FluentAssertions;
using Xunit;

namespace DFApp.Web.Tests.Infrastructure;

/// <summary>
/// SSRF 出站防护单元测试
/// </summary>
public class SsrfGuardTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("127.0.0.2")]
    [InlineData("127.1.1.1")]
    [InlineData("127.255.255.254")]
    [InlineData("10.0.0.5")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.169.254")]
    [InlineData("0.0.0.0")]
    [InlineData("100.64.0.1")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("fc00::1")]
    [InlineData("::ffff:127.0.0.1")]
    [InlineData("::ffff:127.0.0.2")]
    public void IsBlockedAddress_PrivateOrReserved_ShouldBeBlocked(string address)
    {
        SsrfGuard.IsBlockedAddress(IPAddress.Parse(address)).Should().BeTrue(
            $"内网/保留地址 {address} 应被拒绝");
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("2606:4700:4700::1111")]
    public void IsBlockedAddress_Public_ShouldBeAllowed(string address)
    {
        SsrfGuard.IsBlockedAddress(IPAddress.Parse(address)).Should().BeFalse(
            $"公网地址 {address} 不应被拒绝");
    }

    [Theory]
    [InlineData("ftp://example.com/feed")]
    [InlineData("file:///etc/passwd")]
    [InlineData("gopher://127.0.0.1:6379/_INFO")]
    [InlineData("not a url")]
    [InlineData("")]
    [InlineData(null)]
    public void EnsureAllowed_NonHttpScheme_ShouldThrow(string? url)
    {
        var act = () => SsrfGuard.EnsureAllowed(url);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureAllowed_UserInfoInUrl_ShouldThrow()
    {
        var act = () => SsrfGuard.EnsureAllowed("http://user:pass@example.com/feed");
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("http://127.0.0.1/api")]
    [InlineData("http://127.0.0.2/api")]
    [InlineData("http://127.10.20.30:8080/")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("http://10.0.0.5:8080/")]
    [InlineData("http://[::1]/")]
    public void EnsureAllowed_PrivateIpLiteral_ShouldThrow(string url)
    {
        var act = () => SsrfGuard.EnsureAllowed(url);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("http://example.com/feed.xml")]
    [InlineData("https://www.cwl.gov.cn/cwl_admin/front/cwlkj_search/kjxx/findDrawNotice")]
    public void EnsureAllowed_PublicHost_ShouldPass(string url)
    {
        var uri = SsrfGuard.EnsureAllowed(url);
        uri.ToString().Should().Be(url);
    }
}
