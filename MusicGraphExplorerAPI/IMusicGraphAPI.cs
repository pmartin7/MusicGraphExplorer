using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MusicGraphStore.GraphDataModel;

namespace MusicGraphExplorerAPI
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMusicGraphAPI
    {
        [OperationContract]
        [WebGet(UriTemplate ="/artist/{spotifyId}", 
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Artist GetArtistForSpotifyId(string spotifyId);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/search?n={name}",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<Artist> SearchArtistByName(string name);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/{spotifyId}/related",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Artist GetRelatedArtistsForSpotifyId(string spotifyId);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/path?from={fromSpotifyId}&to={toSpotifyId}&size={pageSize}",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<List<Artist>> GetPathsBetweenArtists(string fromSpotifyId, string toSpotifyId, int pageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/genre/{genre}/related",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Genre GetRelatedGenresForGenre(string genre);

        [OperationContract]
        [WebGet(UriTemplate = "/genre/{genre}/artists",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Genre GetArtistsForGenre(string genre);

        [OperationContract]
        [WebGet(UriTemplate = "/genres",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<Genre> GetAllGenres();

    }
}
