using System;
using System.Linq;
using System.Threading.Tasks;

using AuthorsService.Extensions;
using AuthorsService.Grpc;
using AuthorsService.Models;

using Grpc.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Author = AuthorsService.Models.Author;

namespace AuthorsService.Controllers
{
    public class AuthorsGrpcService : AuthorsServiceProto.AuthorsServiceProtoBase
    {
        private readonly AuthorContext context;
        private readonly ILogger<AuthorsGrpcService> logger;

        public AuthorsGrpcService(AuthorContext context, ILogger<AuthorsGrpcService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public override async Task<GetAuthorsResponse> GetAuthors(GetAuthorsRequest request,
            ServerCallContext callContext)
        {
            try
            {
                var authors = await this.context.Authors.Select(a => a.ToGrpcDto())
                    .ToListAsync(callContext.CancellationToken);
                GetAuthorsResponse response = new();
                response.Authors.AddRange(authors);
                return response;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }

        public override async Task<GetAuthorResponse> GetAuthor(GetAuthorRequest request, ServerCallContext callContext)
        {
            try
            {
                var author =
                    await this.context.Authors.FindAsync(new object[] {request.Id}, callContext.CancellationToken);

                if (author == null)
                    throw new RpcException(new Status(StatusCode.NotFound, $"There is no author with id {request.Id}"));

                GetAuthorResponse response = new() {Author = author.ToGrpcDto()};
                return response;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }

        public override async Task<UpdateAuthorResponse>
            UpdateAuthor(UpdateAuthorRequest request, ServerCallContext callContext)
        {
            var author =
                await this.context.Authors.FindAsync(new object[] {request.Author.Id}, callContext.CancellationToken);

            if (author is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"There is no author with id {request.Author.Id}"));
            }

            author.Age = request.Author.Age;
            author.Biography = request.Author.Biography;
            author.FirstName = request.Author.FirstName;
            author.LastName = request.Author.LastName;
            author.NumberOfBooks = request.Author.NumberOfBooks;
            this.context.Entry(author).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(callContext.CancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.context.Authors.Any(a => a.Id.ToString() == request.Author.Id))
                {
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"There is no author with id {request.Author.Id}"));
                }

                throw new RpcException(new Status(StatusCode.Internal, "Couldn't update author"));
            }

            return new UpdateAuthorResponse();
        }

        public override async Task<CrateAuthorResponse> CreateAuthor(CrateAuthorRequest request,
            ServerCallContext callContext)
        {
            var author = new Author {
                Age = request.Author.Age,
                Biography = request.Author.Biography,
                FirstName = request.Author.FirstName,
                Id = Guid.NewGuid(),
                LastName = request.Author.LastName,
                NumberOfBooks = request.Author.NumberOfBooks
            };

            try
            {
                this.context.Authors.Add(author);
                await this.context.SaveChangesAsync(callContext.CancellationToken);
                CrateAuthorResponse response = new() {Id = author.Id.ToString()};
                return response;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
        }

        public override async Task<DeleteAuthorResponse>
            DeleteAuthor(DeleteAuthorRequest request, ServerCallContext callContext)
        {
            var author = await this.context.Authors.FindAsync(new object[] {request.Id}, callContext.CancellationToken);
            if (author == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"There is no author with id {request.Id}"));

            this.context.Authors.Remove(author);
            await this.context.SaveChangesAsync(callContext.CancellationToken);

            return new DeleteAuthorResponse();
        }

        public override async Task<UpdateAuthorBooksCountResponse> UpdateAuthorBooksCount(
            UpdateAuthorBooksCountRequest request, ServerCallContext callContext)
        {
            var author = await this.context.Authors.FindAsync(new object[] { request.Id }, callContext.CancellationToken);
            if (author == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"There is no author with id {request.Id}"));

            switch (request.UpdateType)
            {
                case UpdateType.Increase:
                    author.NumberOfBooks += request.Delta;
                    break;
                case UpdateType.Decrease:
                    if (author.NumberOfBooks < request.Delta)
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"Author has {author.NumberOfBooks} books but delta is {request.Delta}"));
                    author.NumberOfBooks -= request.Delta;
                    break;
                default:
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid update type {request.UpdateType}"));
            }

            this.context.Entry(author).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync(callContext.CancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.context.Authors.Any(a => a.Id.ToString() == request.Id))
                {
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"There is no author with id {request.Id}"));
                }

                throw new RpcException(new Status(StatusCode.Internal, "Couldn't update author"));
            }

            return new UpdateAuthorBooksCountResponse();
        }
    }
}