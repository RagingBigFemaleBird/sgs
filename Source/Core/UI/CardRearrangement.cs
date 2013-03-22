using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Sanguosha.Core.UI
{
    [ProtoContract]
    public class CardRearrangement
    {
        [ProtoMember(1)]
        public int SourceDeckIndex { get; set; }
        [ProtoMember(2)]
        public int SourceCardIndex { get; set; }
        [ProtoMember(3)]
        public int DestDeckIndex { get; set; }
        [ProtoMember(4)]
        public int DestCardIndex { get; set; }
        public CardRearrangement()
        {
        }
        public CardRearrangement(int sourceDeckIndex, int sourceCardIndex, int destDeckIndex, int destCardIndex)
        {
            SourceDeckIndex = sourceDeckIndex;
            SourceCardIndex = sourceCardIndex;
            DestDeckIndex = destDeckIndex;
            DestCardIndex = destCardIndex;
        }
    }
}
