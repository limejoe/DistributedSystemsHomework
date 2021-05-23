using AuthorsService.Models;
using SharedTypes.Api;

namespace AuthorsService.Extensions
{
    public static class AuthorExtensions
    {
        public static AuthorDto ToDto(this Author author) =>
            new()
            {
                Age = author.Age,
                Biography = author.Biography,
                FirstName = author.FirstName,
                Id = author.Id,
                LastName = author.LastName,
                NumberOfBooks = author.NumberOfBooks
            };
    }
}