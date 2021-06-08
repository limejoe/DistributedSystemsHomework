using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AuthorsService.Grpc;

using BooksService.Grpc;

using FrontendService.ApiClients;
using FrontendService.Contracts;
using FrontendService.Extensions;

using Microsoft.AspNetCore.Mvc;

using SharedTypes.Api;

using Book = FrontendService.Contracts.Book;

namespace FrontendService.Controllers
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class BooksControllerGrpc : ControllerBase
    {
        private readonly AuthorsServiceProto.AuthorsServiceProtoClient authorsApiClient;
        private readonly BooksServiceProto.BooksServiceProtoClient booksApiClient;

        public BooksControllerGrpc(AuthorsServiceProto.AuthorsServiceProtoClient authorsApiClient,
            BooksServiceProto.BooksServiceProtoClient booksApiClient)
        {
            this.authorsApiClient = authorsApiClient;
            this.booksApiClient = booksApiClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken)
        {
            var response = await this.booksApiClient.GetBooksAsync(new GetBooksRequest(), cancellationToken: cancellationToken);
            if (response.Books.Capacity == 0) return Array.Empty<Book>();

            var authorDtos = (await this.authorsApiClient.GetAuthorsAsync(new GetAuthorsRequest(), cancellationToken: cancellationToken)).Authors.ToDictionary(a => a.Id);
            return response.Books.Select(bookDto => bookDto.ToContract(authorDtos[bookDto.AuthorId]));
        }

        [HttpGet("{id}")]
        public async Task<Book> GetBook(Guid id, CancellationToken cancellationToken)
        {
            var bookResponse = await this.booksApiClient.GetBookAsync(new GetBookRequest {Id = id.ToString()}, cancellationToken: cancellationToken);
            var authorResponse = await this.authorsApiClient.GetAuthorAsync(new GetAuthorRequest {Id = bookResponse.Book.AuthorId }, cancellationToken: cancellationToken);
            return bookResponse.Book.ToContract(authorResponse.Author);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBook message, CancellationToken cancellationToken)
        {
            var book = await this.booksApiClient.CreateBookAsync(
                new CrateBookRequest {
                    Book = new BooksService.Grpc.Book {
                        AuthorId = message.AuthorId.ToString(),
                        Description = message.Description,
                        Id = "",
                        Title = message.Title
                    }
                }, cancellationToken: cancellationToken);
            return this.Ok(book.Id);
        }
    }
}