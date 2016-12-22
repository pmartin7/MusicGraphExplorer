using MusicGraphStore.GraphDataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicGraphExplorerAPI.DataContract
{
    [DataContract]
    public class GetGenreResponse
    {

        #region properties
        [DataMember(Order = 0)]
        public string Name { get; set; }

        #endregion

        #region Contructors
        public GetGenreResponse(Genre genre)
        {
            this.Name = genre.Name;
        }
        
        #endregion
    }
}