using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphExplorerAPI.DataContract
{
    [DataContract]
    public class GetArtistAndRelatedArtistsResponse
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

        [DataMember(Order = 4)]
        public List<GetRelatedArtistResponse> RelatedArtists { get; set; }

        [DataMember(Order = 5)]
        public int TotalGenres { get; set; }

        [DataMember(Order = 6)]
        public List<GetRelatedGenreResponse> Genres { get; set; }
        #endregion

        #region Constructor
        public GetArtistAndRelatedArtistsResponse(Artist artist)
        {
            //construct the list of Genres
            Genres = new List<GetRelatedGenreResponse>();
            foreach (Genre g in artist.Genres)
            {
                Genres.Add(new GetRelatedGenreResponse(g));
            }

            //construct the list of related artists
            RelatedArtists = new List<GetRelatedArtistResponse>();
            foreach (Artist a in artist.RelatedArtists)
            {
                RelatedArtists.Add(new GetRelatedArtistResponse(a));
            }

            if (null != Genres) this.TotalGenres = Genres.Count;

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