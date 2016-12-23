using MusicGraphExplorerAPI.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicGraphExplorerAPI.Comparers
{
    public class RelatedArtistListComparer : IEqualityComparer<List<GetRelatedArtistResponse>>
    {
        public bool Equals(List<GetRelatedArtistResponse> list1, List<GetRelatedArtistResponse> list2)
        {
            if (list1.Count != list2.Count) { return false; }

            //we will ensure that every artist in the path is the same, and the order is the same
            for (int i = 0; i < list1.Count; i++)
            {
                ////TODO: consider whether we want to return paths with same artists but variation in relevance (relevance is directional)
                if ( (list1[i].SpotifyId != list2[i].SpotifyId) )
                  //|| (list1[i].Relevance != list2[i].Relevance) )
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<GetRelatedArtistResponse> list)
        {
            if (list == null) return 0;
            unchecked
            {
                int hash = 19;
                foreach (GetRelatedArtistResponse a in list)
                {
                    hash = hash * 31 + (a.SpotifyId == null ? 0 : a.SpotifyId.GetHashCode());
                }
                return hash;
            }
        }
    }
}