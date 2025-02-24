Dockerfile
Simple dotnet sdk v8 container

docker-compose.yml
simple service to run the container

Url.cs
ShortUrlKey: A nullable string property to store the short URL key.

CreateShortUrlCommand.cs
Handler Logic:
Created a new Url entity with the original URL.
Saved the entity to generate an ID.
Encoded the ID to generate a short URL key using Hashids.
Updated the Url entity with the short URL key and saved changes.

RedirectToUrlCommand.cs
Handler Logic:
Decoded the short URL key to retrieve the original ID.
Fetched the Url entity from the database using the decoded ID.
Returned the original URL, ensuring it has a valid scheme (http/https).

RedirectToUrlCommand.cs
SendRedirectAsync(result) Doesn’t Properly End Request Processing, issue with local url paths.