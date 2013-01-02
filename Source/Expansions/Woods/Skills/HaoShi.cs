using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 好施-摸牌阶段摸牌时，你可以额外摸两张牌，若此时你的手牌多于五张，则将一半(向下取整)的手牌交给场上手牌数最少的一名其他角色。
    /// </summary>
    public class HaoShi : TriggerSkill
    {
        public class HaoShiVerifier : CardsAndTargetsVerifier
        {
            public HaoShiVerifier(int cc)
            {
                MinCards = cc;
                MaxCards = cc;
                MinPlayers = 1;
                MaxPlayers = 1;
                Helper.NoCardReveal = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                var list = new List<Player>(Game.CurrentGame.AlivePlayers);
                list.Remove(source);
                int minHC = int.MaxValue;
                List<Player> minHCPlayers = new List<Player>();
                foreach (var pl in list)
                {
                    int count = Game.CurrentGame.Decks[pl, DeckType.Hand].Count;
                    if (count < minHC)
                    {
                        minHC = count;
                        minHCPlayers = new List<Player>() { pl };
                    }
                    else if (count == minHC)
                    {
                        minHCPlayers.Add(pl);
                    }
                }
                return minHCPlayers.Contains(player);
            }
        }

        protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            List<Card> cards;
            List<Player> players;
            ISkill skill;
            int halfHC = Game.CurrentGame.Decks[owner, DeckType.Hand].Count / 2;
            if (!Game.CurrentGame.UiProxies[owner].AskForCardUsage(new CardUsagePrompt("HaoShi"), new HaoShiVerifier(halfHC), out skill, out cards, out players))
            {
                cards = new List<Card>();
                cards.AddRange(Game.CurrentGame.Decks[owner, DeckType.Hand].GetRange(0, halfHC));
                int minHC = int.MaxValue;
                Player p = null;
                foreach (var pl in Game.CurrentGame.AlivePlayers)
                {
                    int count = Game.CurrentGame.Decks[owner, DeckType.Hand].Count;
                    if (pl != owner && count < minHC)
                    {
                        p = pl;
                        minHC = count;
                    }
                }
                players = new List<Player>() { p };
            }
            Game.CurrentGame.HandleCardTransferToHand(owner, players[0], cards);
        }

        public static PlayerAttribute HaoShiUsed = PlayerAttribute.Register("HaoShiUsed", true);

        public HaoShi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[HaoShiUsed] == 1 && Game.CurrentGame.Decks[p, DeckType.Hand].Count > 5; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, Type = TriggerType.Skill, Priority = int.MaxValue };
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Draw], trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { p[Player.DealAdjustment] += 2; p[HaoShiUsed] = 1; },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger2);
            IsAutoInvoked = false;
        }
    }
}
