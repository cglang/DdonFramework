using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Cache.Sqlite
{
    public class SqliteCache : ICache
    {
        private static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly string _connectionString;
        private readonly JsonSerializerOptions _serializerOptions;

        public SqliteCache(SqliteCacheOptions options)
        {
            _connectionString = options.ConnectionString;
            _serializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "CREATE TABLE IF NOT EXISTS CacheEntries (" +
                "Key TEXT PRIMARY KEY," +
                "Value BLOB NOT NULL," +
                "ExpiresAt INTEGER NULL," +
                "SlidingWindow INTEGER NULL)";
            command.ExecuteNonQuery();
        }

        public byte[] Get(string key) => GetAsync(key).GetAwaiter().GetResult();

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(token);

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT Value, SlidingWindow FROM CacheEntries " +
                "WHERE Key = @Key AND (ExpiresAt IS NULL OR ExpiresAt > @Now)";
            command.Parameters.Add(new SqliteParameter("@Key", key));
            command.Parameters.Add(new SqliteParameter("@Now", UnixNow()));

            using var reader = await command.ExecuteReaderAsync(token);
            if (!await reader.ReadAsync(token)) return null;

            var bytes = (byte[])reader[0];

            if (!reader.IsDBNull(1))
            {
                var windowSeconds = reader.GetInt32(1);

                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE CacheEntries SET ExpiresAt = @ExpiresAt WHERE Key = @Key";
                updateCmd.Parameters.Add(new SqliteParameter("@ExpiresAt", UnixNow() + windowSeconds));
                updateCmd.Parameters.Add(new SqliteParameter("@Key", key));
                await updateCmd.ExecuteNonQueryAsync(token);
            }

            return bytes;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => SetAsync(key, value, options).GetAwaiter().GetResult();

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            => SetInternalAsync(key, value, options, token);

        public Task SetAsync<TItem>(string key, TItem value, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions);
            return SetInternalAsync(key, bytes, null, token);
        }

        public Task SetAsync<TItem>(string key, TItem value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions);
            return SetInternalAsync(key, bytes, options, token);
        }

        private async Task SetInternalAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            long? expiresAt = null;
            int? slidingWindow = null;
            var now = UnixNow();

            if (options != null)
            {
                if (options.AbsoluteExpiration.HasValue)
                    expiresAt = ToEpochSeconds(options.AbsoluteExpiration.Value);

                if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    var candidate = now + (long)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds;
                    if (!expiresAt.HasValue || candidate < expiresAt.Value)
                        expiresAt = candidate;
                }

                if (options.SlidingExpiration.HasValue)
                {
                    slidingWindow = (int)options.SlidingExpiration.Value.TotalSeconds;
                    var candidate = now + slidingWindow;
                    if (!expiresAt.HasValue || candidate < expiresAt.Value)
                        expiresAt = candidate;
                }
            }

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(token);

            using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT OR REPLACE INTO CacheEntries (Key, Value, ExpiresAt, SlidingWindow) " +
                "VALUES (@Key, @Value, @ExpiresAt, @SlidingWindow)";

            command.Parameters.Add(new SqliteParameter("@Key", key));
            command.Parameters.Add(new SqliteParameter("@Value", value));
            command.Parameters.Add(new SqliteParameter("@ExpiresAt", (object)expiresAt ?? DBNull.Value));
            command.Parameters.Add(new SqliteParameter("@SlidingWindow", (object)slidingWindow ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(token);
        }

        public void Refresh(string key) => RefreshAsync(key).GetAwaiter().GetResult();

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(token);

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT SlidingWindow FROM CacheEntries " +
                "WHERE Key = @Key AND ExpiresAt IS NOT NULL AND ExpiresAt > @Now";
            command.Parameters.Add(new SqliteParameter("@Key", key));
            command.Parameters.Add(new SqliteParameter("@Now", UnixNow()));

            var result = await command.ExecuteScalarAsync(token);
            if (result is DBNull || result is null) return;

            var windowSeconds = Convert.ToInt32(result);

            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE CacheEntries SET ExpiresAt = @ExpiresAt WHERE Key = @Key";
            updateCmd.Parameters.Add(new SqliteParameter("@ExpiresAt", UnixNow() + windowSeconds));
            updateCmd.Parameters.Add(new SqliteParameter("@Key", key));
            await updateCmd.ExecuteNonQueryAsync(token);
        }

        public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return RemoveCoreAsync(key, token);
        }

        public Task RemoveAsync(string[] keys, CancellationToken token = default)
        {
            return Task.WhenAll(keys.Select(key => RemoveCoreAsync(key, token)));
        }

        public async Task<TItem> GetAsync<TItem>(string key, CancellationToken token = default)
        {
            var bytes = await GetAsync(key, token);
            if (bytes is null) return default;
            return JsonSerializer.Deserialize<TItem>(bytes, _serializerOptions);
        }

        public async Task<bool> ContainsKeyAsync(string key, CancellationToken token = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(token);

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(1) FROM CacheEntries " +
                "WHERE Key = @Key AND (ExpiresAt IS NULL OR ExpiresAt > @Now)";
            command.Parameters.Add(new SqliteParameter("@Key", key));
            command.Parameters.Add(new SqliteParameter("@Now", UnixNow()));

            var count = (long)await command.ExecuteScalarAsync(token);
            return count > 0;
        }

        private async Task RemoveCoreAsync(string key, CancellationToken token = default)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(token);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CacheEntries WHERE Key = @Key";
            command.Parameters.Add(new SqliteParameter("@Key", key));
            await command.ExecuteNonQueryAsync(token);
        }

        private static long UnixNow()
        {
            return (long)(DateTime.UtcNow - s_epoch).TotalSeconds;
        }

        private static long ToEpochSeconds(DateTimeOffset dt)
        {
            return (long)(dt.UtcDateTime - s_epoch).TotalSeconds;
        }
    }
}
