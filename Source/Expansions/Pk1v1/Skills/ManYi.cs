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
using Sanguosha.Expansions.Pk1v1.Cards;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 蛮裔-你登场时，你可视为对对手使用一张【南蛮入侵】；【南蛮入侵】对你无效。
    /// </summary>
    public class ManYi : TriggerSkill
    {
        class ManYiVerifier : CardUsageVerifier
        {
            public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (cards != null || cards.Count > 0)
                {
                    return VerifierResult.Fail;
                }
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (players[0] == source)
                {
                    return VerifierResult.Fail;
                }
                return new NanManRuQin().Verify(source, null, players, false);
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            public ManYiVerifier()
            {
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Owner.AskForCardUsage(new CardUsagePrompt("ManYi"), new ManYiVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse();
                GameEventArgs args = new GameEventArgs();
                args.Source = Owner;
                args.Targets = players;
                args.Skill = new CardWrapper(Owner, new NanManRuQin(), false);
                args.Cards = new List<Card>();
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            }
        }

        public ManYi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return true;
                },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.HeroDebut, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            ) { Type = TriggerType.Skill };
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger2);
            IsAutoInvoked = null;
        }
    }
}
