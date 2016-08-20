using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using MusicGraphStore.GraphDataModel;

namespace MusicGraphStore.DataAccessLayer
{
    /// <summary>
    /// Data Access Class for the Artist and Genre Graph data store
    /// The Data Access Layer is a singleton for the application
    /// </summary>
    public sealed class DataAccess
    {
        #region Properties
        IDriver driver;
        #endregion

        #region Singleton Constructor
        //allocate an instance of the data access singleton
        static readonly DataAccess _instance = new DataAccess();

        //publicly available instance of the data access object
        public static DataAccess Instance
        {
            get { return _instance; }
        }

        //private constructor
        private DataAccess()
        {
            driver = GraphDatabase.Driver(
                Properties.Settings.Default.neo4jSrv, 
                AuthTokens.Basic(Properties.Settings.Default.neo4jUsr, Properties.Settings.Default.neo4jPwd));
        }

        #endregion

        #region Public Read Methods

        /// <summary>
        /// Get an Artist metadata corresponding to the spotifyId provided, and its Genres
        /// Does not return related artists
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <exception cref="">Found more than one corresponding artist</exception>
        /// <returns></returns>
        ////TODO: Add relevance within genre
        public Artist GetArtistForSpotifyId(string spotifyId)
        {
            Artist artist = new Artist();

            using (var session = driver.Session())
            {
                try
                {
                    //verify if artist exists
                    var result = session.Run(
                            "MATCH (a:Artist {SpotifyId: {SpotifyId}})-[:IN_GENRE]->(g:Genre)"
                          + "RETURN a, g",
                            new Dictionary<string, object> { { "SpotifyId", spotifyId } });

                    foreach (var record in result)
                    {
                        //first record --- populate the artist
                        if (null == artist.SpotifyId)
                        {
                            artist = Helpers.DeserializeNode(record["a"].As<INode>(), artist);
                        }

                        //if more than one artist is found, throw an exception
                        if (record["a"].As<INode>().Properties["SpotifyId"].As<string>() != artist.SpotifyId)
                        {
                            throw new ArgumentOutOfRangeException("spotifyId", result, "Found multiple artist records for the provided SpotifyId");
                        }

                        //then populate genres
                        if (null != record["g"].As<INode>().Properties["Name"].As<string>())
                        {
                            artist.Genres.Add(Helpers.DeserializeNode(record["g"].As<INode>(), new Genre()));
                            artist.TotalGenres++;
                        }
                    }
                }
                catch (Exception e) { throw e; }
            }

            return artist;
        }

        /// <summary>
        /// Get an artist and related artists for the provided spotifyId
        /// Does not return genres
        /// </summary>
        /// <param name="spotifyId"></param>
        /// <returns></returns>
        public Artist GetRelatedArtistsForSpotifyId(string spotifyId)
        {
            Artist artist = new Artist();
            
            using (var session = driver.Session())
            {
                try
                {
                    artist = this.GetArtistForSpotifyId(spotifyId);

                    //get related artists for the artist
                    var result2 = session.Run(
                            "MATCH (a:Artist {SpotifyId:{SpotifyId}})-[r:RELATED]->(b:Artist)  "
                          + "RETURN DISTINCT b, r.Relevance as Relevance "
                          + "ORDER BY Relevance DESC ",
                            new Dictionary<string, object> { { "SpotifyId", spotifyId } });

                    foreach (var record in result2)
                    {
                        Artist item = Helpers.DeserializeNode(record["b"].As<INode>(), new Artist());
                        item = Helpers.DeserializeRecord(record, item);
                        artist.RelatedArtists.Add(item);
                    }
                }
                catch (Exception e) { throw e; }
            }

            return artist;
        }


        /// <summary>
        /// Return all artists in the graph
        /// </summary>
        /// <returns></returns>
        public List<Artist> GetAllArtists()
        {
            List<Artist> response = new List<Artist>();

            using (var session = driver.Session())
            {
                try
                {
                    //Get All Genres
                    var result = session.Run(
                            "MATCH (a:Artist)"
                          + "RETURN DISTINCT a");

                    foreach (var record in result)
                    {
                        response.Add(Helpers.DeserializeNode(record["a"].As<INode>(), new Artist()));
                    }
                }
                catch (Exception e) { throw e; }
            }

            return response;
        }

