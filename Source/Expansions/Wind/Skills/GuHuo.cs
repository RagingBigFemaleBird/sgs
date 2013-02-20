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
    /// 蛊惑–当你需要使用或打出一张基本牌或非延时类锦囊牌时，你可以声明并将一张手牌扣于桌上。若无人质疑，则该牌按你所述之牌来用。若有人质疑则亮出验明：若为真，质疑者各失去一点体力；若为假，质疑者各摸一张牌。除非被质疑的牌的花色为红桃且为真（仍然可用），否则无论真假，该牌都作废，弃置之。
    /// </summary>
    public class GuHuo : CardTransformSkill, IAdditionalTypedSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = new CompositeCard();
            card.Subcards = new List<Card>();
            card.Type = AdditionalType;
            card[TieSuoLianHuan.ProhibitReforging] = 1;
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

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            int GuHuoOrder = Game.CurrentGame.Decks[null, DeckType.GuHuo].Count;
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
            Dictionary<Player, int> believe = new Dictionary<Player,int>();
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
                            Game.CurrentGame.LoseHealth(player, 1);
                        }
                    }
                    if (Game.CurrentGame.Decks[null, DeckType.GuHuo][GuHuoOrder].Suit != SuitType.Heart)
                    {
                        ret = false;
                    }
                }
                else
                {
                    foreach (var player in toProcess)
                    {
                        if (believe[player] == 0)
                        {
                            Game.CurrentGame.DrawCards(player, 1);
                        }
                    }
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
    }
}
