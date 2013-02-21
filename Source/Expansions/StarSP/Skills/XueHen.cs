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
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 雪恨―锁定技，一名角色的回合结束阶段开始时，若你为无节操状态，你须捡回节操并选择一项：1、弃置其等同于你已损失体力值数的牌；2、视为对一名角色使用一张【杀】。
    /// </summary>
    public class XueHen : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return XueHenEffect;
        }

        class XueHenShaVerifier : CardsAndTargetsVerifier
        {
            public XueHenShaVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
                Discarding = false;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return Game.CurrentGame.PlayerCanBeTargeted(source, new List<Player>() { player }, new Card() { Place = new DeckPlace(source, DeckType.None), Type = new Sha() });
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Owner[FenYong.FenYongStatus] = 0;
            int answer = 0;
            Player current = Game.CurrentGame.CurrentPlayer;
            int choiceCount = Owner.LostHealth;
            int currentPlayerCardsCount = current.HandCards().Count + current.Equipments().Count();
            List<Player> shaCheck = Game.CurrentGame.AlivePlayers;
            shaCheck.Remove(Owner);
            bool canUseSha = Game.CurrentGame.PlayerCanBeTargeted(Owner, shaCheck, new Card() { Place = new DeckPlace(Owner, DeckType.None), Type = new Sha() });
            if (canUseSha)
            {
                List<OptionPrompt> prompts = new List<OptionPrompt>();
                prompts.Add(new OptionPrompt("XueHenQiPai", current, choiceCount));
                prompts.Add(new OptionPrompt("XueHenSha"));
                Owner.AskForMultipleChoice(new MultipleChoicePrompt("XueHen"), prompts, out answer);
            }
            if (answer == 0)
            {
                XueHenEffect = 0;
                NotifySkillUse();
                if (currentPlayerCardsCount == 0) return;
                List<Card> toDiscard = new List<Card>();
                if (currentPlayerCardsCount <= choiceCount)
                {
                    toDiscard.AddRange(current.HandCards());
                    toDiscard.AddRange(current.Equipments());
                }
                else
                {
                    List<List<Card>> choiceAnswer;
                    List<DeckPlace> sourcePlace = new List<DeckPlace>();
                    sourcePlace.Add(new DeckPlace(current, DeckType.Hand));
                    sourcePlace.Add(new DeckPlace(current, DeckType.Equipment));
                    if (!Owner.AskForCardChoice(new CardChoicePrompt("XueHen", current, Owner),
                        sourcePlace,
                        new List<string>() { "QiPaiDui" },
                        new List<int>() { choiceCount },
                        new RequireCardsChoiceVerifier(choiceCount),
                        out choiceAnswer,
                        null,
                        CardChoiceCallback.GenericCardChoiceCallback))
                    {
                        choiceAnswer = new List<List<Card>>();
                        choiceAnswer.Add(Game.CurrentGame.PickDefaultCardsFrom(new List<DeckPlace>() { new DeckPlace(current, DeckType.Hand), new DeckPlace(current, DeckType.Equipment) }, choiceCount));
                    }
                    toDiscard = choiceAnswer[0];
                }
                Game.CurrentGame.HandleCardDiscard(current, toDiscard);
            }
            else
            {
                XueHenEffect = 1;
                NotifySkillUse();
                Owner[Sha.NumberOfShaUsed]--;
                Sha.UseDummyShaTo(Owner, null, new RegularSha(), new CardUsagePrompt("XueHen"), XueHenSha);
            }
        }

        public XueHen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Owner[FenYong.FenYongStatus] == 1; },
                Run,
                TriggerCondition.Global
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card[XueHenSha] != 0; },
                (p, e, a) =>
                {
                    ShaEventArgs args = a as ShaEventArgs;
                    args.RangeApproval[0] = true;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(Sha.PlayerShaTargetValidation, trigger2);

            IsEnforced = true;
        }
        int XueHenEffect;
        public static CardAttribute XueHenSha = CardAttribute.Register("XueHenSha");
    }
}
