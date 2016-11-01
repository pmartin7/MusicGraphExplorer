using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicGraphStore.DataAccessLayer;

namespace MusicGraphStore.GraphDataModel
{
    public class GraphUpdateState
    {
        #region properties
        public string Type { get; set; }

        public DateTime LastInstructionAddedStartDtTm { get; set; }

        public DateTime LastInstructionAddedEndDtTm { get; set; }

        public DateTime LastUpdateStartDtTm { get; set; }

        public DateTime LastUpdateEndDtTm { get; set; }

        public int NumberOfNodesUpdated { get; set; }
        #endregion

        #region Public Refresh from Graph Store Methods
        public void RefreshFromDb()
        {
            DataAccess dal = DataAccess.Instance;
            GraphUpdateState state = dal.GetGraphUpdateState(Type);
            LastInstructionAddedEndDtTm = state.LastInstructionAddedEndDtTm;
            LastInstructionAddedStartDtTm = state.LastInstructionAddedStartDtTm;
            LastUpdateEndDtTm = state.LastUpdateEndDtTm;
            LastUpdateStartDtTm = state.LastUpdateStartDtTm;
            NumberOfNodesUpdated = state.NumberOfNodesUpdated;
        }
        #endregion

        #region Public Update Graph Methods

        public void SyncLastInstructionAddedStartDtTm()
        {
            DataAccess dal = DataAccess.Instance;
            dal.UpdateGraphUpdateState(this, "LastInstructionAddedStartDtTm");
        }

        public void SyncLastInstructionAddedEndDtTm()
        {
            DataAccess dal = DataAccess.Instance;
            dal.UpdateGraphUpdateState(this, "LastInstructionAddedEndDtTm");
        }

        public void SyncLastUpdateStartdDtTm()
        {
            DataAccess dal = DataAccess.Instance;
            dal.UpdateGraphUpdateState(this, "LastGraphUpdateStartDtTm");
        }

        public void SyncLastUpdateEndDtTm()
        {
            DataAccess dal = DataAccess.Instance;
            dal.UpdateGraphUpdateState(this, "LastGraphUpdateEndDtTm");
        }

        public void SyncNumberOfNodesUpdated( int number)
        {
            DataAccess dal = DataAccess.Instance;
            dal.UpdateGraphUpdateStateNumberOfRecords(this, number);
        }

        #endregion
    }
}
