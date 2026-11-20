using CompanyName.Application.Common.Constants;
using CompanyName.Application.Common.Extensions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Services
{
    internal sealed class LookupParameterService : ILookupParameterService
    {
        private const string MalayParametersCacheKey = "LookupParameterService_Parameters_my";
        private const string EnglishParametersCacheKey = "LookupParameterService_Parameters_en";

        private readonly ITemplateDbContext _context;
        private readonly IMemoryCache _cache;

        public LookupParameterService(ITemplateDbContext context,
                                      IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async ValueTask<IReadOnlyList<DropdownItem>> GetParametersAsync(string parameterName,
                                                                               string culture = "my",
                                                                               CancellationToken cancellationToken = default)
        {
            var parametersByName = string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase)
                ? await GetEnglishParametersByNameAsync(cancellationToken)
                : await GetMalayParametersByNameAsync(cancellationToken);

            parametersByName.TryGetValue(parameterName, out var items);
            items ??= Array.Empty<DropdownItem>();

            string selectPrompt = string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase)
                ? "-- Please Select --"
                : "-- Sila Pilih --";

            var result = new List<DropdownItem>
            {
                new DropdownItem { Text = selectPrompt, Value = string.Empty }
            };
            result.AddRange(items);
            return result;
        }

        public async ValueTask<IReadOnlyList<Gl_ParameterDescriptionResponse>> GetAllParametersAsync(CancellationToken cancellationToken = default)
        {
            string cacheKey = $"LookupParameterService_{nameof(GetAllParametersAsync)}";
            var results = await _cache.GetOrCreateExclusiveAsync<IReadOnlyList<Gl_ParameterDescriptionResponse>>(cacheKey, async entry =>
            {
                entry.AbsoluteExpiration = DateTime.Now.AddHours(8);
                return await _context.GlParametersDescriptions.AsNoTracking().Where(x => x.IsDeleted == 0)
                .Select(x => new Gl_ParameterDescriptionResponse(
                            x.ParameterTypeId,
                            x.ParameterId,
                            x.ParameterDescription,
                            x.Sort,
                            x.IsActive.Value
                        ))
                    .ToListAsync(cancellationToken);
            }, CacheEntry.BaseEntryOptions);

            return results ?? Array.Empty<Gl_ParameterDescriptionResponse>();
        }

        private async ValueTask<IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>>> GetMalayParametersByNameAsync(
            CancellationToken cancellationToken)
        {
            var results = await _cache.GetOrCreateExclusiveAsync<IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>>>(
                MalayParametersCacheKey,
                async entry =>
                {
                    entry.AbsoluteExpiration = DateTime.Now.AddHours(8);
                    var connection = await _context.GetOpenDbConnectionAsync(cancellationToken);
                    var rows = (await connection.QueryAsync<LookupParameterCacheRow>(MalayLookupSql)).AsList();
                    return BuildParametersByName(rows);
                },
                CacheEntry.BaseEntryOptions);

            return results ?? EmptyParametersByName;
        }

        private async ValueTask<IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>>> GetEnglishParametersByNameAsync(
            CancellationToken cancellationToken)
        {
            var results = await _cache.GetOrCreateExclusiveAsync<IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>>>(
                EnglishParametersCacheKey,
                async entry =>
                {
                    entry.AbsoluteExpiration = DateTime.Now.AddHours(8);
                    var connection = await _context.GetOpenDbConnectionAsync(cancellationToken);
                    var rows = (await connection.QueryAsync<LookupParameterCacheRow>(EnglishLookupSql)).AsList();
                    return BuildParametersByName(rows);
                },
                CacheEntry.BaseEntryOptions);

            return results ?? EmptyParametersByName;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>> BuildParametersByName(
            IReadOnlyList<LookupParameterCacheRow> rows)
        {
            return rows
                .Where(row => !string.IsNullOrEmpty(row.ParameterName))
                .GroupBy(row => row.ParameterName)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<DropdownItem>)group
                        .Select(row => new DropdownItem { Value = row.Value, Text = row.Text })
                        .ToList());
        }

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<DropdownItem>> EmptyParametersByName =
            new Dictionary<string, IReadOnlyList<DropdownItem>>();

        private sealed class LookupParameterCacheRow
        {
            public string ParameterName { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
        }

        private const string EnglishLookupSql = """
            SELECT
                b.parameter_description AS ParameterName,
                CAST(a.parameter_id AS NVARCHAR(100)) AS Value,
                a.parameter_description_en AS Text
            FROM gl_parameters a
            LEFT JOIN gl_parameters_description b ON a.parameter_type_id = b.parameter_type_id
            WHERE a.is_deleted = 0
              AND b.parameter_description IS NOT NULL
            """;

        private const string MalayLookupSql = """
            SELECT
                b.parameter_description AS ParameterName,
                CAST(a.parameter_id AS NVARCHAR(100)) AS Value,
                a.parameter_description AS Text
            FROM gl_parameters a
            LEFT JOIN gl_parameters_description b ON a.parameter_type_id = b.parameter_type_id
            WHERE a.is_deleted = 0
              AND b.parameter_description IS NOT NULL
            """;
    }
}
