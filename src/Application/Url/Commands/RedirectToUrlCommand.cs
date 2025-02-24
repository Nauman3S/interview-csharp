using FluentValidation;
using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UrlShortenerService.Application.Url.Commands;

public record RedirectToUrlCommand : IRequest<string>
{
    public string Id { get; init; } = default!;
}

public class RedirectToUrlCommandValidator : AbstractValidator<RedirectToUrlCommand>
{
    public RedirectToUrlCommandValidator()
    {
        _ = RuleFor(v => v.Id)
          .NotEmpty()
          .WithMessage("Id is required.");
    }
}

public class RedirectToUrlCommandHandler : IRequestHandler<RedirectToUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public RedirectToUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received Redirect Request with ID: {request.Id}");

        // Decode the short URL key to get the original ID
        var ids = _hashids.Decode(request.Id);
        if (ids.Length == 0)
        {
            Console.WriteLine("Decoding failed - No valid ID found.");
            throw new KeyNotFoundException("Short URL not found.");
        }

        long decodedId = ids[0];  // Ensure the ID is properly cast
        Console.WriteLine($"Decoded ID: {decodedId}");

        // Fetch the URL entity from the database
        var urlEntity = await _context.Urls
                                      .Where(u => u.Id == decodedId)
                                      .FirstOrDefaultAsync(cancellationToken);

        if (urlEntity == null)
        {
            Console.WriteLine("Entity not found in the database.");
            throw new KeyNotFoundException("Short URL not found.");
        }

        Console.WriteLine($"Found Original URL: {urlEntity.OriginalUrl}");
         // Ensure the URL contains a valid scheme (http/https)
        var redirectUrl = urlEntity.OriginalUrl;
        if (!redirectUrl.StartsWith("http://") && !redirectUrl.StartsWith("https://"))
        {
            redirectUrl = "https://" + redirectUrl;
        }

        Console.WriteLine($"Redirecting to: {redirectUrl}");
        return redirectUrl;
    }
}
