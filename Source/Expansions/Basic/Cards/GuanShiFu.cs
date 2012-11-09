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

namespace Sanguosha.Expansions.Basic.Cards
{
    public class GuanShiFu : Weapon
    {
        public GuanShiFu()
        {
            EquipmentSkill = new GuanShiFuSkill();
        }

        public class GuanShiFuVerifier : ICardUsageVerifier
        {
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > 2)
                {
                    return VerifierResult.Fail;
                }
                foreach (Card c in cards)
                {
                    if (!Game.CurrentGame.PlayerCanDiscardCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                    if (c.Place.DeckType == DeckType.Equipment && (c.Type is Weapon))
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (cards.Count < 2)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            public UiHelper Helper
            {
                get { return new UiHelper(); }
            }

            public GuanShiFuVerifier()
            {
            }

        }

        public class GuanShiFuSkill : TriggerSkill
        {
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("GuanShiFu"),
                    new GuanShiFuVerifier(),
                    out skill, out cards, out players))
                {
                    NotifySkillUse(new List<Player>());
                    Game.CurrentGame.HandleCardDiscard(Owner, cards);
                    Trace.Assert(eventArgs.Card.Type is Sha);
                    Game.CurrentGame.DoDamage(eventArgs.Source, eventArgs.Targets[0], 1, (eventArgs.Card.Type as Sha).ShaDamageElement, eventArgs.Card);
                }
            }
            public GuanShiFuSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    Run,
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false, AskForConfirmation = false };
                Triggers.Add(Sha.PlayerShaTargetDodged, trigger);
            }
        }

        public override int AttackRange
        {
            get { return 3; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            return;
        }

    }
}
