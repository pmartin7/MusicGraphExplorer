using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.QueueManager
{
    public static class InstructionCodes
    {
        public const ushort UpdateGraphFromSpotify = 0;

        public const ushort UpdateArtistRelationships = 1;

        public const ushort UpdateGenreRelationships = 2;

        public const ushort UpdateArtistGenreRelevance = 3;
    }
}
