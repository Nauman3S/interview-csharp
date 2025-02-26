using MediatR;
using UrlShortenerService.Api.Endpoints.Url.Requests;
using UrlShortenerService.Application.Url.Commands;
using IMapper = AutoMapper.IMapper;

namespace UrlShortenerService.Api.Endpoints.Url;

public class RedirectToUrlSummary : Summary<RedirectToUrlEndpoint>
{
    public RedirectToUrlSummary()
    {
        Summary = "Redirect to the original url from the short url";
        Description =
            "This endpoint will redirect to the original url from the short url. If the short url is not found, it will return a 404.";
        Response(404, "No short url found.");
        Response(500, "Internal server error.");
    }
}

public class RedirectToUrlEndpoint : BaseEndpoint<RedirectToUrlRequest>
{
    public RedirectToUrlEndpoint(ISender mediator, IMapper mapper)
        : base(mediator, mapper) { }

    public override void Configure()
    {
        base.Configure();
        Get("u/{Id}");
        AllowAnonymous();
        Description(
            d => d.WithTags("Url")
        );
        Summary(new RedirectToUrlSummary());
    }

    public override async Task HandleAsync(RedirectToUrlRequest req, CancellationToken ct)
    {
            Console.WriteLine($"Handling redirect for: {req.Id}");

        var result = await Mediator.Send(
            new RedirectToUrlCommand
            {
                Id = req.Id
            },
            ct
        );
        Console.WriteLine($"Redirecting to: {result}");

        // Ensure the URL is properly formatted
        if (!result.StartsWith("http://") && !result.StartsWith("https://"))
        {
            result = "https://" + result;
        }

        // Send a proper HTTP 302 redirect and terminate processing
        HttpContext.Response.Redirect(result, permanent: false);
        await HttpContext.Response.CompleteAsync(); // Ensures request is terminated immediately

    }
}
