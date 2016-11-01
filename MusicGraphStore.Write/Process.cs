using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.Write
{
    /// <summary>
    /// Collection of methods to process data to write in the music graph store
    /// </summary>
    public class Process
    {
        #region Public Process Methods
        
        #region Build Graph from Spotify Data
        /// <summary>
        /// Query Spotify for artists from a list of seeds, gt related artists and genres
        /// and insert into the Musig Graph Store. Then do the same for all related artists
        /// Build the queue of artists to retrieve and insert up until the queue length reaches N
        /// Then stop enqueuing new artists, but continue dequeuing
        /// </summary>
        /// <param name="seeds">list of spotifyIds to seed with</param>
        /// <param name="maxQueueSize">Maximum queue size</param>
        /// <param name="dal">Instance of the data access layer to the graph store</param>
        ////TODO: MAKE THIS METHOD MULTI-THREADED
        public static void BuildSpotifyGraph(DataAccess dal, List<string> seeds, int maxQueueSize)
        {
            ////declarations
            //SpotifyWebAPI.Artist artist = new Artist();
            //List<Artist> relatedArtists = new List<Artist>();
            Random rng = new Random();

            //maintain a list of artist spotifyId to query and insert
            //we will not use a queue because we will insert at random positions to avoid creating imbalances
            //enqueue the seed
            List<string> artistQueue = new List<string>();
            foreach (string sid in seeds)
            {
                artistQueue.Add(sid);
            }

            //Add artists up to maxQueueSize artists.
            //When we reach maxQueueSize, we will stop adding, but continue dequeueing
            //stopEnqueue tracks this condition
            bool stopEnqueue = false;

            //processedArtists will keep track of artists that exist
            List<string> processedArtists = new List<string>();

            //track the current spotifyId we iterate over
            string currentSpotifyId;

            try
            {
                while (artistQueue.Count > 0)
                {
                    currentSpotifyId = artistQueue[0];
                    artistQueue.RemoveAt(0);

                    //Query Spotify for the artist, related artists and genres and insert into graph
                    List<string> relatedArtists = InsertRelatedArtists(dal, currentSpotifyId);

                    //add the current artist to the dictionary of processed artists
                    ///TODO: figure out why this condition is needed. 
                    /// For some reason with Seed Nine Inch Nails, artist How to Destroy Angels is added twice
                    if (!processedArtists.Contains(currentSpotifyId))
                    {
                        processedArtists.Add(currentSpotifyId);
                    }

                    //add related artists to queue if the stop enqueue condition has not been reached
                    //verify that the artist has not been used in the past
                    if (!stopEnqueue)
                    {
                        foreach (string relatedArtistId in relatedArtists)
                        {
                            if (!processedArtists.Contains(relatedArtistId) && !artistQueue.Contains(relatedArtistId))
                            {
                                //insert at a random place, but do not insert within the seeds
                                int N = artistQueue.Count;
                                int k = rng.Next(Math.Min(seeds.Count, artistQueue.Count), N);
                                artistQueue.InsertRange(k, new List<string>() { relatedArtistId });
                            }
                        }
                    }

                    //evaluate stopEnqueue condition. Once it is reached once, stop evaluating the condition
                    if (!stopEnqueue && (artistQueue.Count > maxQueueSize))
                    {
                        stopEnqueue = true;
                    }

                    Console.WriteLine("Queue Size: {0} / {1} Artists", artistQueue.Count, maxQueueSize);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Compute and Update Genre-Genre Relationships        /// <summary>
        /// this method will get all genres, then compute all related genres for each genre
        /// then insert the relationships into the graph 
        /// </summary>
        /// <param name="dal"></param>
        ////TODO: Make this method multi-threaded 
        public static void ComputeAndInsertRelatedGenres(DataAccess dal)
        {
            try
            {
                //first get All Genres
                List<MusicGraphStore.GraphDataModel.Genre> genres = dal.GetAllGenres();
                System.Diagnostics.Debug.WriteLine("Updating relationship relevance for {0} genres", genres.Count);

                int n = 0;

                //then for each genre, compute the related genres, and insert
                foreach (Genre genre in genres)
                {
                    n++;
                    Genre response = dal.ComputeRelatedGenresForGenre(genre);
                    dal.InsertOrUpdateRelatedGenresForGenre(response);
                    //Console.WriteLine("updated {0} related genres for: {1}", response.RelatedGenres.Count, response.Name);
                    System.Diagnostics.Debug.WriteLine("{2}/{3} - updated {0} related genres for: {1}", response.RelatedGenres.Count, response.Name, n, genres.Count);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Compute and Update Artist-Artist Relationships
        /// <summary>
        /// This method will compute a relationship score for related artists, then insert the relationship score in the grap
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="pageSize">number of artists to process</param>
        /// <param name="pageStart">starting index of artists to process in the list of retrieved artists</param>
        public static void ComputeAndInsertRelatedArtists(DataAccess dal, int pageStart, int pageSize)
        {
            //first get all artists
            List<MusicGraphStore.GraphDataModel.Artist> artists = dal.GetAllArtists();

            if (pageSize == 0) { pageSize = artists.Count; }

            //then for each artist, compute related artists and relevance score
            //and finally insert relationship in the graph with the new relevance score
            for (int i = pageStart; i < Math.Min(artists.Count, pageStart + pageSize); i++)
            {
                MusicGraphStore.GraphDataModel.Artist a = dal.ComputeRelatedArtistRelevance(artists[i]);
                dal.InsertOrUpdateRelatedArtistsForArtist(a);
                Console.WriteLine("inserted {0} relationship scores for artist: {1}", a.RelatedArtists.Count, a.Name);
            }
        }

        /// <summary>
        /// This method will compute a relationship score for related artists, then insert the relationship score in the grap
        /// Using a multi-threaded implementation of the iteration
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="dal"></param>
        private static void computeAndInsertRelatedArtists_parallel(DataAccess dal, int pageStart, int pageSize)
        {
            //first get all artists
            List<MusicGraphStore.GraphDataModel.Artist> artists = dal.GetAllArtists();

            if (pageSize == 0) { pageSize = artists.Count; }

            //then for each artist, compute related artists and relevance score
            //and finally insert relationship in the graph with the new relevance score
            Object sync = new Object();

            Parallel.For(pageStart, Math.Min(artists.Count, pageStart + pageSize), i =>
            {
                MusicGraphStore.GraphDataModel.Artist a = dal.ComputeRelatedArtistRelevance(artists[i]);
                dal.InsertOrUpdateRelatedArtistsForArtist(a);
                Console.WriteLine("inserted {0} relationship scores for artist: {1}", a.RelatedArtists.Count, a.Name);
            });
        }
        #endregion

        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Insert or update artist, related artists, and genres of all artists
        /// </summary>
        /// <param name="dal">data access layer to use for the Music Graph Store</param>
        /// <param name="SpotifyId">spotifyId of the artist to insert or update</param>
        /// <returns>A list of SpotifyIds for the related Artists found a call to Spotify API</returns>
        private static List<string> InsertRelatedArtists(DataAccess dal, string SpotifyId)
        {
            SpotifyWebAPI.Artist artist = new SpotifyWebAPI.Artist();
            List<SpotifyWebAPI.Artist> relatedArtists = new List<SpotifyWebAPI.Artist>();
            List<string> result = new List<string>();

            try
            {
                //query spotify for the artist
                Console.WriteLine("Querying Spotify ...");
                Task.Run(async () =>
                {
                    artist = await SpotifyWebAPI.Artist.GetArtist(SpotifyId);
                    relatedArtists = await artist.GetRelatedArtists();
                }).Wait();
                Console.WriteLine("Retrieved {0} related artists for {1}", relatedArtists.Count, artist.Name);

                //construct the Artist object
                MusicGraphStore.GraphDataModel.Artist a = new MusicGraphStore.GraphDataModel.Artist()
                {
                    Name = artist.Name,
                    Popularity = artist.Popularity,
                    SpotifyId = artist.Id,
                };

                //add Genres and construct Genre objects
                foreach (string genre in artist.Genres)
                {
                    Genre g = new Genre() { Name = genre };
                    a.Genres.Add(g);
                }

                //then do the same for related artists
                foreach (SpotifyWebAPI.Artist ra in relatedArtists)
                {
                    //construct the Artist object
                    MusicGraphStore.GraphDataModel.Artist related = new MusicGraphStore.GraphDataModel.Artist()
                    {
                        Name = ra.Name,
                        Popularity = ra.Popularity,
                        SpotifyId = ra.Id,
                    };

                    //add Genres and construct Genre objects
                    foreach (string genre in ra.Genres)
                    {
                        Genre g = new Genre() { Name = genre };
                        related.Genres.Add(g);
                    }

                    //add to the list of related artists of a
                    a.RelatedArtists.Add(related);

                    //add to the result
                    result.Add(related.SpotifyId);
                }

                //insert into Graph
                dal.InsertOrUpdateRelatedArtistsForArtist(a, a.RelatedArtists);

                //return the list of spotifyIds of related artists
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return result;
            }
        }
        #endregion

    }
}
