using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AuthorsService.Grpc;

using BooksService.Extensions;
using BooksService.Grpc;
using BooksService.Models;

using Grpc.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Book = BooksService.Models.Book;

namespace BooksService.Controllers
{
    public class BooksGrpcService : BooksServiceProto.BooksServiceProtoBase
    {
        private readonly BookContext context;
        private readonly AuthorsServiceProto.AuthorsServiceProtoClient authorsClient;
        private readonly ILogger<BooksGrpcService> logger;

        public BooksGrpcService(BookContext context, AuthorsServiceProto.AuthorsServiceProtoClient authorsClient,
            ILogger<BooksGrpcService> logger)
        {
            this.context = context;
            this.authorsClient = authorsClient;
            this.logger = logger;
        }

        public override async Task<GetBooksResponse> GetBooks(GetBooksRequest request, ServerCallContext callContext)
        {
            try
            {
                var books = await this.context.Books.Select(a => a.ToGrpcDto())
                    .ToListAsync(callContext.CancellationToken);
                GetBooksResponse response = new();
                response.Books.AddRange(books);
                return response;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }

        public override async Task<GetBookResponse> GetBook(GetBookRequest request, ServerCallContext callContext)
        {
            try
            {
                var book =
                    await this.context.Books.FindAsync(new object[] {request.Id}, callContext.CancellationToken);

                if (book == null)
                    throw new RpcException(new Status(StatusCode.NotFound, $"There is no author with id {request.Id}"));

                GetBookResponse response = new() {Book = book.ToGrpcDto()};
                return response;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }

        public override async Task<UpdateBookResponse> UpdateBook(UpdateBookRequest request,
            ServerCallContext callContext)
        {
            var book =
                await this.context.Books.FindAsync(new object[] {request.Book.Id}, callContext.CancellationToken);

            if (book is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"There is no book with id {request.Book.Id}"));
            }

            book.AuthorId = Guid.Parse(request.Book.AuthorId);
            book.Description = request.Book.Description;
            book.Title = request.Book.Title;
            this.context.Entry(book).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(callContext.CancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.context.Books.Any(a => a.Id.ToString() == request.Book.Id))
                {
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"There is no book with id {request.Book.Id}"));
                }

                throw new RpcException(new Status(StatusCode.Internal, "Couldn't update book"));
            }

            return new UpdateBookResponse();
        }

        public override async Task<CrateBookResponse> CreateBook(CrateBookRequest request, ServerCallContext callContext)
        {
            await this.UpdateAuthorBooksCount(request.Book.AuthorId, UpdateType.Increase,
                callContext.CancellationToken);

            var book = new Book {
                Description = request.Book.Description,
                Id = Guid.NewGuid(),
                Title = request.Book.Title,
                AuthorId = Guid.Parse(request.Book.AuthorId)
            };

            try
            {
                this.context.Books.Add(book);
                await this.context.SaveChangesAsync(callContext.CancellationToken);

                return new CrateBookResponse {Id = book.Id.ToString()};
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                await this.UpdateAuthorBooksCount(request.Book.AuthorId, UpdateType.Decrease,
                    callContext.CancellationToken);
                throw new RpcException(new Status(StatusCode.Internal, "Book creation failed"));
            }
        }

        public override async Task<DeleteBooksResponse> DeleteBook(DeleteBookRequest request,
            ServerCallContext callContext)
        {
            var book = await this.context.Books.FindAsync(new object[] {request.Id}, callContext.CancellationToken);
            if (book == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"There is no book with id {request.Id}"));

            await this.UpdateAuthorBooksCount(book.AuthorId.ToString(), UpdateType.Decrease,
                callContext.CancellationToken);

            try
            {
                this.context.Books.Remove(book);
                await this.context.SaveChangesAsync(callContext.CancellationToken);

                return new DeleteBooksResponse();
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                await this.UpdateAuthorBooksCount(book.AuthorId.ToString(), UpdateType.Increase,
                    callContext.CancellationToken);
                throw new RpcException(new Status(StatusCode.Internal, $"Cannot delete book {book.Id}"));
            }
        }

        private async Task UpdateAuthorBooksCount(string authorId, UpdateType updateType,
            CancellationToken cancellationToken)
        {
            try
            {
                await this.authorsClient.UpdateAuthorBooksCountAsync(
                    new UpdateAuthorBooksCountRequest {Delta = 1, Id = authorId, UpdateType = updateType},
                    cancellationToken: cancellationToken);
            }
            catch (RpcException e)
            {
                this.logger.LogError(e.Message);
                switch (e.StatusCode)
                {
                    case StatusCode.NotFound:
                    case StatusCode.InvalidArgument:
                        throw new RpcException(e.Status, e.Message);
                    default:
                        throw new RpcException(new Status(StatusCode.Internal,
                            "Something went wrong when updating author books count"));
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }
    }
}