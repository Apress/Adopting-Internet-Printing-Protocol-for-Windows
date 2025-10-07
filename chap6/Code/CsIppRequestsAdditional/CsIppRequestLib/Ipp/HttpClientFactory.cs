using System;
using System.Collections.Concurrent;
using System.Net.Http;


namespace CsIppRequestLib
{
    public static class HttpClientFactory
    {
        private static readonly ConcurrentDictionary<string, HttpClient> HttpClientPool = new ConcurrentDictionary<string, HttpClient>();

        public static HttpClient GetHttpClient(string printerUri)
        {
            // Determine if the URL requires encryption (starts with https)
            bool isEncrypted = printerUri.StartsWith("https", StringComparison.OrdinalIgnoreCase);
            var key = isEncrypted ? "Encrypted" : "Default";

            return HttpClientPool.GetOrAdd(key, _ =>
            {
                var handler = new HttpClientHandler();
                if (isEncrypted)
                {
                    // Trust all certificates (not recommended in production)
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                }
                return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            });
        }
    }

}
