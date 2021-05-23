using BooksService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BooksService.Extensions;

using SharedTypes.Api;

namespace BooksService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookContext context;

        public BooksController(BookContext context)
        {
            this.context = context;
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
            var book = new Book {
                Description = bookDto.Description,
                Id = Guid.NewGuid(),
                Title = bookDto.Title,
                AuthorId = bookDto.AuthorId
            };

            this.context.Books.Add(book);
            await this.context.SaveChangesAsync(cancellationToken);

            bookDto.Id = book.Id;
            return this.CreatedAtAction(nameof(BooksController.GetBook), new {id = book.Id}, bookDto);
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