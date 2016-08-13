using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicGraphStore.GraphDataModel;
using MusicGraphStore.DataAccessLayer;

namespace MusicGraphStore.Test
{
    
    [TestClass]
    public class DALWriteTests
    {

        [TestMethod]
        public void InsertOrUpdatedRelatedGenresForGenre_CompletesSuccessfully()
        {
            Genre g1 = new Genre { Name = "industrial" };
            Genre item = new Genre()
            {
                Name = "industrial rock",
                Relevance = 42
            };
            g1.RelatedGenres.Add(item);

            try
            {
                DataAccess dal = DataAccess.Instance;
                dal.InsertOrUpdateRelatedGenresForGenre(g1);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void InsertOrUpdateRelatedArtistsForArtists_CompletesSuccessfully()
        {

            Artist a1 = new Artist
            {
                SpotifyId = "0X380XXQSNBYuleKzav5UO",
                Name = "Nine Inch Nails",
                Popularity=65
            };

            Artist a2 = new Artist
            {
                SpotifyId = "4uYwLU7k03RCQSRXGtQGg0",
                Name = "Orgy",
                Popularity = 45,
                Relevance = float.Parse("41")
            };

            a1.RelatedArtists.Add(a2);

            try
            {
                DataAccess dal = DataAccess.Instance;
                dal.InsertOrUpdateRelatedArtistsForArtist(a1);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
