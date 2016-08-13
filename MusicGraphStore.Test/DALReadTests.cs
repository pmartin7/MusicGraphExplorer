using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicGraphStore.GraphDataModel;
using MusicGraphStore.DataAccessLayer;
using System.Collections.Generic;

namespace MusicGraphStore.Test
{
    [TestClass]
    public class DALReadTests
    {

        #region Read methods Tests
        [TestMethod]
        public void GAFSID_ResponseContainsNameAndGenres()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response = dal.GetArtistForSpotifyId("4KWTAlx2RvbpseOGMEmROg"); //R.E.M.
                Assert.IsTrue((response.Genres.Count > 0) && (null != response.Name));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GRAFSID_ResponseContainsNameAndRelatedArtists()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response = dal.GetRelatedArtistsForSpotifyId("22WZ7M8sxp5THdruNY3gXt"); //The Doors
                Assert.IsTrue((response.RelatedArtists.Count > 0) && (null != response.Name));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [TestMethod]
        public void GAAFG_WithNullInput_ResponseNotEmpty()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                List<Genre> response = dal.GetAllArtistsForGenres(null);
                Assert.IsTrue(response.Count > 0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GAAFG_WithGenre_ResponseContainsGenre()
        {
            Genre g1 = new Genre { Name = "industrial" };
            Genre g2 = new Genre { Name = "pop" };
            List<Genre> genres = new List<Genre>();
            genres.Add(g1);
            genres.Add(g2);

            try
            {
                DataAccess dal = DataAccess.Instance;
                List<Genre> response = new List<Genre>();
                response = dal.GetAllArtistsForGenres(genres);
                var item1 = response.Find
                    (x => (x.Name == g1.Name));
                var item2 = response.Find
                    (x => (x.Name == g2.Name));
                Assert.IsTrue((null != item1)&&(null != item2));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GAG_ResponseContainsGenre()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                List<Genre> response = new List<Genre>();
                response = dal.GetAllGenres();
                Assert.IsTrue(response.Count > 0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GAA_ResponseContainsArtist()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                List<Artist> response = dal.GetAllArtists();
                Assert.IsTrue(response.Count > 0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GPBA_ResponseContainsArtistPathsAndArtistRelevance()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                List<List<Artist>> response = dal.GetPathsBetweenArtists("4KWTAlx2RvbpseOGMEmROg", "22WZ7M8sxp5THdruNY3gXt", 5);
                Assert.IsTrue(
                    (response.Count > 0)
                    &&
                    (null != response[0].Find( x => (x.Relevance > 0)))
                    );
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        #endregion

        #region Search methods Tests
        [TestMethod]
        public void SABN_ResponseContainsArtists()
        {
            try
            {
                DataAccess dal = DataAccess.Instance;
                List<Artist> response = dal.SearchArtistByName("nine");
                Assert.IsTrue((response.Count > 0) && (null != response[0].Name));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        #endregion

        #region Compute methods Tests
        [TestMethod]
        public void CRGG_ResponseContainsGenreAndScore()
        {
            Genre g1 = new Genre { Name = "industrial" };

            try
            {
                DataAccess dal = DataAccess.Instance;
                Genre response = dal.ComputeRelatedGenresForGenre(g1);
                var item1 = response.RelatedGenres.Find
                    (x => (x.Relevance > 10));
                Assert.IsTrue((null != item1));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void CRABRR_ResponseContainsArtistAndRelevance()
        {
            Artist a = new Artist { SpotifyId = "0X380XXQSNBYuleKzav5UO" };

            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response = dal.ComputeRelatedArtistsByRelationRelevance(a);
                var item1 = response.RelatedArtists.Find
                    (x => (x.Relevance > 0));
                Assert.IsTrue((null != item1));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void CAWCG_ResponseContainsArtistAndGenreCounts()
        {
            Artist a = new Artist { SpotifyId = "0oeUpvxWsC8bWS6SnpU8b9" };

            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response = dal.ComputeArtistsWithCommonGenres(a);
                var item1 = response.RelatedArtists.Find
                    (x => (x.CommonGenres > 0));
                Assert.IsTrue((null != item1) && response.TotalGenres>=0);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void CRAR_ResponseContainsArtistAndRelevance()
        {
            Artist a = new Artist { SpotifyId = "0X380XXQSNBYuleKzav5UO" };

            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response  = dal.ComputeRelatedArtistRelevance(a);
                var item1 = response.RelatedArtists.Find
                    (x => (x.Relevance > 0));
                Assert.IsTrue((null != item1));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void CRAR_RelevanceDiffersFromCRABRR()
        {
            Artist a = new Artist { SpotifyId = "0X380XXQSNBYuleKzav5UO" };

            try
            {
                DataAccess dal = DataAccess.Instance;
                Artist response1 = dal.ComputeRelatedArtistRelevance(a);

                Artist response2 = dal.ComputeRelatedArtistsByRelationRelevance(a);

                foreach (var item1 in response1.RelatedArtists)
                {
                    var item = response2.RelatedArtists.Find( x => (x.SpotifyId == item1.SpotifyId));
                    if (null == item) { Assert.Fail("mismatching list of artists"); }
                    if ((null != item) && (item.Relevance != item1.Relevance)) { Assert.IsTrue(true); break; }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        #endregion

    }
}
