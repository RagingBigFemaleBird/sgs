using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 天香-每当你受到伤害时，你可以弃置一张红桃手牌来转移此伤害给一名其他角色，然后该角色摸X张牌(X为该角色当前已损失的体力值)。
    /// </summary>
    public class TianXiang : TriggerSkill
    {
        public class TianXiangVerifier : CardsAndTargetsVerifier
        {

            public TianXiangVerifier()
            {
                MinPlayers = 1;
                MaxPlayers = 1;
                MinCards = 1;
                MaxCards = 1;
                Discarding = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Suit == SuitType.Heart;
            }
       }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            DamageEventArgs args = eventArgs as DamageEventArgs;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("TianXiang"), new TianXiangVerifier(),
                out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                ReadOnlyCard ncard = new ReadOnlyCard(args.ReadonlyCard);
                ncard[Armor.IgnoreAllArmor] = 0;
                Game.CurrentGame.DoDamage(args.Source, players[0], args.Magnitude, args.Element, args.Card, ncard);
                Game.CurrentGame.DrawCards(players[0], players[0].MaxHealth - players[0].Health);
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        public TianXiang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.DamageInflicted, trigger);
            IsAutoInvoked = null;
        }

    }
}
