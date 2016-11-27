using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;

namespace MusicGraphExplorerAPI
{
    public class MusicGraphAPI : IMusicGraphAPI
    {
        public Artist GetArtistForSpotifyId(string spotifyId)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get artist from music graph store
            return dal.GetArtistForSpotifyId(spotifyId);
        }
    }
}
