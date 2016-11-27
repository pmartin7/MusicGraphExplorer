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
        /// <summary>
        /// Get the artist matching the provided spotifyId
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public Artist GetArtistForSpotifyId(string spotifyId)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get artist from music graph store
            return dal.GetArtistForSpotifyId(spotifyId);
        }

        /// <summary>
        /// Get all artists related to the artist matching the provided spotifyId
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public Artist GetRelatedArtistsForSpotifyId(string spotifyId)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get artist from music graph store
            return dal.GetRelatedArtistsForSpotifyId(spotifyId);
        }
    }

}
