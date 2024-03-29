using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Core.System.Net.Http
{
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static void SetBasicAuthentication(
            this HttpClient httpClient,
            string username,
            string password)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        public static void SetBearerToken(
            this HttpClient httpClient,
            string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<T?> GetAsync<T>(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            HttpKeyValue? headers = default)
        {
            var content = await GetReadAsStringAsync(httpClient, uri, queryData, headers);
            return JsonSerializer.Deserialize<T>(content, Options);
        }

        public static async Task<string> GetStringAsync(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            HttpKeyValue? headers = default)
        {
            return await GetReadAsStringAsync(httpClient, uri, queryData, headers);
        }

        private static async Task<string> GetReadAsStringAsync(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            HttpKeyValue? headers = default)
        {
            uri = queryData is null ? uri : $"{uri}?{queryData.GetStringQueryData()}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            if (headers is not null)
            {
                Parallel.ForEach(headers, head => httpRequestMessage.Headers.Add(head.Key, head.Value));
            }

            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<T?> PostAsync<T>(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            object? formData = default,
            HttpKeyValue? headers = default)
        {
            var content = await PostReadAsStringAsync(httpClient, uri, queryData, formData, headers);
            return JsonSerializer.Deserialize<T>(content, Options);
        }

        public static Task<string> PostAsStringAsync(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            object? formData = default,
            HttpKeyValue? headers = default)
        {
            return PostReadAsStringAsync(httpClient, uri, queryData, formData, headers);
        }

        private static async Task<string> PostReadAsStringAsync(
            this HttpClient httpClient,
            string uri,
            HttpKeyValue? queryData = default,
            object? formData = default,
            HttpKeyValue? headers = default)
        {
            uri = queryData is null ? uri : $"{uri}?{queryData.GetStringQueryData()}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

            var content = new StringContent(JsonSerializer.Serialize(formData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            httpRequestMessage.Content = content;

            if (headers is not null)
            {
                Parallel.ForEach(headers, head => httpRequestMessage.Headers.Add(head.Key, head.Value));
            }

            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class HttpKeyValue : Collection<KeyValuePair<string, string>>
    {
        public string GetStringQueryData()
        {
            var keys = this.Select(kv =>
            {
                var key = WebUtility.UrlEncode(kv.Key);
                var value = WebUtility.UrlEncode(kv.Value);
                return $"{key}={value}";
            }).ToArray();

            return string.Join("&", keys);
        }
    }
}