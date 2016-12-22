using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicGraphExplorerAPI
{
    internal static class Helper
    {
        internal static string GetSpotifyUrlFromSpotifyId(string spotifyId)
        {
            return string.Format("https://open.spotify.com/artist/{0}", spotifyId);
        }
    }
}