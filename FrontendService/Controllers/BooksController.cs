using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FrontendService.ApiClients;
using FrontendService.Contracts;
using FrontendService.Extensions;

using Microsoft.AspNetCore.Mvc;

using SharedTypes.Api;

namespace FrontendService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AuthorsApiClient authorsApiClient;
        private readonly BooksApiClient booksApiClient;

        public BooksController(AuthorsApiClient authorsApiClient, BooksApiClient booksApiClient)
        {
            this.authorsApiClient = authorsApiClient;
            this.booksApiClient = booksApiClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken)
        {
            var bookDtos = await this.booksApiClient.GetBooksAsync(cancellationToken);
            if (bookDtos.Length == 0) return Array.Empty<Book>();

            var authorDtos = (await this.authorsApiClient.GetAuthorsAsync(cancellationToken)).ToDictionary(a => a.Id);
            return bookDtos.Select(bookDto => bookDto.ToContract(authorDtos[bookDto.AuthorId]));
        }

        [HttpGet("{id}")]
        public async Task<Book> GetBook(Guid id, CancellationToken cancellationToken)
        {
            var bookDto = await this.booksApiClient.GetBookAsync(id, cancellationToken);
            var authorDto = await this.authorsApiClient.GetAuthorAsync(bookDto.AuthorId, cancellationToken);
            return bookDto.ToContract(authorDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBook message, CancellationToken cancellationToken)
        {
            var responseMessage = await this.authorsApiClient.UpdateAuthorBooksCountAsync(
                new UpdateBooksCount {
                    AuthorId = message.AuthorId, Delta = 1, UpdateType = UpdateBooksCountType.Increase
                },
                cancellationToken);

            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return this.NotFound($"There is no author with id {message.AuthorId}");
                case HttpStatusCode.BadRequest:
                    return this.BadRequest(responseMessage.ReasonPhrase);
                case HttpStatusCode.OK:
                    var book = await this.booksApiClient.CreateBookAsync(
                        new BookDto {
                            AuthorId = message.AuthorId,
                            Description = message.Description,
                            Id = Guid.Empty,
                            Title = message.Title
                        }, cancellationToken);
                    return this.Ok(book.Id);
                default:
                    return this.StatusCode((int) responseMessage.StatusCode, responseMessage.ReasonPhrase);
            }
        }
    }
}