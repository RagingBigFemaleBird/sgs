using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 言笑–出牌阶段，你可以将一张方片牌置入一名角色的判定区内，判定区内有“言笑”牌的角色下个判定阶段开始时，获得其判定区内的所有牌。
    /// </summary>
    class YanXiao : OneToOneCardTransformSkill
    {
        public override CardHandler PossibleResult
        {
            get { return new YanXiaoPai(); }
        }

        public override bool VerifyInput(Card card, object arg)
        {
            return card.Suit == SuitType.Diamond;
        }
    }

    class YanXiaoTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!eventArgs.Source.DelayedTools().Any(c => c.Type is YanXiaoPai))
            {
                return;
            }
            Game.CurrentGame.HandleCardTransferToHand(null, eventArgs.Source, eventArgs.Source.DelayedTools());
        }
    }

    #region YanXiaoPai

    class YanXiaoPai : DelayedTool
    {
        public override void Activate(Player p, Card c)
        {
            Trace.Assert(false);
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        public override void Process(GameEventArgs handlerArgs)
        {
            var source = handlerArgs.Source;
            var dests = handlerArgs.Targets;
            var readonlyCard = handlerArgs.ReadonlyCard;
            var inResponseTo = handlerArgs.InResponseTo;
            var card = handlerArgs.Card;
            Trace.Assert(dests.Count == 1);
            AttachTo(source, dests[0], card);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool DelayedToolConflicting(Player p)
        {
            return false;
        }
    }

    #endregion
}
