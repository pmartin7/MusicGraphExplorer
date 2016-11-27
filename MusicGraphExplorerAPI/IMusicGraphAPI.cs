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

    }
}