        /// <summary>
        /// Get all Artists classified by genres for the provided genres, ordered by popularity within each genre. 
        /// If input is empty, this method will return all artists and genres.
        /// The response can return the same artist for multiple genres.
        /// </summary>
        /// <param name="genres">List of genre names to retrieve artists from</param>
        /// <returns>List of new genres with ordered artists for each genre</returns>
        ////TODO: add Artist relevance within a Genre
        public List<Genre> GetAllArtistsForGenres(List<Genre> genres)
        {
            List<Genre> response = new List<Genre>();

            //first handle condition where input parameter is empty --- return all artists
            if ((null == genres) || (genres.Count == 0))
            {

                using (var session = driver.Session())
                {
                    try
                    {
                        //query graph db
                        var result = session.Run(
                                "MATCH (a:Artist)-->(g:Genre) "
                              + "RETURN g, a "
                              + "ORDER BY a.Popularity DESC");

                        //process records
                        //we have a record per each (artist, genre) tuple
                        foreach (var record in result)
                        {
                            //verify if response already contains genre
                            Genre item = response.Find
                                (x => (x.Name == record["g"].As<INode>().Properties["Name"].As<string>()));

                            if (null == item)
                            {
                                //genre is not already in the response, create the item and add to response
                                item = Helpers.DeserializeNode(record["g"].As<INode>(), new Genre());
                                item.Artists.Add(Helpers.DeserializeNode(record["a"].As<INode>(), new Artist()));
                                response.Add(item);

                            }

                            else
                            {
                                //the response already contains an item for the genre of the current artist record, add the artist to this item
                                item.Artists.Add(Helpers.DeserializeNode(record["a"].As<INode>(), new Artist()));
                            }
                        }
                    }
                    catch (Exception e) { throw e; }
                }

                return response;
            }

            //else query music graph db for each genre, process records to add to the response object, and return response
            foreach (Genre genre in genres)
            {
                using (var session = driver.Session())
                {
                    try
                    {
                        //query graph db
                        var result = session.Run(
                                "MATCH (a:Artist)-->(g:Genre {Name: {genre}}) "
                              + "RETURN DISTINCT a "
                              + "ORDER BY a.Popularity DESC",
                                new Dictionary<string, object> { { "genre", genre.Name } });

                        //process records
                        //we have one record per each artist tuple
                        Genre item = new Genre() { Name = genre.Name };

                        foreach (var record in result)
                        {
                            item.Artists.Add(Helpers.DeserializeNode(record["a"].As<INode>(), new Artist()));
                        }
                        response.Add(item);
                    }
                    catch (Exception e) { throw e; }
                }
            }

            return response;
        }

