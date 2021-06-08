using AuthorsService.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AuthorsService.Extensions;

using SharedTypes.Api;

namespace AuthorsService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorContext context;

        public AuthorsController(AuthorContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(CancellationToken cancellationToken) =>
            await this.context.Authors.Select(a => a.ToDto()).ToListAsync(cancellationToken);

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id, CancellationToken cancellationToken)
        {
            var author = await this.context.Authors.FindAsync(new object[] {id}, cancellationToken);

            if (author == null) return this.NotFound();

            return author.ToDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(Guid id, AuthorDto authorDto, CancellationToken cancellationToken)
        {
            if (id != authorDto.Id) return this.BadRequest();

            var author = await this.context.Authors.FindAsync(new object[] {id}, cancellationToken);
            if (author is null) return this.NotFound();

            author.Age = authorDto.Age;
            author.Biography = authorDto.Biography;
            author.FirstName = authorDto.FirstName;
            author.LastName = authorDto.LastName;
            author.NumberOfBooks = authorDto.NumberOfBooks;
            this.context.Entry(author).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.AuthorExists(id)) return this.NotFound();

                throw;
            }

            return this.Ok();
        }

        [HttpPut("{id}/bookscount")]
        public async Task<IActionResult> UpdateAuthorBooksCount(Guid id, UpdateBooksCount updateCountDto,
            CancellationToken cancellationToken)
        {
            if (id != updateCountDto.AuthorId) return this.BadRequest();

            var author = await this.context.Authors.FindAsync(new object[] {id}, cancellationToken);
            if (author is null) return this.NotFound();

            switch (updateCountDto.UpdateType)
            {
                case UpdateBooksCountType.Increase:
                    author.NumberOfBooks += updateCountDto.Delta;
                    break;
                case UpdateBooksCountType.Decrease:
                    if (author.NumberOfBooks < updateCountDto.Delta)
                        return this.BadRequest($"Author has {author.NumberOfBooks} books but delta is {updateCountDto.Delta}");
                    author.NumberOfBooks -= updateCountDto.Delta;
                    break;
                default:
                    return this.BadRequest($"Unsupported update type {updateCountDto.UpdateType}");
            }

            this.context.Entry(author).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.AuthorExists(id)) return this.NotFound();

                throw;
            }

            return this.Ok();
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorDto authorDto,
            CancellationToken cancellationToken)
        {
            var author = new Author {
                Age = authorDto.Age,
                Biography = authorDto.Biography,
                FirstName = authorDto.FirstName,
                Id = Guid.NewGuid(),
                LastName = authorDto.LastName,
                NumberOfBooks = authorDto.NumberOfBooks
            };

            this.context.Authors.Add(author);
            await this.context.SaveChangesAsync(cancellationToken);

            authorDto.Id = author.Id;
            return this.CreatedAtAction(nameof(AuthorsController.GetAuthor), new {id = author.Id}, author);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(Guid id, CancellationToken cancellationToken)
        {
            var author = await this.context.Authors.FindAsync(new object[] {id}, cancellationToken);
            if (author == null) return this.NotFound();

            this.context.Authors.Remove(author);
            await this.context.SaveChangesAsync(cancellationToken);

            return this.Ok();
        }

        private bool AuthorExists(Guid id) => this.context.Authors.Any(e => e.Id == id);
    }
}