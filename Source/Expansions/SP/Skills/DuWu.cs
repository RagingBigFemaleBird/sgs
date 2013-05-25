using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.SP.Skills
{
    public class DuWu : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            List<Card> cards = arg.Cards;
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            (LinkedPassiveSkill as DuWuPassive).DuWuPlayer = arg.Targets[0];
            Game.CurrentGame.DoDamage(Owner, arg.Targets[0], 1, DamageElement.None, null, null);
            (LinkedPassiveSkill as DuWuPassive).DuWuPlayer = null;
            return true;
        }

        public static PlayerAttribute DuWuUsed = PlayerAttribute.Register("DuWuUsed", true);

        public DuWu()
        {
            MinCards = 0;
            MaxCards = int.MaxValue;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = true;
            LinkedPassiveSkill = new DuWuPassive();
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[DuWuUsed] != 0) return false;
            if ((players == null || players.Count == 0) && (cards != null && cards.Count > 0)) return false;
            if (players == null || players.Count == 0) return null;
            int req = players[0].Health;
            if (players.Any(p => Game.CurrentGame.DistanceTo(source, p) > source[Player.AttackRange] + 1))
            {
                return false;
            }
            if (req > 0 && (cards == null || cards.Count < req)) return null;
            if (req > 0 && (cards != null && cards.Count > req)) return false;
            if (req > 0 && cards != null && cards.Count > 0)
            {
                var temp = new Sha();
                temp.HoldInTemp(cards);
                if (players.Any(p => Game.CurrentGame.DistanceTo(source, p) > source[Player.AttackRange] + 1))
                {
                    temp.ReleaseHoldInTemp();
                    return false;
                }
                temp.ReleaseHoldInTemp();
            }
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand || card.Place.DeckType == DeckType.Equipment;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        public class DuWuPassive : TriggerSkill
        {
            public Player DuWuPlayer;
            public DuWuPassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Targets.Contains(DuWuPlayer); },
                    (p, e, a) =>
                    {
                        Owner[DuWu.DuWuUsed] = 1;
                        Game.CurrentGame.LoseHealth(Owner, 1);
                    },
                    TriggerCondition.Global
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
                IsAutoInvoked = null;
            }
        }
    }
}
