using FluentValidation;
using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Domain.Entities; 

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        // Create a new Url entity
        var urlEntity = new UrlShortenerService.Domain.Entities.Url { OriginalUrl = request.Url };

        // Save the entity to generate the Id
        _context.Urls.Add(urlEntity);
        
        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Generated ID: {urlEntity.Id}");

        // Generate the short URL key
        var shortUrlKey = _hashids.Encode((int)urlEntity.Id);
        Console.WriteLine($"Encoded Key: {shortUrlKey}");

        // Set the ShortUrlKey property
        urlEntity.ShortUrlKey = shortUrlKey;
        _context.Urls.Update(urlEntity);

        // Update the entity with the ShortUrlKey
        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"🔗 Short URL Created: /u/{shortUrlKey}");


        return shortUrlKey;
    }
}
