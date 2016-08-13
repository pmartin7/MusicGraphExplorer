using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sample application to retrieve the shortest, most relevant path from an artist to another artist");
            Console.WriteLine("Press [ENTER] to start ...");
            Console.ReadLine();

            //initiate data access layer
            DataAccess dal = DataAccess.Instance;

            //find origin Artist
            List<Artist> fromArtists = new List<Artist>();

            try {
                while (fromArtists.Count == 0)
                {
                    Console.WriteLine("Name of the artist to start from:");
                    string fromName = Console.ReadLine();
                    fromArtists = dal.SearchArtistByName(fromName);
                    if (fromArtists.Count == 0) {
                        Console.WriteLine("no matching artist\n");
                    }
                    else
                    {
                        Console.WriteLine("Starting from Artist: {0}\n", fromArtists[0].Name);
                    }
                }

                //find destination Artist
                List<Artist> toArtists = new List<Artist>();

                while (toArtists.Count == 0)
                {
                    Console.WriteLine("Name of the artist to go to:");
                    string toName = Console.ReadLine();
                    toArtists = dal.SearchArtistByName(toName);
                    if (toArtists.Count == 0)
                    {
                        Console.WriteLine("no matching artist\n");
                    }
                    else
                    {
                        Console.WriteLine("Going to Artist: {0}\n", toArtists[0].Name);
                    }
                }

                //search top 6 most relevant shortest paths between artists
                Console.WriteLine("\nSearching paths between artists...");
                List<List<Artist>> paths = dal.GetPathsBetweenArtists(fromArtists[0].SpotifyId, toArtists[0].SpotifyId, 6);

                //display path in console
                foreach (var path in paths)
                {
                    Console.WriteLine("\nNEW PATH");
                    foreach (Artist a in path)
                    {
                        Console.WriteLine("\t Artist({0}), Popularity({1}), RelevanceToPrevious({2:0.00})", a.Name, a.Popularity, a.Relevance);
                    }
                }

                Console.Read();
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.Read(); }
        }
    }
}
