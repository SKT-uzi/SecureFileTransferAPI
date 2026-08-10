using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ResumableFileTransfer.Common
{
    /// <summary>
    /// A simple HttpClient factory that creates and caches named HttpClient instances.
    /// Backwards-compatible helper API kept: Initialize, ApiClient and SASClient.
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly string _clsFullName = typeof(HttpClientHelper).FullName;

        // Cache of named clients to avoid creating many HttpClient instances.
        private static readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Legacy-compatible Initialize: creates (or returns existing) "api" and "sas" clients.
        /// </summary>
        public static void Initialize(
            string apiBaseAddress = null,
            string apiToken = null,
            int? apiTimeout = null,
            int? sasTimeout = null)
        {
            // Create or return named clients
            CreateClient("api", apiBaseAddress, apiToken, apiTimeout);
            CreateClient("sas", null, null, sasTimeout);
        }

        /// <summary>
        /// Create or get a named HttpClient. If a client with the same name already exists, it is returned unchanged.
        /// </summary>
        public static HttpClient CreateClient(string name, string baseAddress = null, string bearerToken = null, int? timeoutSeconds = null, HttpMessageHandler handler = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw BaseHelper.CreateException(_clsFullName, new ArgumentException("Client name must be provided.", nameof(name)));

            return _clients.GetOrAdd(name, _ =>
            {
                var client = handler != null ? new HttpClient(handler, disposeHandler: false) : new HttpClient();

                if (!string.IsNullOrEmpty(baseAddress))
                    client.BaseAddress = new Uri(baseAddress);

                if (!string.IsNullOrEmpty(bearerToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                client.Timeout = timeoutSeconds.HasValue ? TimeSpan.FromSeconds(timeoutSeconds.Value) : TimeSpan.FromMinutes(5);

                return client;
            });
        }

        /// <summary>
        /// Get an existing named client. Throws if it does not exist.
        /// </summary>
        public static HttpClient GetClient(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw BaseHelper.CreateException(_clsFullName, new ArgumentException("Client name must be provided.", nameof(name)));

            if (_clients.TryGetValue(name, out var client))
                return client;

            throw BaseHelper.CreateException(_clsFullName, new InvalidOperationException($"HttpClient named '{name}' is not created. Call CreateClient or Initialize first."));
        }

        /// <summary>
        /// Try get an existing named client without throwing.
        /// </summary>
        public static bool TryGetClient(string name, out HttpClient client) => _clients.TryGetValue(name, out client);

        /// <summary>
        /// Remove and dispose a named client. Returns true if removed.
        /// </summary>
        public static bool RemoveClient(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (_clients.TryRemove(name, out var client))
            {
                try { client.Dispose(); } catch { /* swallow disposal exceptions */ }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dispose and clear all cached clients.
        /// </summary>
        public static void DisposeAll()
        {
            if (_clients.IsEmpty) return;
            foreach (var kv in _clients)
            {
                try { kv.Value.Dispose(); } catch { /* swallow */ }
            }
            _clients.Clear();
        }

        public static HttpClient ApiClient => GetClient("api");
        public static HttpClient SASClient => GetClient("sas");
    }
}
