using FrontendService.Contracts;

using SharedTypes.Api;

namespace FrontendService.Extensions
{
    public static class AuthorExtensions
    {
        public static Author ToContract(this AuthorDto dto) => new() {
            NumberOfBooks = dto.NumberOfBooks,
            LastName = dto.LastName,
            Age = dto.Age,
            Biography = dto.Biography,
            FirstName = dto.FirstName,
            Id = dto.Id
        };
    }
}