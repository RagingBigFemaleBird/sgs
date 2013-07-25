using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 蛊惑–每名角色的回合限一次，你可以扣置一张手牌当任意一张基本牌或非延时锦囊牌使用或打出。此时，一旦有其他角色质疑时，翻开此牌，若为假此牌作废，若为真则质疑角色获得技能“瞠惑”（锁定技，你不能质疑于吉。只要你的体力值为1，你失去角色其他技能。）
    /// </summary>
    public class GuHuo : CardTransformSkill, IAdditionalTypedSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Subcards = new List<Card>();
            card.Type = AdditionalType;
            card[TieSuoLianHuan.ProhibitReforging] = 1;
            if (Game.CurrentGame.CurrentPlayer == null || Game.CurrentGame.CurrentPlayer[GuHuoUsed] == 1)
            {
                return VerifierResult.Fail;
            }
            if (AdditionalType == null)
            {
                return VerifierResult.Partial;
            }
            if (!CardCategoryManager.IsCardCategory(AdditionalType.Category, CardCategory.Basic) &&
                !CardCategoryManager.IsCardCategory(AdditionalType.Category, CardCategory.ImmediateTool))
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count != 1)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }

            card.Subcards.Add(cards[0]);
            return VerifierResult.Success;
        }

        public static readonly PlayerAttribute GuHuoUsed = PlayerAttribute.Register("GuHuoUsed", true);
        public static readonly PlayerAttribute ZhiYiZhong = PlayerAttribute.Register("ZhiYi", false, false, true);
        public static readonly PlayerAttribute BuZhiYiZhong = PlayerAttribute.Register("BuZhiYi", false, false, true);

        public override void NotifyAction(Player source, List<Player> targets, CompositeCard card)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = card;
            log.SkillAction = this;
            log.Source = source;
            log.Targets = targets;
            log.ShowCueLine = true;
            log.SpecialEffectHint = GenerateSpecialEffectHintIndex(source, targets, card);
            Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
            if (card.Subcards != null)
            {
                foreach (Card c in card.Subcards)
                {
                    c.Log.SkillAction = this;
                    c.Log.Source = source;
                    c.Log.Targets = targets;
                    c.Log.GameAction = GameAction.Use;
                }
            }
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets, bool isPlay)
        {
            int GuHuoOrder = Game.CurrentGame.Decks[null, DeckType.GuHuo].Count;
            Game.CurrentGame.CurrentPlayer[GuHuoUsed] = 1;
            CardsMovement move = new CardsMovement();
            Trace.Assert(card.Subcards.Count == 1);
            move.Cards = new List<Card>();
            move.Cards.AddRange(card.Subcards);
            move.To = new DeckPlace(null, DeckType.GuHuo);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(Owner, move.Cards);
            var toProcess = new List<Player>(from p in Game.CurrentGame.AlivePlayers where p.Health > 0 select p);
            toProcess.Remove(Owner);
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, toProcess);
            foreach (var skfilter in new List<Player>(toProcess))
            {
                foreach (var sk in skfilter.Skills)
                {
                    if (sk.GetType().Name.Contains("ChengHuo"))
                    {
                        toProcess.Remove(skfilter);
                        break;
                    }
                }
            }
            Dictionary<Player, int> believe = new Dictionary<Player, int>();
            foreach (var player in toProcess)
            {
                int answer = 0;
                Game.CurrentGame.UiProxies[player].AskForMultipleChoice(new MultipleChoicePrompt("GuHuo", Owner, AdditionalType), Prompt.YesNoChoices, out answer);
                believe.Add(player, 1 - answer);
                player[ZhiYiZhong] = answer;
                player[BuZhiYiZhong] = 1 - answer;
            }
            Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.Decks[null, DeckType.GuHuo][GuHuoOrder]);
            bool guhuoSucceed = true;
            foreach (var v in believe)
            {
                if (v.Value == 0)
                {
                    guhuoSucceed = false;
                    break;
                }
            }
            bool ret = true;
            if (!guhuoSucceed)
            {
                if (Game.CurrentGame.Decks[null, DeckType.GuHuo][GuHuoOrder].Type.GetType().IsAssignableFrom(AdditionalType.GetType()))
                {
                    foreach (var player in toProcess)
                    {
                        if (believe[player] == 0)
                        {
                            Game.CurrentGame.PlayerAcquireAdditionalSkill(player, new ChengHuo(), null);
                            if (player.Health == 1)
                            {
                                player.LoseAllHerosSkills();
                                foreach (ISkill sk in new List<ISkill>(player.AdditionalSkills))
                                {
                                    if (!sk.GetType().Name.Contains("ChengHuo"))
                                    {
                                        Game.CurrentGame.PlayerLoseAdditionalSkill(player, sk);
                                    }
                                }
                                player[ChengHuo.ChengHuoStatus] = 1;
                            }
                        }
                    }
                }
                else
                {
                    ret = false;
                }
                if (!ret)
                {
                    move = new CardsMovement();
                    move.Cards = new List<Card>();
                    move.Cards.Add(Game.CurrentGame.Decks[null, DeckType.GuHuo][GuHuoOrder]);
                    move.To = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move);
                }
            }
            if (ret)
            {
                card.Subcards = new List<Card>();
                card.Subcards.Add(Game.CurrentGame.Decks[null, DeckType.GuHuo][GuHuoOrder]);
            }
            foreach (var player in toProcess)
            {
                player[ZhiYiZhong] = 0;
                player[BuZhiYiZhong] = 0;
            }
            return ret;
        }

        public GuHuo()
        {
            Helper.NoCardReveal = true;
        }

        public CardHandler AdditionalType { get; set; }
        public class ChengHuo : TriggerSkill
        {

            public static PlayerAttribute ChengHuoStatus = PlayerAttribute.Register("ChengHuo", false, false, true);

            public ChengHuo()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return p.Health == 1;
                    },
                    (p, e, a) =>
                    {

                        p.LoseAllHerosSkills();
                        foreach (ISkill sk in new List<ISkill>(p.AdditionalSkills))
                        {
                            if (!sk.GetType().Name.Contains("ChengHuo"))
                            {
                                Game.CurrentGame.PlayerLoseAdditionalSkill(p, sk);
                            }
                        }
                        p[ChengHuoStatus] = 1;
                    },
                    TriggerCondition.OwnerIsTarget
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.AfterHealthChanged, trigger);
                IsAutoInvoked = null;
            }

        }
    }
}
