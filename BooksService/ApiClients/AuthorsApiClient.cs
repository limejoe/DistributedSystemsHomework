using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharedTypes.Api;

namespace BooksService.ApiClients
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