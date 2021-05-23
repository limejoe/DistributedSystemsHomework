using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharedTypes.Api;

namespace FrontendService.ApiClients
{
    public class AuthorsApiClient
    {
        private readonly ILogger<AuthorsApiClient> logger;
        private readonly HttpClient httpClient;
        public const string Name = nameof(AuthorsApiClient);

        public AuthorsApiClient(IHttpClientFactory httpClientFactory, ILogger<AuthorsApiClient> logger)
        {
            this.logger = logger;
            this.httpClient = httpClientFactory.CreateClient(AuthorsApiClient.Name);
        }

        public async Task<AuthorDto[]> GetAuthorsAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.GetFromJsonAsync<AuthorDto[]>("", cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<AuthorDto> GetAuthorAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.GetFromJsonAsync<AuthorDto>(id.ToString(), cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<AuthorDto> CreateAuthorAsync(AuthorDto authorDto, CancellationToken cancellationToken)
        {
            try
            {
                var message = await this.httpClient.PostAsJsonAsync("", authorDto, cancellationToken);
                await using var stream = await message.Content.ReadAsStreamAsync(cancellationToken);
                var deserializeAsync = await JsonSerializer.DeserializeAsync<AuthorDto>(stream,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true}, cancellationToken);
                return deserializeAsync;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> UpdateAuthorAsync(AuthorDto authorDto,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.PutAsJsonAsync(authorDto.Id.ToString(), authorDto, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> UpdateAuthorBooksCountAsync(UpdateBooksCount updateBooksCount,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.PutAsJsonAsync($"{updateBooksCount.AuthorId}/bookscount",
                    updateBooksCount, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}