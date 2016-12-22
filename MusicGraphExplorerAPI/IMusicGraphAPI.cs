using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MusicGraphStore.GraphDataModel;
using MusicGraphExplorerAPI.DataContract;

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
        GetArtistResponse GetArtistForSpotifyId(string spotifyId);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/search?n={name}",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<SearchArtistResponse> SearchArtistByName(string name);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/{spotifyId}/related",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        GetArtistAndRelatedArtistsResponse GetRelatedArtistsForSpotifyId(string spotifyId);

        [OperationContract]
        [WebGet(UriTemplate = "/artist/path?from={fromSpotifyId}&to={toSpotifyId}&size={pageSize}",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<List<GetRelatedArtistResponse>> GetPathsBetweenArtists(string fromSpotifyId, string toSpotifyId, int pageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/genre/{genre}/related",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        GetGenreAndRelatedGenresResponse GetRelatedGenresForGenre(string genre);

        [OperationContract]
        [WebGet(UriTemplate = "/genre/{genre}/artists",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        GetGenreAndRelatedArtistsResponse GetArtistsForGenre(string genre);

        [OperationContract]
        [WebGet(UriTemplate = "/genres",
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<GetGenreResponse> GetAllGenres();

    }
}
