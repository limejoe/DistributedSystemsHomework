using AuthorsService.Models;

using SharedTypes.Api;

using AuthorGrpcDto = AuthorsService.Grpc.Author;

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

        public static AuthorGrpcDto ToGrpcDto(this Author author) =>
            new()
            {
                Age = author.Age,
                Biography = author.Biography,
                FirstName = author.FirstName,
                Id = author.Id.ToString(),
                LastName = author.LastName,
                NumberOfBooks = author.NumberOfBooks
            };

    }
}