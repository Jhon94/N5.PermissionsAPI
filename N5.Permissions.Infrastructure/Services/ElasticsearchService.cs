using Microsoft.Extensions.Logging;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using Nest;

namespace N5.Permissions.Infrastructure.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _client;
        private readonly ILogger<ElasticsearchService> _logger;
        private const string IndexName = "permissions";

        public ElasticsearchService(IElasticClient client, ILogger<ElasticsearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexPermissionAsync(Permission permission)
        {
            try
            {
                var permissionDocument = new
                {
                    permission.Id,
                    permission.EmployeeForename,
                    permission.EmployeeSurname,
                    FullName = $"{permission.EmployeeForename} {permission.EmployeeSurname}",
                    permission.PermissionTypeId,
                    PermissionTypeDescription = permission.PermissionType?.Description ?? "",
                    permission.PermissionDate,
                    permission.CreatedAt,
                    permission.UpdatedAt
                };

                var response = await _client.IndexAsync(permissionDocument, i => i
                    .Index(IndexName)
                    .Id(permission.Id));

                if (!response.IsValid)
                {
                    _logger.LogError("Failed to index permission {PermissionId}: {Error}",
                        permission.Id, response.OriginalException?.Message);
                    throw new Exception($"Failed to index permission: {response.OriginalException?.Message}");
                }

                _logger.LogInformation("Permission {PermissionId} indexed successfully", permission.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing permission {PermissionId}", permission.Id);
                throw;
            }
        }

        public async Task UpdatePermissionAsync(Permission permission)
        {
            try
            {
                var permissionDocument = new
                {
                    permission.Id,
                    permission.EmployeeForename,
                    permission.EmployeeSurname,
                    FullName = $"{permission.EmployeeForename} {permission.EmployeeSurname}",
                    permission.PermissionTypeId,
                    PermissionTypeDescription = permission.PermissionType?.Description ?? "",
                    permission.PermissionDate,
                    permission.CreatedAt,
                    permission.UpdatedAt
                };

                var response = await _client.UpdateAsync<object>(permission.Id, u => u
                    .Index(IndexName)
                    .Doc(permissionDocument)
                    .DocAsUpsert());

                if (!response.IsValid)
                {
                    _logger.LogError("Failed to update permission {PermissionId}: {Error}",
                        permission.Id, response.OriginalException?.Message);
                    throw new Exception($"Failed to update permission: {response.OriginalException?.Message}");
                }

                _logger.LogInformation("Permission {PermissionId} updated successfully", permission.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", permission.Id);
                throw;
            }
        }

        public async Task DeletePermissionAsync(int permissionId)
        {
            try
            {
                var response = await _client.DeleteAsync<object>(permissionId, d => d
                    .Index(IndexName));

                if (!response.IsValid && response.Result != Result.NotFound)
                {
                    _logger.LogError("Failed to delete permission {PermissionId}: {Error}",
                        permissionId, response.OriginalException?.Message);
                    throw new Exception($"Failed to delete permission: {response.OriginalException?.Message}");
                }

                _logger.LogInformation("Permission {PermissionId} deleted successfully", permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", permissionId);
                throw;
            }
        }

        public async Task<IEnumerable<Permission>> SearchPermissionsAsync(string searchTerm)
        {
            try
            {
                var response = await _client.SearchAsync<Permission>(s => s
                    .Index(IndexName)
                    .Query(q => q
                        .MultiMatch(m => m
                            .Fields(f => f
                                .Field(p => p.EmployeeForename)
                                .Field(p => p.EmployeeSurname)
                                .Field("FullName")
                                .Field("PermissionTypeDescription"))
                            .Query(searchTerm)
                            .Fuzziness(Fuzziness.Auto))));

                if (!response.IsValid)
                {
                    _logger.LogError("Failed to search permissions: {Error}", response.OriginalException?.Message);
                    return Enumerable.Empty<Permission>();
                }

                return response.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching permissions");
                return Enumerable.Empty<Permission>();
            }
        }
    }
}
