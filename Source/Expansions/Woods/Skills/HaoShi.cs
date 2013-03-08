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
using System.Diagnostics;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 好施-摸牌阶段摸牌时，你可以额外摸两张牌，若此时你的手牌多于五张，则将一半(向下取整)的手牌交给场上手牌数最少的一名其他角色。
    /// </summary>
    public class HaoShi : TriggerSkill
    {
        public class HaoShiVerifier : CardsAndTargetsVerifier
        {
            public HaoShiVerifier(int cc, List<Player> targets)
            {
                MinCards = cc;
                MaxCards = cc;
                MinPlayers = 1;
                MaxPlayers = 1;
                Helper.NoCardReveal = true;
                minHCPlayers = targets;
            }

            private List<Player> minHCPlayers;

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return minHCPlayers.Contains(player);
            }
        }

        protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            List<Card> cards;
            List<Player> players;
            ISkill skill;
            int halfHC = Game.CurrentGame.Decks[owner, DeckType.Hand].Count / 2;
                        
            int minHC = int.MaxValue;
            List<Player> minHCPlayers = new List<Player>();
            var alivePlayers = Game.CurrentGame.AlivePlayers;
            foreach (var pl in alivePlayers)
            {
                if (pl == owner) continue;
                int count = Game.CurrentGame.Decks[pl, DeckType.Hand].Count;
                if (count < minHC)
                {
                    minHC = count;
                    minHCPlayers.Clear();
                    minHCPlayers.Add(pl);
                }
                else if (count == minHC)
                {
                    minHCPlayers.Add(pl);
                }
            }

            Trace.Assert(minHCPlayers.Count > 0);
            CardUsagePrompt prompt;
            if (minHCPlayers.Count == 1) prompt = new CardUsagePrompt("HaoShi1", halfHC, minHCPlayers[0]);
            else prompt = new CardUsagePrompt("HaoShi", halfHC);

            if (!Game.CurrentGame.UiProxies[owner].AskForCardUsage(prompt, new HaoShiVerifier(halfHC, minHCPlayers), out skill, out cards, out players))
            {
                cards = new List<Card>();
                cards.AddRange(Game.CurrentGame.Decks[owner, DeckType.Hand].GetRange(0, halfHC));
                Player p = minHCPlayers[0];
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
