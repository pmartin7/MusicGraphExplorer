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

        internal static Artist deserializeRecord(IRecord record, Artist a)
        {
            if (null == record) { return a; }

            if (record.Keys.Contains<string>("SpotifyId")) { a.SpotifyId = record["SpotifyId"].As<string>(); }

            if (record.Keys.Contains<string>("Name")) { a.Name = record["Name"].As<string>(); }

            if (record.Keys.Contains<string>("Popularity")) { a.Popularity = record["Popularity"].As<int>(); }

            if (record.Keys.Contains<string>("Url")) { a.Url = record["Url"].As<string>(); }

            if (record.Keys.Contains<string>("Relevance")) { a.Relevance = float.Parse(record["Relevance"].As<string>()); }

            return a;
        }

        internal static Genre deserializeRecord(IRecord record, Genre g)
        {
            if (null == record) { return g; }

            if (record.Keys.Contains<string>("Name")) { g.Name = record["Name"].As<string>(); }

            return g;
        }

    }
}
