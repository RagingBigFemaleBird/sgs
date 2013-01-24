using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 涅槃-限定技，当你处于濒死状态时，你可以弃置所有的牌和判定区里的牌，然后将你的武将牌翻至正面朝上，并重置之，再摸三张牌且体力回复至3点。
    /// </summary>
    public class NiePan : SaveLifeSkill
    {
        protected override bool? SaveLifeVerify(Player source, List<Card> cards, List<Player> players)
        {
            return Owner[NiePanUsed] == 0;
        }

        public NiePan()
        {
            IsSingleUse = true;
            Helper.HasNoConfirmation = true;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[NiePanUsed] = 1;
            Game.CurrentGame.SyncImmutableCardsAll(Owner.HandCards());
            List<Card> toDiscard = new List<Card>();
            toDiscard.AddRange(Owner.HandCards());
            toDiscard.AddRange(Owner.Equipments());
            toDiscard.AddRange(Owner.DelayedTools());
            Game.CurrentGame.HandleCardDiscard(Owner, toDiscard);
            Owner.IsImprisoned = false;
            Owner.IsIronShackled = false;
            Game.CurrentGame.DrawCards(Owner, 3);
            Game.CurrentGame.RecoverHealth(Owner, Owner, Math.Min(3, Owner.MaxHealth) - Owner.Health);
            return true;
        }
        public static PlayerAttribute NiePanUsed = PlayerAttribute.Register("NiePanUsed", false);
    }
}
