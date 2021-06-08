using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AuthorsService.Grpc;

using FrontendService.Contracts;
using FrontendService.Extensions;

using Microsoft.AspNetCore.Mvc;

using Author = FrontendService.Contracts.Author;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrontendService.Controllers
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class AuthorsControllerGrpc : ControllerBase
    {
        private readonly AuthorsServiceProto.AuthorsServiceProtoClient authorsApiClient;

        public AuthorsControllerGrpc(AuthorsServiceProto.AuthorsServiceProtoClient authorsApiClient)
        {
            this.authorsApiClient = authorsApiClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Author>> Get(CancellationToken cancellationToken) =>
            (await this.authorsApiClient.GetAuthorsAsync(new GetAuthorsRequest(), cancellationToken: cancellationToken))
            .Authors.Select(dto => dto.ToContract());

        [HttpGet("{id}")]
        public async Task<Author> GetAuthor(Guid id, CancellationToken cancellationToken) =>
            (await this.authorsApiClient.GetAuthorAsync(new GetAuthorRequest {Id = id.ToString()},
                cancellationToken: cancellationToken)).Author.ToContract();

        [HttpPost]
        public async Task<Guid> CreateAuthor(CreateAuthor message, CancellationToken cancellationToken) =>
            Guid.Parse((await this.authorsApiClient.CreateAuthorAsync(
                new CrateAuthorRequest {
                    Author = new AuthorsService.Grpc.Author {
                        NumberOfBooks = 0,
                        Id = "",
                        Age = message.Age,
                        Biography = message.Biography,
                        FirstName = message.FirstName,
                        LastName = message.LastName
                    }
                },
                cancellationToken: cancellationToken)).Id);
    }
}