        /// <summary>
        /// Get All Genres in the music graph store
        /// </summary>
        /// <returns>list of Genres</returns>
        public List<Genre> GetAllGenres()
        {
            List<Genre> response = new List<Genre>();

            using (var session = driver.Session())
            {
                try
                {
                    //Get All Genres
                    var result = session.Run(
                            "MATCH (g:Genre)"
                          + "RETURN DISTINCT g");

                    foreach (var record in result)
                    {
                        response.Add(Helpers.DeserializeNode(record["g"].As<INode>(), new Genre()));
                    }
                }
                catch (Exception e) { throw e; }
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
        public List<List<Artist>> GetPathsBetweenArtists(string fromSpotifyId, string toSpotifyId, int pageSize)
        {
            List<List<Artist>> response = new List<List<Artist>>();

            using (var session = driver.Session())
            {
                try
                {
                    //solve Dijsktra: get all paths, then sort by "distance", that is by relevance of relationship between artists in the path
                    var result = session.Run(
                            "MATCH (from: Artist {SpotifyId: {spotifyId1 }}), (to: Artist {SpotifyId: {spotifyId2 }}) , " 
                          + "paths = allShortestPaths((from) -[:RELATED *]-(to)) "
                          + "WITH REDUCE(dist = 0, rel in [x in rels(paths) where x.Relevance IS NOT NULL] | dist + rel.Relevance) AS distance, paths "
                          + "RETURN paths, distance "
                          + "ORDER BY distance DESC "
                          + "LIMIT {pageSize} ",
                          new Dictionary<string, object> { { "spotifyId1", fromSpotifyId }, { "spotifyId2", toSpotifyId }, { "pageSize", pageSize } });

                    foreach (var record in result)
                    {
                        List<Artist> artistPath = new List<Artist>();
                        IPath path = record["paths"].As<IPath>();
                        int k = 0;
                        foreach (var node in path.Nodes)
                        {
                            Artist artist = Helpers.DeserializeNode(node, new Artist());

                            //not starting node --- provide the relevance of the relationship to the previous Artist
                            if (k>0)
                            {
                                artist.Relevance = float.Parse(path.Relationships[k-1]["Relevance"].As<string>());
                            }

                            artistPath.Add(artist);
                            k++;
                        }

                        response.Add(artistPath);

                    }
                }
                catch (Exception e) { throw e; }
            }

            return response;
        }
        #endregion

        #region Public Search Methods

        /// <summary>
        /// Search all artists whose name contains the provided string. Search is case insensitive.
        /// </summary>
        /// <param name="name">name to search for</param>
        /// <returns>list of matching artists</returns>
        ////TODO: modify query and neo4j configuration to do fuzzy matching of the string. This can be done using manual indexes and node_auto_index, or perhaps with solr.
        public List<Artist> SearchArtistByName(string name)
        {
            List<Artist> response = new List<Artist>();

            using (var session = driver.Session())
            {
                try
                {
                    string query = "MATCH (a:Artist) "
                          + "WHERE a.Name =~ \"(?i).*" + name + ".*\" "
                          + "RETURN a "
                          + "ORDER BY a.Popularity DESC";

                    //get matchin artists
                    var result = session.Run(query);

                    foreach (var record in result)
                    {
                        response.Add(Helpers.DeserializeNode(record["a"].As<INode>(), new Artist()));
                    }
                }
                catch (Exception e) { throw e; }
            }

            return response;
        }

        #endregion

        #region Public Write Methods
        /// <summary>
        /// Insert of update one artist in the Music Graph store
        /// </summary>
        /// <param name="a">Artist to insert</param>
        public void InsertOrUpdateArtist(Artist a)
        {
            using (var session = driver.Session())
            {
                try
                {
                    //either Match the existing artist by SpotifyId and update other properties
                    //or create the artist
                    var result = session.Run(
                            "MERGE (a:Artist {SpotifyId: {SpotifyId}})"
                          + "ON MATCH SET a.Name = {Name}, a.Popularity = {Popularity}, a.Url = {Url}"
                          + "ON CREATE SET a.Name = {Name}, a.Popularity = {Popularity}, a.Url = {Url}",
                            new Dictionary<string, object> {
                                { "SpotifyId", a.SpotifyId }, {"Name", a.Name }, {"Popularity", a.Popularity }, {"Url", a.Url } });
                }
                catch (Exception e) { throw e; }
            }
        }

        /// <summary>
        /// Insert or update a collection of Genres that an artist is related to in the Music Graph store
        /// This will insert the artist if the artist does not exist
        /// </summary>
        /// <param name="artist">artist to relate to the genres</param>
        /// <param name="Genres">list of genres for the artist (artist.Genres) </param>
        public void InsertOrUpdateArtistAndGenres(Artist a, List<Genre> genres)
        {
            using (var session = driver.Session())
            {
                try
                {
                    //either Match the existing artist by SpotifyId and update other properties
                    //or create the artist
                    session.Run(
                            "MERGE (a:Artist {SpotifyId: {SpotifyId}})"
                          + "ON MATCH SET a.Name = {Name}, a.Popularity = {Popularity}, a.Url = {Url} "
                          + "ON CREATE SET a.Name = {Name}, a.Popularity = {Popularity}, a.Url = {Url} ",
                            new Dictionary<string, object> {
                                { "SpotifyId", a.SpotifyId }, {"Name", a.Name }, {"Popularity", a.Popularity }, {"Url", a.Url } });


                    foreach (Genre genre in genres)
                    {
                        //either Match the existing genres by name 
                        //or create the genres 
                        session.Run(
                            "MERGE (g:Genre {Name: {Name}}) "
                          + "ON CREATE SET g.Name = {Name}",
                            new Dictionary<string, object> { { "Name", genre.Name } });

                        //either Match the existing artist relationship to Genre 
                        //or create the relationship
                        session.Run(
                            "MATCH (a:Artist {SpotifyId: {SpotifyId}}), (g:Genre {Name: {name}}) "
                          + "MERGE (a)-[:IN_GENRE]->(g)",
                            new Dictionary<string, object>
                            {
                                {"SpotifyId", a.SpotifyId }, {"name", genre.Name }
                            }
                            );
                    }
                }
                catch (Exception e) { throw e; }
            }
        }

        /// <summary>
        /// Insert or update a collection or Artists that an existing artist is related to in the Music Graph store
        /// the provided artist a will be inserted or updated in the Music Graph store
        /// </summary>
        /// <param name="a">Artist</param>
        /// <param name="artists">List of Artists related to a</param>
        public void InsertOrUpdateRelatedArtistsForArtist(Artist a, List<Artist> artists)
        {
            using (var session = driver.Session())
            {
                try
                {
                    //Insert or update a and his genres
                    this.InsertOrUpdateArtistAndGenres(a, a.Genres);


                    foreach (Artist artist in artists)
                    {
                        //insert or update related artist and his genres
                        this.InsertOrUpdateArtistAndGenres(artist, artist.Genres);

                        //either Match the existing a relationship to artist 
                        //or create the relationship
                        session.Run(
                            "MATCH (a:Artist {SpotifyId: {SpotifyId1}}), (b:Artist {SpotifyId: {SpotifyId2}}) "
                          + "MERGE (a)-[:RELATED]->(b)",
                            new Dictionary<string, object>
                            {
                                {"SpotifyId1", a.SpotifyId }, {"SpotifyId2", artist.SpotifyId }
                            }
                            );
                    }
                }
                catch (Exception e) { throw e; }
            }
        }

        /// <summary>
        /// Insert or update a collection or Artists that an existing artist is related to in the Music Graph store
        /// the provided artist a will be inserted or updated in the Music Graph store
        /// This overload variant will insert related artists and relevance score of each related artist
        /// directly pulled from a.RelatedArtists
        /// </summary>
        /// <param name="a">Artist</param>
        public void InsertOrUpdateRelatedArtistsForArtist(Artist a)
        {
            using (var session = driver.Session())
            {
                try
                {
                    //Insert or update a and his genres
                    this.InsertOrUpdateArtistAndGenres(a, a.Genres);


                    foreach (Artist artist in a.RelatedArtists)
                    {
                        //insert or update related artist and his genres
                        this.InsertOrUpdateArtistAndGenres(artist, artist.Genres);

                        //either Match the existing a relationship to artist 
                        //or create the relationship
                        session.Run(
                            "MATCH (a:Artist {SpotifyId: {SpotifyId1}}), (b:Artist {SpotifyId: {SpotifyId2}}) "
                          + "MERGE (a)-[r:RELATED]->(b) "
                          + "ON MATCH SET r.Relevance = {Relevance} "
                          + "ON CREATE SET r.Relevance = {Relevance} ",
                            new Dictionary<string, object>
                            {
                                {"SpotifyId1", a.SpotifyId }, {"SpotifyId2", artist.SpotifyId }, {"Relevance", artist.Relevance }
                            }
                            );
                    }
                }
                catch (Exception e) { throw e; }
            }
        }

        /// <summary>
        /// Insert or update a collection of related genres for a genre, with an associated relevance
        /// The relevance is expected to be computed from ComputeRelatedGenresForGenre
        /// The list of related genres is pulled from g.RelatedGenres
        /// </summary>
        /// <param name="g">Genre</param>
        /// <param name="r">response of ComputeRelatedGenresForGenre</param>
        public void InsertOrUpdateRelatedGenresForGenre(Genre g)
        {
            using (var session = driver.Session())
            {
                try
                {
                    foreach (Genre item in g.RelatedGenres)
                    {
                        //if the genre is the input, do nothing
                        if ( (item.Name != g.Name) && (null != item.Name))
                        {
                            //insert the genre if it does not exist
                            //either Match the existing genres by name 
                            //or create the genres 
                            session.Run(
                                "MERGE (g:Genre {Name: {Name}}) "
                              + "ON CREATE SET g.Name = {Name}",
                                new Dictionary<string, object> { { "Name", item.Name } });

                            //create the relationship if it does not exist
                            //otherwise update the score
                            session.Run(
                                "MATCH(g1: Genre { Name: {genre1}}), (g2: Genre { Name: {genre2}}) "
                              + "MERGE(g1)-[r: RELATED_TO_GENRE]->(g2) "
                              + "ON MATCH SET r.Score = {score} "
                              + "ON CREATE SET r.Score = {score} ",
                                new Dictionary<string, object> {
                                    { "genre1", item.Name }, { "genre2", g.Name }, {"score", item.Relevance }
                                });
                        }
                    }
                }
                catch (Exception e) { throw e; }
            }
        }

        #endregion

        #region Public Compute Methods
        /// <summary>
        /// Note: this method computes the score for genres related to a given genre
        /// Related genres are either other genres of artist in the given genre,
        /// or genres of artists related to artists in the given genre 
        /// </summary>
        /// <param name="genre">genre</param>
        /// <returns>a new Genre object with a refreshed list of RelatedGenres and their associated relevance</returns>
        ////TODO: Separate in an internal library
        public Genre ComputeRelatedGenresForGenre(Genre genre)
        {
            Genre response = new Genre()
            {
                Name = genre.Name,
                Artists = genre.Artists
            };

            using (var session = driver.Session())
            {
                try
                {
                    //query graph db
                    var result = session.Run(
                          "MATCH(a: Artist)-[:IN_GENRE]->(target:Genre { Name: {inputgenre}}) "
                        + "OPTIONAL MATCH (a) -[:IN_GENRE]->(g1:Genre) "
                        + "WITH g1 AS g1, count(distinct a) AS x "
                        + "WITH collect({ genre: g1.Name, score: 2 * x}) as rows "

                        + "MATCH(a: Artist)-[:IN_GENRE]->(target: Genre { Name: {inputgenre}}) "
                        + "OPTIONAL MATCH (a) -[:RELATED]-(b) -[:IN_GENRE]->(g2: Genre) "
                        + "WITH g2 AS g2, count(distinct b) AS y, rows AS rows "
                        + "WITH rows + collect({ genre: g2.Name, score: y}) AS allrows "

                        + "UNWIND allrows AS row "
                        + "WITH row.genre AS Name, row.score AS score "
                        + "RETURN distinct Name, sum(score) AS Relevance ORDER BY Relevance DESC ",
                          new Dictionary<string, object> { { "inputgenre", genre.Name } });

                    //process records
                    foreach (var record in result)
                    {
                        response.RelatedGenres.Add(Helpers.DeserializeRecord(record, new Genre()));
                    }
                }
                catch (Exception e) { throw e; }
            }

            return response;
        }

        /// <summary>
        /// Computes the related artists for a given artist passed in as an argument
        /// For each related artist, a relevance score is computed using a query to the graph db
        /// Based on the number of common related artists in each direction
        /// This method will be used to compute the final related artist relevance score in combination
        /// with the ComputeArtistsWithCommonGenres method
        /// </summary>
        /// <param name="artist">artist </param>
        /// <returns>a new artist object with a refreshed list of RelatedArtists. Each RelatedArtist has a relevance score</returns>
        public Artist ComputeRelatedArtistsByRelationRelevance(Artist artist)
        {
            //ensure that artist is not empty
            if (null == artist.SpotifyId) { throw new ArgumentNullException("artist does not have a spotifyId"); }

            //returning a new artist object as we will need to aggregate results with
            //the ComputeArtistsWithCommonGenres result
            Artist response = new Artist()
            {
                SpotifyId = artist.SpotifyId,
                Name = artist.Name,
                Popularity = artist.Popularity,
                Genres = artist.Genres
            };

            //Compute a first score based on related artists, and store in response temporarily
            using (var session = driver.Session())
            {
                try
                {
                    // common related artists ->: 0.35*n/(N+1)
                    // +common related artists < -: 0.35 * n / (N + 1)
                    // n is the number of common related artists in one specific direction, and N is the total number of artists
                    // related to the target artist provided as argument
                    var result = session.Run(
                            "MATCH (a:Artist {SpotifyId: {SpotifyId}})-[:RELATED]->(b) "
                          + "WITH count(distinct b) as TotalRel "

                          + "MATCH(b: Artist) < -[:RELATED]-(c:Artist)-[:RELATED]->(a: Artist {SpotifyId: {SpotifyId}}) "
                          + "WHERE(b) <-[:RELATED]-(a) "
                          + "WITH b as b, count(distinct c) as cnt, TotalRel AS TotalRel "
                          + "WITH collect({ artistId: b.SpotifyId, artistName: b.Name, artistPopularity: b.Popularity, score: cnt}) AS rows, TotalRel AS TotalRel "

                          + "MATCH(b: Artist) -[:RELATED]->(c: Artist) < -[:RELATED] - (a:Artist {SpotifyId: {SpotifyId}}) "
                          + "WHERE(b) <-[:RELATED]-(a) "
                          + "WITH rows as rows, b as b, count(distinct c) as cnt, TotalRel AS TotalRel "
                          + "WITH rows + collect({artistId: b.SpotifyId, artistName: b.Name, artistPopularity: b.Popularity, score: cnt}) AS allrows, TotalRel AS TotalRel "

                          + "UNWIND allrows as row "
                          + "WITH row.artistId AS artistId, row.artistName AS artistName, row.artistPopularity AS Popularity, row.score AS score, TotalRel AS TotalRel "
                          + "RETURN DISTINCT artistId, artistName, Popularity, avg(score) / TotalRel as score ORDER BY score desc ",
                            new Dictionary<string, object> { { "SpotifyId", artist.SpotifyId } });

                    //process results
                    foreach (var record in result)
                    {
                        Artist a = new Artist()
                        {
                            Name = record["artistName"].As<string>(),
                            SpotifyId = record["artistId"].As<string>(),
                            Popularity = record["Popularity"].As<int>(),
                            Relevance = record["score"].As<float>()
                        };
                        
                        response.RelatedArtists.Add(a);
                    }
                }
                catch (Exception e) { throw e; }
             }


            //then compute score based on genre, and add it to the previous score in the response

            return response;

         }

        /// <summary>
        /// Computes the artists with common genres for a given artist passed in as an argument
        /// For each artist with matching genres, the number of common genres is returned
        /// This method will be used to compute the final related artist relevance score in combination
        /// with the ComputeReltatedArtistsByRelationRelevance method
        /// </summary>
        /// <param name="artist">artist </param>
        /// <returns>a new artist objects with a list of related artists with common genres ordered by relevance score</returns>
        public Artist ComputeArtistsWithCommonGenres(Artist artist)
        {
            //ensure that artist is not empty
            if (null == artist.SpotifyId) { throw new ArgumentNullException("artist does not have a spotifyId"); }

            //returning a new artist object as we will need to aggregate results with
            //the ComputeArtistsWithCommonGenres result
            Artist response = new Artist()
            {
                SpotifyId = artist.SpotifyId,
                Name = artist.Name,
                Popularity = artist.Popularity,
                Genres = artist.Genres
            };

            //Compute a first score based on related artists, and store in response temporarily
            using (var session = driver.Session())
            {
                try
                {
                          var result = session.Run(
                              " MATCH(a: Artist { SpotifyId: {SpotifyId}})-[:IN_GENRE]->(g: Genre) "
                            + " WITH count(distinct g) as totalGenres "
                            + " MATCH(b: Artist) -[:IN_GENRE]->(g: Genre) < -[:IN_GENRE] - (a:Artist { SpotifyId: {SpotifyId}}) "
                            + " RETURN b.Name as Name, b.SpotifyId as Id, b.Popularity AS Popularity, count(distinct g) as commonGenres, totalGenres ORDER BY commonGenres ",
                            new Dictionary<string, object> { { "SpotifyId", artist.SpotifyId } });

                    //process results
                    foreach (var record in result)
                    {
                        Artist a = new Artist()
                        {
                            Name = record["Name"].As<string>(),
                            SpotifyId = record["Id"].As<string>(),
                            Popularity = record["Popularity"].As<int>(),
                            CommonGenres = record["commonGenres"].As<int>()
                        };

                        response.RelatedArtists.Add(a);
                        response.TotalGenres = record["totalGenres"].As<int>();
                    }
                }
                catch (Exception e) { throw e; }
            }

            return response;
        }

        /// <summary>
        /// For each related artist to a given artist,
        /// Computes the relationship relevance as A*ComputeRelatedArtistsByRelationRelevance + B*ComputeArtistsWithCommonGenres
        /// </summary>
        /// <param name="artist">artist </param>
        /// <returns>a new artist object with a list of related artists ordered by relevance score</returns>
        public Artist ComputeRelatedArtistRelevance(Artist artist)
        {
            //first get related artists, and determine temporary relevance score based on number of common related artists
            Artist response = ComputeRelatedArtistsByRelationRelevance(artist);

            //then get the list of artists with common genres, and number of common genres
            Artist commonGenreArtist = ComputeArtistsWithCommonGenres(artist);

            //then refine the relationship score using the common genres signal
            //this ensures that we only retrieve artists who are already explicitely related in the graph store
            foreach(Artist item in response.RelatedArtists)
            {
                Artist genreArtist =
                    commonGenreArtist.RelatedArtists.Find(x => (x.SpotifyId == item.SpotifyId));
                if (null != genreArtist)
                {
                    item.Relevance = float.Parse("0.85") * item.Relevance
                        + float.Parse("0.15") * (genreArtist.CommonGenres.As<float>() / (commonGenreArtist.TotalGenres.As<float>() + 1));
                }
                else
                {
                    item.Relevance = float.Parse("0.85") * item.Relevance;
                }
            }

            response.RelatedArtists.Sort((item1, item2) => (item2.Relevance.CompareTo(item1.Relevance)));

            return response;
        }
        #endregion
    }
}
