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
            ////TODO : make dal instance a static property shared across all public methods of the web service
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

            //get related artists from music graph store
            return dal.GetRelatedArtistsForSpotifyId(spotifyId);
        }

        /// <summary>
        /// Get all genres related to the genre matching the provided genre name
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public Genre GetRelatedGenresForGenre(string genre)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get related genres from music graph store
            return dal.GetRelatedGenresForGenre(genre);
        }


        /// <summary>
        /// Get all artists classified in a genre matching the provided genre name
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        public Genre GetArtistsForGenre(string genre)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get artists from the musioc graph store
            List<Genre> genres = dal.GetAllArtistsForGenres(
                new List<Genre>() { new Genre() { Name = genre } });

            Genre response = new Genre() { Name = genre };

            if (null != genres)
            {
                response = genres[0];
            }

            return response;
        }

        /// <summary>
        /// Get All Genres in the music graph store
        /// </summary>
        /// <returns>list of Genres</returns>
        public List<Genre> GetAllGenres()
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get related genres from music graph store
            return dal.GetAllGenres();
        }

        /// <summary>
        /// Search all artists which match the name substring passed in
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Artist> SearchArtistByName(string name)
        {
            DataAccess dal = DataAccess.Instance;

            return dal.SearchArtistByName(name);
        }

        /// <summary>
        /// Computes the most relevant artist paths to go from one artist to another artist. The paths do not account for directionality of relationship between artists
        /// </summary>
        /// <param name="fromSpotifyId">starting artist spotifyId</param>
        /// <param name="toSpotifyId">destination artist spotifyId</param>
        /// <param name="pageSize">number of paths to return</param>
        /// <returns>a list of paths. Each path is a list of artist. The relevance of artists within each path is the relevance of the relationship with the previous artist in the list</returns>
        public List<List<Artist>> GetPathsBetweenArtists(string fromSpotifyId, string toSpotifyId, int pageSize)
        {
            DataAccess dal = DataAccess.Instance;

            return dal.GetPathsBetweenArtists(fromSpotifyId, toSpotifyId, pageSize);
        }

    }
}
