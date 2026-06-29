using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Shared;
using KimidoriSongHashController = TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers.SongHashController;
using MomoiroSongHashController = TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers.SongHashController;

namespace TaikoLocalServer.Tests.Architecture;

public sealed class GameProtocolProtobufRequestContentTypeMiddlewareTests
{
    [Theory]
    [InlineData(typeof(KimidoriSongHashController))]
    [InlineData(typeof(MomoiroSongHashController))]
    public void ProtocolControllerPostWithoutContentType_AssumesProtobuf(Type controllerType)
    {
        var context = CreateContext(controllerType);
        context.Request.Method = HttpMethods.Post;

        GameProtocolProtobufRequestContentTypeMiddleware.ApplyAssumedContentType(context);

        Assert.Equal(GameProtocolProtobufRequestContentTypeMiddleware.ProtobufContentType, context.Request.ContentType);
    }

    [Fact]
    public void ProtocolControllerPostWithContentType_PreservesExistingContentType()
    {
        var context = CreateContext(typeof(KimidoriSongHashController));
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/octet-stream";

        GameProtocolProtobufRequestContentTypeMiddleware.ApplyAssumedContentType(context);

        Assert.Equal("application/octet-stream", context.Request.ContentType);
    }

    [Fact]
    public void ProtocolControllerGetWithoutContentType_DoesNotAssumeProtobuf()
    {
        var context = CreateContext(typeof(KimidoriSongHashController));
        context.Request.Method = HttpMethods.Get;

        GameProtocolProtobufRequestContentTypeMiddleware.ApplyAssumedContentType(context);

        Assert.Null(context.Request.ContentType);
    }

    [Fact]
    public void NonProtocolControllerPostWithoutContentType_DoesNotAssumeProtobuf()
    {
        var context = CreateContext(typeof(NonProtocolController));
        context.Request.Method = HttpMethods.Post;

        GameProtocolProtobufRequestContentTypeMiddleware.ApplyAssumedContentType(context);

        Assert.Null(context.Request.ContentType);
    }

    [Fact]
    public void PostWithoutSelectedEndpoint_DoesNotAssumeProtobuf()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;

        GameProtocolProtobufRequestContentTypeMiddleware.ApplyAssumedContentType(context);

        Assert.Null(context.Request.ContentType);
    }

    private static DefaultHttpContext CreateContext(Type controllerType)
    {
        var context = new DefaultHttpContext();
        var action = new ControllerActionDescriptor
        {
            ControllerTypeInfo = controllerType.GetTypeInfo()
        };
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(action),
            $"{controllerType.Name} test endpoint"));

        return context;
    }

    private sealed class NonProtocolController;
}
