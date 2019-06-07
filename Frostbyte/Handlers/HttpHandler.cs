﻿using Frostbyte.Entities.Enums;
using Frostbyte.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed class HttpHandler
    {
        private static Lazy<HttpHandler> _lazyInstance
            = new Lazy<HttpHandler>(new HttpHandler());

        public static HttpHandler Instance = _lazyInstance.Value;

        private string Url { get; set; }

        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cancellationToken;

        public HttpHandler()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Frostbyte");
            _cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(7));
        }

        public HttpHandler WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public HttpHandler WithPath(string path)
        {
            Url = Url.WithPath(path);
            return this;
        }

        public HttpHandler WithParameter(string key, string value)
        {
            Url = Url.WithParameter(key, value);
            return this;
        }

        public HttpHandler WithCustomHeader(string key, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(key))
            {
                return this;
            }

            _client.DefaultRequestHeaders.Add(key, value);
            return this;
        }

        public async ValueTask<ReadOnlyMemory<byte>> GetBytesAsync(string url = default)
        {
            url = url ?? Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            var get = await _client.GetAsync(Url, _cancellationToken.Token).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogHandler<HttpHandler>.Log.RawLog(LogLevel.Error, get.ReasonPhrase, default);
                return default;
            }

            using var content = get.Content;
            var array = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Url = string.Empty;

            return new ReadOnlyMemory<byte>(array);
        }

        public async ValueTask<Stream> GetStreamAsync(string url = default)
        {
            url = url ?? Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            var get = await _client.GetAsync(Url, _cancellationToken.Token).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogHandler<HttpHandler>.Log.RawLog(LogLevel.Error, get.ReasonPhrase, default);
                return default;
            }

            using var content = get.Content;
            var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            Url = string.Empty;

            return stream;
        }

        public async ValueTask<string> GetStringAsync(string url = default)
        {
            url = url ?? Url;
            if (string.IsNullOrWhiteSpace(url))
                return default;

            var get = await _client.GetAsync(Url, _cancellationToken.Token).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
            {
                LogHandler<HttpHandler>.Log.RawLog(LogLevel.Error, get.ReasonPhrase, default);
                return default;
            }

            using var content = get.Content;
            var str = await content.ReadAsStringAsync().ConfigureAwait(false);
            Url = string.Empty;

            return str;
        }
    }
}