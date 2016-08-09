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

            //retrieve artist and genre graph from spotify and insert into the store
            //start with four seeds: Nine Inch Nails, The Naked and Famous, Jack Johnson, Saing Germain des Pres
            string nineInchNailsSeedSpotifyId = "0X380XXQSNBYuleKzav5UO";
            string TNFSpotifyId = "0oeUpvxWsC8bWS6SnpU8b9";
            string JJSpotifyId = "3GBPw9NK25X1Wt2OUvOwY3";
            string SGDPSpotifyId = "484sZUYmnRXN84zmk3GY1n";
            string TheDoorsSId = "22WZ7M8sxp5THdruNY3gXt";
            string BobMarleySId = "28raJmIB4blKdjookHKbBJ";
            string ParisComboSId = "5xDjKV6UvzyrI3RnwHq02G";
            string HerbieHancockSId = "2ZvrvbQNrHKwjT7qfGFFUW";
            string PhilCollinsSId = "4lxfqrEsLX6N1N4OCSkILp";
            string RamonesSId = "1co4F2pPNH8JjTutZkmgSm";
            string DaftPunkSId = "4tZwfgrHOc3mvqYlEYSvVi";
            string RachelPlattenSId = "3QLIkT4rD2FMusaqmkepbq";
            string Jamiroquai = "6J7biCazzYhU3gM9j1wfid";
            string IamSId = "56Q6weEROZ1RsVrTak8Bm7";
            string MelvinsSId = "6aVjo0xHSiuW5hkasoYSR3";
            string AhmadJamalSId = "0mj1trLDvMoZFkKVIobIbd";
            string FCKahunaSId = "1UQ5GQDdYPKgbIEn9sMiSg";
            string TheSugarcubesSId = "1G0Xwj8mza6b03iYkVdzDP";
            string REMSId = "4KWTAlx2RvbpseOGMEmROg";
            string AyoSId = "6OkX55UMCw4Hgc5HM4zr7K";
            string TheMohawksSId = "5EasC32GkrcTsCH1VF4xKy";
            string GeneralElektriksSId = "5ly9gfnncBXv77g2zuRG9m";
            string GorillazSId = "3AA28KZvwAUcZuOKwyblJQ";
            string TheCardigansSId = "1tqZaCwM57UFKjWoYwMLrw";
            string KeziahJonesSId = "7fkVKWnSaQNFwqrR62vsSo";
            string KronosQuartetSId = "6qU2x4UbHIEOLsz645esEI";
            string FerryCorstenSId = "2ohlvFf9PBsDELdRstPtlP";
            string BobSinclarSId = "5YFS41yoX0YuFY39fq21oN";
            string ManuChaoSId = "6wH6iStAh4KIaWfuhf0NYM";


            List<string> seeds = new List<string>();
            seeds.Add(nineInchNailsSeedSpotifyId);
            seeds.Add(TNFSpotifyId);
            seeds.Add(JJSpotifyId);
            seeds.Add(SGDPSpotifyId);
            seeds.Add(TheDoorsSId);
            seeds.Add(BobMarleySId);
            seeds.Add(ParisComboSId);
            seeds.Add(HerbieHancockSId);
            seeds.Add(PhilCollinsSId);
            //seeds.Add(RamonesSId);
            //seeds.Add(RachelPlattenSId);
            //seeds.Add(DaftPunkSId);
            //seeds.Add(Jamiroquai);
            //seeds.Add(IamSId);
            //seeds.Add(MelvinsSId);
            //seeds.Add(AhmadJamalSId);
            //seeds.Add(FCKahunaSId);
            //seeds.Add(TheSugarcubesSId);
            //seeds.Add(REMSId);
            //seeds.Add(AyoSId);
            //seeds.Add(TheMohawksSId);
            //seeds.Add(GeneralElektriksSId);
            //seeds.Add(GorillazSId);
            //seeds.Add(TheCardigansSId);
            //seeds.Add(KeziahJonesSId);
            //seeds.Add(KronosQuartetSId);
            //seeds.Add(FerryCorstenSId);
            //seeds.Add(BobSinclarSId);
            //seeds.Add(ManuChaoSId);

            int N = 1500;
            buildSpotifyGraph(dal, seeds, N);
            //buildSpotifyGraph_old(dal, seeds, N);

            //then post-process graph to compute related genres, and insert/update related genres and scores in the graph
            computeAndInsertRelatedGenres(dal);

            //then post-process and add a relevance score to artist-artist relations
            computeAndInsertRelatedArtists(dal);

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
        private static void computeAndInsertRelatedArtists(DataAccess dal)
        {
            //first get all artists
            List<MusicGraphStore.GraphDataModel.Artist> artists = dal.GetAllArtists();

            //then for each artist, compute related artists and relevance score
            //and finally insert relationship in the graph with the new relevance score
            foreach (var artist in artists)
            {
                MusicGraphStore.GraphDataModel.Artist a = dal.ComputeRelatedArtistRelevance(artist);
                dal.InsertOrUpdateRelatedArtistsForArtist(a);
                Console.WriteLine("inserted {0} relationship scores for artist: {1}", a.RelatedArtists.Count, a.Name);
            }
        }
        #endregion

    }
}
