using MusicGraphStore.GraphDataModel;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.DataAccessLayer
{
    /// <summary>
    /// Helper methods
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Deserialize an IRecord into an Artist
        /// </summary>
        /// <param name="record"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        internal static Artist DeserializeRecord(IRecord record, Artist a)
        {
            if (null == record) { return a; }

            if (record.Keys.Contains<string>("SpotifyId")) { a.SpotifyId = record["SpotifyId"].As<string>(); }

            if (record.Keys.Contains<string>("Name")) { a.Name = record["Name"].As<string>(); }

            if (record.Keys.Contains<string>("Popularity")) { a.Popularity = record["Popularity"].As<int>(); }

            if (record.Keys.Contains<string>("Url")) { a.Url = record["Url"].As<string>(); }

            if (record.Keys.Contains<string>("Relevance")) { a.Relevance = float.Parse(record["Relevance"].As<string>()); }

            return a;
        }

        /// <summary>
        /// Deserialize an INode into an Artist
        /// </summary>
        /// <param name="record"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        internal static Artist DeserializeNode(INode record, Artist a)
        {
            if (null == record) { return a; }

            if (record.Properties.ContainsKey("SpotifyId")) { a.SpotifyId = record.Properties["SpotifyId"].As<string>(); }

            if (record.Properties.ContainsKey("Name")) { a.Name = record.Properties["Name"].As<string>(); }

            if (record.Properties.ContainsKey("Popularity")) { a.Popularity = record.Properties["Popularity"].As<int>(); }

            if (record.Properties.ContainsKey("Url")) { a.Url = record.Properties["Url"].As<string>(); }

            if (record.Properties.ContainsKey("Relevance")) { a.Relevance = float.Parse(record.Properties["Relevance"].As<string>()); }

            return a;
        }

        /// <summary>
        /// Deserialize an Irecord into a Genre
        /// </summary>
        /// <param name="record"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        internal static Genre DeserializeRecord(IRecord record, Genre g)
        {
            if (null == record) { return g; }

            if (record.Keys.Contains<string>("Name")) { g.Name = record["Name"].As<string>(); }

            return g;
        }

        /// <summary>
        /// Deserialize an INode into a Genre
        /// </summary>
        /// <param name="record"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        internal static Genre DeserializeNode(INode record, Genre g)
        {
            if (null == record) { return g; }

            if (record.Properties.ContainsKey("Name")) { g.Name = record.Properties["Name"].As<string>(); }

            return g;
        }

    }
}
