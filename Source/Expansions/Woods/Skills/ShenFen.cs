using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 神愤―出牌阶段，你可以弃6枚“暴怒”标记，对所有其他角色各造成1点伤害，所有其他角色先弃置各自装备区里的牌，再弃置四张手牌，然后将你的武将牌翻面。每阶段限一次。
    /// </summary>
    public class ShenFen : ActiveSkill
    {
        public ShenFen()
        {
            UiHelper.HasNoConfirmation = true;
        }

        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[KuangBao.BaoNuMark] < 6 || Owner[ShenFenUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[ShenFenUsed] = 1;
            Owner[KuangBao.BaoNuMark] -= 6;
            var players = Game.CurrentGame.AlivePlayers;
            players.Remove(Owner);
            Game.CurrentGame.SortByOrderOfComputation(Owner, players);
            foreach (Player p in players)
            {
                Game.CurrentGame.DoDamage(Owner, p, 1, DamageElement.None, null, null);
            }
            foreach (Player p in players)
            {
                List<Card> toDiscard = new List<Card>();
                toDiscard.AddRange(p.Equipments());
                Game.CurrentGame.HandleCardDiscard(p, toDiscard);
                if (p.HandCards().Count() <= 4)
                {
                    toDiscard.Clear();
                    toDiscard.AddRange(p.HandCards());
                    Game.CurrentGame.HandleCardDiscard(p, toDiscard);
                }
                else
                {
                    Game.CurrentGame.ForcePlayerDiscard(p, (pl, d) => { return 4; }, false);
                }
            }
            Owner.IsImprisoned = !Owner.IsImprisoned;
            return true;
        }

        private static PlayerAttribute ShenFenUsed = PlayerAttribute.Register("ShenFenUsed", true);
    }
}
