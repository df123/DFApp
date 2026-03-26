using DFApp.Web.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using Xunit;

namespace DFApp.Web.Tests.Infrastructure;

/// <summary>
/// 全局异常过滤器单元测试
/// </summary>
public class GlobalExceptionFilterTests
{
    private readonly GlobalExceptionFilter _filter;

    public GlobalExceptionFilterTests()
    {
        _filter = new GlobalExceptionFilter();
    }

    /// <summary>
    /// 测试处理业务异常
    /// </summary>
    [Fact]
    public void OnException_BusinessException_ShouldReturnBadRequest()
    {
        // Arrange
        var exceptionContext = CreateExceptionContext(new BusinessException("业务错误"));
        var expectedMessage = "业务错误";

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        exceptionContext.ExceptionHandled.Should().BeTrue();
        var result = exceptionContext.Result as ObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = result?.Value as ErrorResponse;
        response.Should().NotBeNull();
        response?.Message.Should().Be(expectedMessage);
    }

    /// <summary>
    /// 测试处理未找到异常
    /// </summary>
    [Fact]
    public void OnException_NotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var exceptionContext = CreateExceptionContext(new NotFoundException("资源未找到"));

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        exceptionContext.ExceptionHandled.Should().BeTrue();
        var result = exceptionContext.Result as ObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var response = result?.Value as ErrorResponse;
        response.Should().NotBeNull();
        // NotFoundException 继承自 BusinessException，返回自定义消息
        response?.Message.Should().Be("资源未找到");
        response?.Code.Should().Be("NotFound");
    }

    /// <summary>
    /// 测试处理验证异常
    /// </summary>
    [Fact]
    public void OnException_ValidationException_ShouldReturnBadRequest()
    {
        // Arrange
        var exceptionContext = CreateExceptionContext(new ValidationException("验证失败"));

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        exceptionContext.ExceptionHandled.Should().BeTrue();
        var result = exceptionContext.Result as ObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = result?.Value as ErrorResponse;
        response.Should().NotBeNull();
        // ValidationException 继承自 BusinessException，返回自定义消息
        response?.Message.Should().Be("验证失败");
        response?.Code.Should().Be("ValidationError");
    }

    /// <summary>
    /// 测试处理未处理的异常
    /// </summary>
    [Fact]
    public void OnException_UnhandledException_ShouldReturnInternalServerError()
    {
        // Arrange
        var exceptionContext = CreateExceptionContext(new Exception("未知错误"));

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        exceptionContext.ExceptionHandled.Should().BeTrue();
        var result = exceptionContext.Result as ObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var response = result?.Value as ErrorResponse;
        response.Should().NotBeNull();
        response?.Message.Should().Be("服务器内部错误，请稍后重试");
    }

    /// <summary>
    /// 测试异常已处理时不重复处理
    /// </summary>
    [Fact]
    public void OnException_ExceptionAlreadyHandled_ShouldNotProcessAgain()
    {
        // Arrange
        var exceptionContext = CreateExceptionContext(new BusinessException("业务错误"));
        exceptionContext.ExceptionHandled = true;

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        // GlobalExceptionFilter 会处理所有异常，即使已经标记为已处理
        exceptionContext.ExceptionHandled.Should().BeTrue();
        var result = exceptionContext.Result as ObjectResult;
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 创建异常上下文
    /// </summary>
    private ExceptionContext CreateExceptionContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();

        // 设置必要的服务
        var services = new ServiceCollection();
        var environmentMock = new Mock<IWebHostEnvironment>();
        environmentMock.Setup(x => x.EnvironmentName).Returns("Production");
        services.AddSingleton(environmentMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }
}
