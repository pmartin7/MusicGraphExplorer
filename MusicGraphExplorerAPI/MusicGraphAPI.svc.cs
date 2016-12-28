using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using MusicGraphExplorerAPI.DataContract;
using System.ServiceModel.Activation;

namespace MusicGraphExplorerAPI
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MusicGraphAPI : IMusicGraphAPI
    {
        /// <summary>
        /// Get the artist matching the provided spotifyId
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public GetArtistResponse GetArtistForSpotifyId(string spotifyId)
        {
            //create instance of data access layer to music graph store
            ////TODO : make dal instance a static property shared across all public methods of the web service
            DataAccess dal = DataAccess.Instance;

            //get artist from music graph store
            Artist a = dal.GetArtistForSpotifyId(spotifyId);
            GetArtistResponse response = new GetArtistResponse( dal.GetArtistForSpotifyId(spotifyId));
            return response;
        }

        /// <summary>
        /// Get all artists related to the artist matching the provided spotifyId
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public GetArtistAndRelatedArtistsResponse GetRelatedArtistsForSpotifyId(string spotifyId)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get related artists from music graph store
            Artist a = dal.GetRelatedArtistsForSpotifyId(spotifyId);
            GetArtistAndRelatedArtistsResponse response = new GetArtistAndRelatedArtistsResponse(a);
            return response;
        }

        /// <summary>
        /// Get all genres related to the genre matching the provided genre name
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public GetGenreAndRelatedGenresResponse GetRelatedGenresForGenre(string genre)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get related genres from music graph store
            Genre result = dal.GetRelatedGenresForGenre(genre);
            GetGenreAndRelatedGenresResponse response = new GetGenreAndRelatedGenresResponse(result);
            return response;
        }


        /// <summary>
        /// Get all artists classified in a genre matching the provided genre name
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        public GetGenreAndRelatedArtistsResponse GetArtistsForGenre(string genre)
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get artists from the musioc graph store
            List<Genre> genres = dal.GetAllArtistsForGenres(
                new List<Genre>() { new Genre() { Name = genre } });

            GetGenreAndRelatedArtistsResponse response = new GetGenreAndRelatedArtistsResponse();

            if (null != genres)
            {
                response = new GetGenreAndRelatedArtistsResponse(genres[0]);
            }

            return response;
        }

        /// <summary>
        /// Get All Genres in the music graph store
        /// </summary>
        /// <returns>list of Genres</returns>
        public List<GetGenreResponse> GetAllGenres()
        {
            //create instance of data access layer to music graph store
            DataAccess dal = DataAccess.Instance;

            //get related genres from music graph store
            List<Genre> genres = new List<Genre>();
            genres = dal.GetAllGenres();

            List<GetGenreResponse> response = new List<GetGenreResponse>();
            foreach (Genre genre in genres)
            {
                response.Add(new GetGenreResponse(genre));
            }

            return response;
        }

        /// <summary>
        /// Search all artists which match the name substring passed in
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<SearchArtistResponse> SearchArtistByName(string name)
        {
            //if input is null, return empty list
            if (null == name)
            {
                return new List<SearchArtistResponse>();
            }

            DataAccess dal = DataAccess.Instance;

            List<Artist> artists = new List<Artist>();
            artists = dal.SearchArtistByName(name);

            List<SearchArtistResponse> response = new List<SearchArtistResponse>();
            foreach (Artist a in artists)
            {
                response.Add(new SearchArtistResponse(a));
            }

            return response;
        }

        /// <summary>
        /// Computes the most relevant artist paths to go from one artist to another artist. The paths do not account for directionality of relationship between artists
        /// </summary>
        /// <param name="fromSpotifyId">starting artist spotifyId</param>
        /// <param name="toSpotifyId">destination artist spotifyId</param>
        /// <param name="pageSize">number of paths to return</param>
        /// <returns>a list of paths. Each path is a list of artist. The relevance of artists within each path is the relevance of the relationship with the previous artist in the list</returns>
        public HashSet<List<GetRelatedArtistResponse>> GetPathsBetweenArtists(string fromSpotifyId, string toSpotifyId, int pageSize)
        {
            DataAccess dal = DataAccess.Instance;

            List<List<Artist>> paths = new List<List<Artist>>();
            paths = dal.GetPathsBetweenArtists(fromSpotifyId, toSpotifyId, pageSize);

            HashSet<List<GetRelatedArtistResponse>> response = new HashSet<List<GetRelatedArtistResponse>>(new Comparers.RelatedArtistListComparer());

            foreach (List<Artist> path in paths)
            {
                List<GetRelatedArtistResponse> responsepath = new List<GetRelatedArtistResponse>();

                foreach (Artist a in path)
                {
                    responsepath.Add(new GetRelatedArtistResponse(a));
                }

                response.Add(responsepath);
            }

            return response;
        }

    }
}
