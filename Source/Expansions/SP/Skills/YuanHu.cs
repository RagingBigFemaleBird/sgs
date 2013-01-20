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
    /// <summary>
    /// 援护-回合结束阶段开始时，你可以将一张装备牌置于一名角色的装备区里，然后根据此装备牌的种类执行以下效果。
    /// 武器牌：弃置与该角色距离为1的一名角色区域中的一张牌；
    /// 防具牌：该角色摸一张牌；
    /// 坐骑牌：该角色回复1点体力。
    /// </summary>
    public class YuanHu : TriggerSkill
    {
        class YuanHuVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if ((cards == null || cards.Count == 0) && players != null && players.Count > 0)
                {
                    return VerifierResult.Fail;
                }
                if (skill != null)
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count == 1 && !cards[0].Type.IsCardCategory(CardCategory.Equipment))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (players[0].Equipments().Any(c => c.Type.IsCardCategory(cards[0].Type.Category)))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        class YuanHuChoiceOnePlayer : CardsAndTargetsVerifier
        {
            public YuanHuChoiceOnePlayer(Player target)
            {
                MaxPlayers = 1;
                MaxCards = 0;
                this.target = target;
            }

            Player target;
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return Game.CurrentGame.DistanceTo(player, target) == 1;
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
                        var result = from player in Game.CurrentGame.AlivePlayers where player != players[0] && Game.CurrentGame.DistanceTo(players[0], player) == 1 select player;
                        if (result.Count() == 0) break;
                        ISkill skill;
                        List<Card> nCards;
                        List<Player> nPlayers;
                        if (!owner.AskForCardUsage(new CardUsagePrompt("YuanHuQiZhi"), new YuanHuChoiceOnePlayer(players[0]), out skill, out nCards, out nPlayers))
                        {
                            nPlayers = new List<Player>();
                            nPlayers.Add(result.First());
                        }
                        var Card = Game.CurrentGame.SelectACardFrom(nPlayers[0], owner, new CardChoicePrompt("YuanHu", nPlayers[0], owner), "YuanHu", false, false);
                        Game.CurrentGame.HandleCardDiscard(nPlayers[0], new List<Card>() { Card });
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
