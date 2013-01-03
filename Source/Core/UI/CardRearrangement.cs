using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public struct CardRearrangement
    {
        public int SourceDeckIndex;
        public int SourceCardIndex;
        public int DestDeckIndex;
        public int DestCardIndex;
        public CardRearrangement(int sourceDeckIndex, int sourceCardIndex, int destDeckIndex, int destCardIndex)
            : this()
        {
            SourceDeckIndex = sourceDeckIndex;
            SourceCardIndex = sourceCardIndex;
            DestDeckIndex = destDeckIndex;
            DestCardIndex = destCardIndex;
        }
    }
}
