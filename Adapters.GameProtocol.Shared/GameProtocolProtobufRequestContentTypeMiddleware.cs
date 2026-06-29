using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared;

public static class GameProtocolProtobufRequestContentTypeMiddleware
{
    public const string ProtobufContentType = "application/protobuf";

    public static IApplicationBuilder UseGameProtocolProtobufRequestContentTypeFallback(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            ApplyAssumedContentType(context);
            await next();
        });

    public static void ApplyAssumedContentType(HttpContext context)
    {
        if (ShouldAssumeProtobufRequest(context))
        {
            context.Request.ContentType = ProtobufContentType;
        }
    }

    public static bool ShouldAssumeProtobufRequest(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || !string.IsNullOrWhiteSpace(context.Request.ContentType))
        {
            return false;
        }

        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        return action is not null && IsGameProtocolController(action.ControllerTypeInfo.AsType());
    }

    private static bool IsGameProtocolController(Type type)
    {
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseProtocolController<>))
            {
                return true;
            }
        }

        return false;
    }
}
