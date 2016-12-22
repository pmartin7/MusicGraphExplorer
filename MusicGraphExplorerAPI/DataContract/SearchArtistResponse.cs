using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphExplorerAPI.DataContract
{
    /// <summary>
    /// Detailed artist data return when searching for an artist
    /// </summary>
    [DataContract]
    public class SearchArtistResponse
    {
        #region Properties
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public int Popularity { get; set; }

        [DataMember(Order = 2)]
        public string SpotifyUrl { get; set; }

        [DataMember(Order = 3)]
        public string SpotifyId { get; set; }

        #endregion

        #region Constructor

        public SearchArtistResponse(Artist artist)
        {
            //construct the list of Genres
            if (null != artist.Name) this.Name = artist.Name;
            this.Popularity = artist.Popularity;
            if (null != artist.SpotifyId)
            {
                this.SpotifyId = artist.SpotifyId;
                this.SpotifyUrl = Helper.GetSpotifyUrlFromSpotifyId(this.SpotifyId);
            }
        }
        #endregion

    }
}