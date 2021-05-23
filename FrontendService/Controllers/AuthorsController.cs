using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FrontendService.ApiClients;
using FrontendService.Contracts;
using FrontendService.Extensions;

using Microsoft.AspNetCore.Mvc;

using SharedTypes.Api;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrontendService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorsApiClient authorsApiClient;

        public AuthorsController(AuthorsApiClient authorsApiClient)
        {
            this.authorsApiClient = authorsApiClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Author>> Get(CancellationToken cancellationToken) =>
            (await this.authorsApiClient.GetAuthorsAsync(cancellationToken)).Select(dto => dto.ToContract());

        [HttpGet("{id}")]
        public async Task<Author> GetAuthor(Guid id, CancellationToken cancellationToken) =>
            (await this.authorsApiClient.GetAuthorAsync(id, cancellationToken)).ToContract();

        [HttpPost]
        public async Task<Guid> CreateAuthor(CreateAuthor message, CancellationToken cancellationToken) =>
            (await this.authorsApiClient.CreateAuthorAsync(
                new AuthorDto {
                    Age = message.Age,
                    Biography = message.Biography,
                    FirstName = message.FirstName,
                    Id = Guid.Empty,
                    LastName = message.LastName,
                    NumberOfBooks = 0
                }, cancellationToken)).Id;
    }
}