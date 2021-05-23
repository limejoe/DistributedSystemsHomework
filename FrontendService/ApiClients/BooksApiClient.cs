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
    public class BooksApiClient
    {
        private readonly ILogger<BooksApiClient> logger;
        private readonly HttpClient httpClient;
        public const string Name = nameof(BooksApiClient);

        public BooksApiClient(IHttpClientFactory httpClientFactory, ILogger<BooksApiClient> logger)
        {
            this.logger = logger;
            this.httpClient = httpClientFactory.CreateClient(BooksApiClient.Name);
        }

        public async Task<BookDto[]> GetBooksAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.GetFromJsonAsync<BookDto[]>("", cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<BookDto> GetBookAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.GetFromJsonAsync<BookDto>(id.ToString(), cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<BookDto> CreateBookAsync(BookDto bookDto, CancellationToken cancellationToken)
        {
            try
            {
                var message = await this.httpClient.PostAsJsonAsync("", bookDto, cancellationToken);
                await using var stream = await message.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<BookDto>(stream,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true}, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> UpdateBookAsync(BookDto bookDto, CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.PutAsJsonAsync(bookDto.Id.ToString(), bookDto, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}