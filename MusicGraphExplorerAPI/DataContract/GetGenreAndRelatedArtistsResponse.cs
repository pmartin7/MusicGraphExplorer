using MusicGraphStore.GraphDataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicGraphExplorerAPI.DataContract
{
    [DataContract]
    public class GetGenreAndRelatedArtistsResponse
    {

        #region properties
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public int ArtistCount { get; set; }

        [DataMember(Order = 2)]
        public List<GetRelatedArtistResponse> Artists { get; set; }

        #endregion

        #region Contructors

        public GetGenreAndRelatedArtistsResponse()
        {
            //construct the list of related artists
            Artists = new List<GetRelatedArtistResponse>();
        }

        public GetGenreAndRelatedArtistsResponse(Genre genre)
        {
            //construct the list of related artists
            Artists = new List<GetRelatedArtistResponse>();
            foreach (Artist a in genre.Artists)
            {
                Artists.Add(new GetRelatedArtistResponse(a));
            }

            if (null != Artists) this.ArtistCount = Artists.Count;

            this.Name = genre.Name;
        }
        
        #endregion
    }
}