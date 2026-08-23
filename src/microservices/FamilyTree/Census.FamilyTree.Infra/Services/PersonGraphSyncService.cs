using Census.FamilyTree.Application.Services;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Census.FamilyTree.Infra.Services
{
    public class PersonGraphSyncService : IPersonGraphSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly IPersonFamilyTreeRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersonGraphSyncService(
            HttpClient httpClient,
            IPersonFamilyTreeRepository repository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;

            var baseUrl = configuration["Services:People"] ?? "http://people:8080";
            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        }

        public async Task SyncPersonSubtreeAsync(string personId, uint level, CancellationToken cancellationToken = default)
        {
            ForwardAuthorizationHeader();

            var synced = new HashSet<string>(StringComparer.Ordinal);
            await SyncPersonRecursiveAsync(personId, level, synced, cancellationToken);
        }

        private async Task SyncPersonRecursiveAsync(
            string personId,
            uint remainingDepth,
            HashSet<string> synced,
            CancellationToken cancellationToken)
        {
            if (!synced.Add(personId))
            {
                return;
            }

            var person = await FetchPersonAsync(personId, cancellationToken);
            if (person == null)
            {
                return;
            }

            await _repository.AddNode(ToNode(person));

            if (remainingDepth == 0)
            {
                return;
            }

            var nextDepth = remainingDepth - 1;

            if (!string.IsNullOrEmpty(person.FatherId))
            {
                await SyncPersonRecursiveAsync(person.FatherId, nextDepth, synced, cancellationToken);
            }

            if (!string.IsNullOrEmpty(person.MotherId))
            {
                await SyncPersonRecursiveAsync(person.MotherId, nextDepth, synced, cancellationToken);
            }

            var children = await FetchChildrenAsync(personId, cancellationToken);
            foreach (var child in children)
            {
                await SyncPersonRecursiveAsync(child.Id, nextDepth, synced, cancellationToken);
            }
        }

        private void ForwardAuthorizationHeader()
        {
            var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return;
            }

            if (AuthenticationHeaderValue.TryParse(authorization, out var header))
            {
                _httpClient.DefaultRequestHeaders.Authorization = header;
            }
        }

        private async Task<PersonRecord?> FetchPersonAsync(string personId, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync($"api/v1/person/{personId}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PersonRecord>(cancellationToken: cancellationToken);
        }

        private async Task<List<PersonRecord>> FetchChildrenAsync(string personId, CancellationToken cancellationToken)
        {
            var children = new List<PersonRecord>();
            var page = 1;
            const int maxPages = 100;

            while (page <= maxPages)
            {
                using var response = await _httpClient.GetAsync($"api/v1/person?page={page}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var pageResult = await response.Content.ReadFromJsonAsync<PeoplePage>(cancellationToken: cancellationToken);
                if (pageResult?.Items == null || !pageResult.Items.Any())
                {
                    break;
                }

                children.AddRange(pageResult.Items.Where(item =>
                    string.Equals(item.FatherId, personId, StringComparison.Ordinal) ||
                    string.Equals(item.MotherId, personId, StringComparison.Ordinal)));

                if (pageResult.Items.Count() < 10)
                {
                    break;
                }

                page++;
            }

            return children;
        }

        private static PersonFamilyTreeNode ToNode(PersonRecord person) =>
            new()
            {
                Id = person.Id,
                Name = person.Name,
                FatherId = NullIfEmpty(person.FatherId),
                MotherId = NullIfEmpty(person.MotherId),
            };

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private sealed class PeoplePage
        {
            public List<PersonRecord> Items { get; set; } = new();
        }

        private sealed class PersonRecord
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? FatherId { get; set; }
            public string? MotherId { get; set; }
        }
    }
}
