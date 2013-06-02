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

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 骁袭-你登场时，你可视为对对手使用一张【杀】。
    /// </summary>
    public class XiaoXi : TriggerSkill
    {
        class XiaoXiVerifier : CardUsageVerifier
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
                return verifier.FastVerify(source, skill, new List<Card>(), players);
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            DummyShaVerifier verifier;
            public XiaoXiVerifier(DummyShaVerifier verifier)
            {
                this.verifier = verifier;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Owner.AskForCardUsage(new CardUsagePrompt("XiaoXi"), new XiaoXiVerifier(verifier), out skill, out cards, out players))
            {
                NotifySkillUse();
                GameEventArgs args = new GameEventArgs();
                Owner[Sha.NumberOfShaUsed]--;
                args.Source = Owner;
                args.Targets = players;
                args.Skill = skill == null ? new CardWrapper(Owner, new RegularSha(), false) : skill;
                args.Cards = new List<Card>();
                CardTransformSkill transformSkill = skill as CardTransformSkill;
                if (transformSkill != null)
                {
                    CompositeCard card;
                    transformSkill.TryTransform(new List<Card>() { new Card() { Type = new RegularSha(), Place = new DeckPlace(null, DeckType.None) } }, players, out card);
                    card.Subcards.Clear();
                    args.Card = card;
                }
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            }
        }

        DummyShaVerifier verifier;
        public XiaoXi()
        {
            verifier = new Basic.Cards.DummyShaVerifier(null, new RegularSha(), XiaoXiSha);
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return true;
                },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card[XiaoXiSha] != 0; },
                (p, e, a) =>
                {
                    ShaEventArgs args = a as ShaEventArgs;
                    args.RangeApproval[0] = true;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.HeroDebut, trigger);
            Triggers.Add(Sha.PlayerShaTargetValidation, trigger2);
            IsAutoInvoked = null;
        }

        public static CardAttribute XiaoXiSha = CardAttribute.Register("XiaoXiSha");
    }
}
