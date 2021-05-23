using FrontendService.Contracts;

using SharedTypes.Api;

namespace FrontendService.Extensions
{
    public static class BooksExtensions
    {
        public static Book ToContract(this BookDto bookDto, AuthorDto authorDto) => new() {
            AuthorFirstname = authorDto.FirstName,
            AuthorId = bookDto.AuthorId,
            AuthorLastName = authorDto.LastName,
            Description = bookDto.Description,
            Id = bookDto.Id,
            Title = bookDto.Title
        };
    }
}