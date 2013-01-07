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

namespace Sanguosha.Expansions.SP.Skills
{
    public class YuanHu : TriggerSkill
    {
        /// <summary>
        /// 援护-回合结束阶段开始时，你可以将一张装备牌置于一名角色的装备区里，然后根据此装备牌的种类执行以下效果。
        /// 武器牌：弃置与该角色距离为1的一名角色区域中的一张牌；
        /// 防具牌：该角色摸一张牌；
        /// 坐骑牌：该角色回复1点体力。
        /// </summary>

        class YuanHuVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if ((cards == null || cards.Count == 0) && players != null && players.Count > 0)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (!cards[0].Type.IsCardCategory(CardCategory.Equipment))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (players.Count > 2 || players.Count > 1 && !cards[0].Type.IsCardCategory(CardCategory.Weapon))
                {
                    return VerifierResult.Fail;
                }
                if (players[0].Equipments().Any(c => c.Type.IsCardCategory(cards[0].Type.Category)))
                {
                    return VerifierResult.Fail;
                }

                CardHandler handler = new Sha();
                bool holdInTemp = cards[0].Place.DeckType == DeckType.Hand;
                if (holdInTemp) handler.HoldInTemp(cards);
                if (players.Count == 1 && cards.Count == 1 && cards[0].Type.IsCardCategory(CardCategory.Weapon))
                {
                    List<Player> pls = Game.CurrentGame.AlivePlayers;
                    if (pls.Any(p => p != players[0] && Game.CurrentGame.DistanceTo(players[0], p) == 1 && p.HandCards().Concat(p.Equipments()).Count() > 0))
                    {
                        if (holdInTemp) handler.ReleaseHoldInTemp();
                        return VerifierResult.Partial;
                    }
                }
                if (players.Count == 2)
                {
                    if (Game.CurrentGame.DistanceTo(players[0], players[1]) != 1 || players[1].HandCards().Concat(players[1].Equipments()).Count() == 0)
                    {
                        if (holdInTemp) handler.ReleaseHoldInTemp();
                        return VerifierResult.Fail;
                    }
                }
                if (holdInTemp) handler.ReleaseHoldInTemp();
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            Card theCard = cards[0];
            Game.CurrentGame.HandleCardTransfer(owner, players[0], DeckType.Equipment, cards);
            switch (theCard.Type.Category)
            {
                case CardCategory.Weapon:
                    {
                        if (players.Count == 2)
                        {
                            List<List<Card>> answer;
                            List<DeckPlace> places = new List<DeckPlace>();
                            places.Add(new DeckPlace(players[1], DeckType.Hand));
                            places.Add(new DeckPlace(players[1], DeckType.Equipment));
                            if (!owner.AskForCardChoice(new CardChoicePrompt("YuanHu", players[1], owner),
                                places,
                                new List<string>() { "YuanHu" },
                                new List<int>() { 1 },
                                new RequireOneCardChoiceVerifier(),
                                out answer))
                            {
                                answer = new List<List<Card>>();
                                answer[0].Add(players[1].HandCards().Concat(players[1].Equipments()).First());
                            }
                            Game.CurrentGame.HandleCardDiscard(players[1], answer[0]);
                        }
                        break;
                    }
                case CardCategory.Armor:
                    {
                        Game.CurrentGame.DrawCards(players[0], 1);
                        break;
                    }
                case CardCategory.DefensiveHorse:
                case CardCategory.OffensiveHorse:
                    {
                        Game.CurrentGame.RecoverHealth(owner, players[0], 1);
                        break;
                    }
                default:
                    break;
            }
        }

        public YuanHu()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                   this,
                   Run,
                   TriggerCondition.OwnerIsSource,
                   new YuanHuVerifier()
               );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
        }
    }
}
