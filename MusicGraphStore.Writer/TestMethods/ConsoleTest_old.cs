using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.TestMethods
{
    public static class ConsoleTest_old
    {
        #region Test Methods
        public static void insertArtistTest(DataAccess dal, string nineInchNailsSeedSpotifyId)
        {
            SpotifyWebAPI.Artist artist = new SpotifyWebAPI.Artist();

            try
            {
                //query spotify for the artist
                Console.WriteLine("Querying Spotify ...");
                Task.Run(async () =>
                {
                    artist = await SpotifyWebAPI.Artist.GetArtist(nineInchNailsSeedSpotifyId);
                }).Wait();
                Console.WriteLine("Retrieved artist: {0}", artist.Name);

                //construct the Artist object
                MusicGraphStore.GraphDataModel.Artist a = new MusicGraphStore.GraphDataModel.Artist()
                {
                    Name = artist.Name,
                    Popularity = artist.Popularity,
                    SpotifyId = artist.Id,
                };

                //insert into Graph
                dal.InsertOrUpdateArtist(a);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void readArtistTest(DataAccess dal, string nineInchNailsSeedSpotifyId)
        {
            try
            {
                MusicGraphStore.GraphDataModel.Artist a = dal.GetArtistForSpotifyId(nineInchNailsSeedSpotifyId);
                Console.WriteLine("Name: {0}, Popularity: {1}, Id: {2}", a.Name, a.Popularity, a.SpotifyId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void insertArtistandGenresTest(DataAccess dal, string nineInchNailsSeedSpotifyId)
        {
            SpotifyWebAPI.Artist artist = new SpotifyWebAPI.Artist();

            try
            {
                //query spotify for the artist
                Console.WriteLine("Querying Spotify ...");
                Task.Run(async () =>
                {
                    artist = await SpotifyWebAPI.Artist.GetArtist(nineInchNailsSeedSpotifyId);
                }).Wait();
                Console.WriteLine("Retrieved artist: {0}", artist.Name);

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

                //insert into Graph
                dal.InsertOrUpdateArtistAndGenres(a, a.Genres);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}
