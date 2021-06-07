using BooksService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using BooksService.ApiClients;
using BooksService.Extensions;

using SharedTypes.Api;

namespace BooksService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookContext context;
        private readonly AuthorsApiClient authorsApiClient;

        public BooksController(BookContext context, AuthorsApiClient authorsApiClient)
        {
            this.context = context;
            this.authorsApiClient = authorsApiClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(CancellationToken cancellationToken) =>
            await this.context.Books.Select(b => b.ToDto()).ToListAsync(cancellationToken);

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(Guid id, CancellationToken cancellationToken)
        {
            var book = await this.context.Books.FindAsync(new object[] {id}, cancellationToken);

            if (book == null) return this.NotFound();

            return book.ToDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(Guid id, BookDto bookDto, CancellationToken cancellationToken)
        {
            if (id != bookDto.Id) return this.BadRequest();

            var book = await this.context.Books.FindAsync(new object[] {id}, cancellationToken);
            if (book is null) return this.NotFound();

            book.AuthorId = bookDto.AuthorId;
            book.Description = bookDto.Description;
            book.Title = bookDto.Title;

            this.context.Entry(book).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.BookExists(id))
                    return this.NotFound();
                throw;
            }

            return this.Ok();
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> CreateBook(BookDto bookDto, CancellationToken cancellationToken)
        {
            var responseMessage = await this.authorsApiClient.UpdateAuthorBooksCountAsync(
                new UpdateBooksCount
                {
                    AuthorId = bookDto.AuthorId,
                    Delta = 1,
                    UpdateType = UpdateBooksCountType.Increase
                },
                cancellationToken);

            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return this.NotFound($"There is no author with id {bookDto.AuthorId}");
                case HttpStatusCode.BadRequest:
                    return this.BadRequest(responseMessage.ReasonPhrase);
                case HttpStatusCode.OK:
                    break;
                default:
                    return this.StatusCode((int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
            }

            var book = new Book {
                Description = bookDto.Description,
                Id = Guid.NewGuid(),
                Title = bookDto.Title,
                AuthorId = bookDto.AuthorId
            };

            try
            {
                this.context.Books.Add(book);
                await this.context.SaveChangesAsync(cancellationToken);

                bookDto.Id = book.Id;
                return this.CreatedAtAction(nameof(BooksController.GetBook), new {id = book.Id}, bookDto);
            }
            catch (Exception)
            {
                await this.authorsApiClient.UpdateAuthorBooksCountAsync(
                    new UpdateBooksCount
                    {
                        AuthorId = bookDto.AuthorId,
                        Delta = 1,
                        UpdateType = UpdateBooksCountType.Decrease
                    },
                    cancellationToken);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
        {
            var book = await this.context.Books.FindAsync(new object[] {id}, cancellationToken);
            if (book is null) return this.NotFound();

            this.context.Books.Remove(book);
            await this.context.SaveChangesAsync(cancellationToken);

            return this.Ok();
        }

        private bool BookExists(Guid id) => this.context.Books.Any(e => e.Id == id);
    }
}