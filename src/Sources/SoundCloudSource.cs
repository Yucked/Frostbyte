using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Results;
using Frostbyte.Misc;

namespace Frostbyte.Sources
{
    public sealed class SoundCloudSource : BaseSource
    {
        private const string
            BASE_URL = "https://api.soundcloud.com",
            CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087";

        public override async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = new SearchResponse();
            var url = string.Empty;

            switch (query)
            {
                case var q when Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    if (!q.Contains("sets"))
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", CLIENT_ID);

                        response.LoadType = LoadType.TrackLoaded;
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", CLIENT_ID);

                        response.LoadType = LoadType.PlaylistLoaded;
                    }

                    break;

                case var _ when !Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    url = BASE_URL
                        .WithPath("tracks")
                        .WithParameter("q", query)
                        .WithParameter("client_id", CLIENT_ID);

                    response.LoadType = LoadType.SearchResult;
                    break;
            }

            var bytes = await HttpFactory.GetBytesAsync(url)
                .ConfigureAwait(false);

            if (bytes.IsEmpty)
                return SearchResponse.WithError("SoundCloud didn't return any results.");

            switch (response.LoadType)
            {
                case LoadType.TrackLoaded:
                    var scTrack = JsonSerializer.Deserialize<SoundCloudTrack>(bytes.Span);
                    response.Tracks.Add(scTrack.ToTrackInfo);
                    break;

                case LoadType.PlaylistLoaded:
                    var scPly = JsonSerializer.Deserialize<SoundCloudPlaylist>(bytes.Span);
                    response.Playlist = scPly.ToPlaylistInfo;
                    foreach (var track in scPly.Tracks)
                        response.Tracks.Add(track.ToTrackInfo);

                    break;

                case LoadType.SearchResult:
                    var scTracks = JsonSerializer.Deserialize<IEnumerable<SoundCloudTrack>>(bytes.Span);
                    foreach (var track in scTracks)
                        response.Tracks.Add(track.ToTrackInfo);
                    break;
            }

            return response;
        }

        public override async ValueTask<Stream> GetStreamAsync(string trackId)
        {
            var bytes = await HttpFactory
                .WithUrl(BASE_URL)
                .WithPath("tracks")
                .WithPath(trackId)
                .WithPath("stream")
                .WithParameter("client_id", CLIENT_ID)
                .GetBytesAsync()
                .ConfigureAwait(false);

            if (bytes.IsEmpty)
                return default;

            var read = JsonSerializer.Deserialize<SoundCloudDirectUrl>(bytes.Span);
            var stream = await HttpFactory
                .WithUrl(read.Url)
                .GetStreamAsync()
                .ConfigureAwait(false);

            return stream;
        }
    }
}