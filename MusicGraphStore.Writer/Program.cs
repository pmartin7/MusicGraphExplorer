using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyWebAPI;
using MusicGraphStore.GraphDataModel;
using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.TestMethods;

namespace MusicGraphStore.Writer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //declarations
            DataAccess dal = DataAccess.Instance;
            

            //load a set of constant seed srtists
            List<string> seeds = new List<string>();
            InitializeSeeds(seeds);

            //pull a a graph of related artists and genres from spotify and insert them into our graph store
            if (Properties.Settings.Default.BuildSpotifyGraph == true)
            {
                buildSpotifyGraph(dal, seeds, Properties.Settings.Default.N);
                //buildSpotifyGraph_old(dal, seeds, N);
            }

            //then post-process graph to compute related genres, and insert/update related genres and scores in the graph
            if (Properties.Settings.Default.ComputeGenreRelevance == true)
            {
                computeAndInsertRelatedGenres(dal);
            }

            //then post-process and add a relevance score to artist-artist relations
            if (Properties.Settings.Default.ComputeArtistRelationship == true)
            {
                computeAndInsertRelatedArtists_parallel(dal, 0, 0);
            }


            //performance test mode for multi-threaded processing
            if (Properties.Settings.Default.PerfTest == true)
            {
                //warm up - throw away round
                computeAndInsertRelatedArtists(dal, 0, 3);
                
                //single threaded
                DateTime start = DateTime.Now;
                computeAndInsertRelatedArtists(dal, 0, 10);
                DateTime end = DateTime.Now;

                //multi-threaded
                DateTime startm = DateTime.Now;
                computeAndInsertRelatedArtists_parallel(dal, 0, 10);
                DateTime endm = DateTime.Now;
                
                //display test results
                Console.WriteLine("Singlethreaded runtime: {0}", end - start);
                Console.WriteLine("Multithreaded runtime: {0}", endm - startm);
            }

            Console.Read();

            #region old test methods - deprecate
            ////test inserting artist
            //Console.WriteLine("Starting Insert Artist Test");
            //ConsoleTest_old.insertArtistTest(dal, nineInchNailsSeedSpotifyId);
            //Console.WriteLine("Insert Artist Test Passed");

            ////test reading artist
            //Console.WriteLine("Starting Read Artist Test");
            //ConsoleTest_old.readArtistTest(dal, nineInchNailsSeedSpotifyId);
            //Console.WriteLine("Read Artist Test Passed");

            ////test inserting artist with genres
            //Console.WriteLine("Starting Insert Artist with Genres Test");
            //ConsoleTest_old.insertArtistandGenresTest(dal, nineInchNailsSeedSpotifyId);
            //Console.WriteLine("Insert Artist Test Passed");

            ////test inserting related artists to artist
            //Console.WriteLine("Starting Insert Artist with Genres Test");
            //InsertRelatedArtists(dal, nineInchNailsSeedSpotifyId);
            //Console.WriteLine("Insert Artist Test Passed");

            ////test reading artist
            //Console.WriteLine("Starting Read Artist Test");
            //ConsoleTest_old.readArtistTest(dal, nineInchNailsSeedSpotifyId);
            //Console.WriteLine("Read Artist Test Passed");
            #endregion

        }

        #region Helpers
        private static void InitializeSeeds(List<string> seeds)
        {
            seeds.Add(SeedArtists.nineInchNailsSeedSpotifyId);
            seeds.Add(SeedArtists.TNFSpotifyId);
            seeds.Add(SeedArtists.JJSpotifyId);
            seeds.Add(SeedArtists.SGDPSpotifyId);
            seeds.Add(SeedArtists.TheDoorsSId);
            seeds.Add(SeedArtists.BobMarleySId);
            seeds.Add(SeedArtists.ParisComboSId);
            seeds.Add(SeedArtists.HerbieHancockSId);
            seeds.Add(SeedArtists.PhilCollinsSId);

            if (Properties.Settings.Default.SeedingMode == SeedingMode.Full)
            {
                seeds.Add(SeedArtists.RamonesSId);
                seeds.Add(SeedArtists.RachelPlattenSId);
                seeds.Add(SeedArtists.DaftPunkSId);
                seeds.Add(SeedArtists.Jamiroquai);
                seeds.Add(SeedArtists.IamSId);
                seeds.Add(SeedArtists.MelvinsSId);
                seeds.Add(SeedArtists.AhmadJamalSId);
                seeds.Add(SeedArtists.FCKahunaSId);
                seeds.Add(SeedArtists.TheSugarcubesSId);
                seeds.Add(SeedArtists.REMSId);
                seeds.Add(SeedArtists.AyoSId);
                seeds.Add(SeedArtists.TheMohawksSId);
                seeds.Add(SeedArtists.GeneralElektriksSId);
                seeds.Add(SeedArtists.GorillazSId);
                seeds.Add(SeedArtists.TheCardigansSId);
                seeds.Add(SeedArtists.KeziahJonesSId);
                seeds.Add(SeedArtists.KronosQuartetSId);
                seeds.Add(SeedArtists.FerryCorstenSId);
                seeds.Add(SeedArtists.BobSinclarSId);
                seeds.Add(SeedArtists.ManuChaoSId);
            }
        }
        #endregion

        #region Build Spotify Graph
        /// <summary>
        /// Query Spotify for artists from a list of seeds, gt related artists and genres
        /// and insert into the Musig Graph Store. Then do the same for all related artists
        /// Build the queue of artists to retrieve and insert up until the queue length reaches N
        /// Then stop enqueuing new artists, but continue dequeuing
        /// </summary>
        /// <param name="seeds">list of spotifyIds to seed with</param>
        /// <param name="maxQueueSize">Maximum queue size</param>
        /// <param name="dal">Instance of the data access layer to the graph store</param>
        private static void buildSpotifyGraph(DataAccess dal, List<string> seeds, int maxQueueSize)
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
                                artistQueue.InsertRange(k, new List<string>() { relatedArtistId});
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
        
        /// DEPRECATE
        /// Old implementation based on queue.
        private static void buildSpotifyGraph_old(DataAccess dal, List<string> seeds, int maxQueueSize)
        {
            ////declarations
            //SpotifyWebAPI.Artist artist = new Artist();
            //List<Artist> relatedArtists = new List<Artist>();

            //maintain a queue of artist spotifyId to query and insert
            //enqueue the seedd
            Queue<string> artistQueue = new Queue<string>();
            foreach (string sid in seeds)
            { 
                artistQueue.Enqueue(sid);
            }

            //Enqueue up to maxQueueSize artists.
            //When we reach maxQueueSize, we will stop enqueuing, but continue dequeueing
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
                    currentSpotifyId = artistQueue.Dequeue();

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
                                artistQueue.Enqueue(relatedArtistId);
                            }
                        }
                    }

                    //evaluate stopEnqueue condition. Once it is reached once, stop evaluating the condition
                    if (!stopEnqueue && (artistQueue.Count > maxQueueSize))
                    {
                        stopEnqueue = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

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

        //this method will get all genres, then compute all related genres for each genre
        //then insert the relationships into the graph
        private static void computeAndInsertRelatedGenres(DataAccess dal)
        {
            try
            {
                //first get All Genres
                List<MusicGraphStore.GraphDataModel.Genre> genres = dal.GetAllGenres();

                //then for each genre, compute the related genres, and insert
                foreach (Genre genre in genres)
                {
                    Genre response = dal.ComputeRelatedGenresForGenre(genre);
                    dal.InsertOrUpdateRelatedGenresForGenre(response);
                    Console.WriteLine("updated {0} related genres for: {1}", response.RelatedGenres.Count, response.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        /// <summary>
        /// This method will compute a relationship score for related artists, then insert the relationship score in the grap
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="pageSize">number of artists to process</param>
        /// <param name="pageStart">starting index of artists to process in the list of retrieved artists</param>
        private static void computeAndInsertRelatedArtists(DataAccess dal, int pageStart, int pageSize)
        {
            //first get all artists
            List<MusicGraphStore.GraphDataModel.Artist> artists = dal.GetAllArtists();

            if (pageSize == 0) { pageSize = artists.Count; }

            //then for each artist, compute related artists and relevance score
            //and finally insert relationship in the graph with the new relevance score
            for (int i = pageStart; i < Math.Min(artists.Count, pageStart + pageSize); i++ )
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

    }
}
