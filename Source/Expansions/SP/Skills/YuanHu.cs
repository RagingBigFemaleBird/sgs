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
                MinPlayers = 1;
                MaxCards = 0;
                this.target = target;
            }

            Player target;
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return Game.CurrentGame.DistanceTo(target, player) == 1;
            }
        }

        void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (owner.AskForCardUsage(new CardUsagePrompt("YuanHu", this), new YuanHuVerifier(), out skill, out cards, out players))
            {
                CardCategory type = cards[0].Type.Category;
                YuanHuEffect = effectMap[type];
                NotifySkillUse(players);
                Game.CurrentGame.HandleCardTransfer(owner, players[0], DeckType.Equipment, cards);
                switch (type)
                {
                    case CardCategory.Weapon:
                        {
                            var result = from player in Game.CurrentGame.AlivePlayers where player != players[0] && Game.CurrentGame.DistanceTo(players[0], player) == 1 select player;
                            if (result.Count() == 0) break;
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
        }

        static Dictionary<CardCategory, int> effectMap = new Dictionary<CardCategory, int>() { 
        { CardCategory.Weapon, 0}, 
        { CardCategory.Armor, 1} ,
        { CardCategory.DefensiveHorse, 2},
        { CardCategory.OffensiveHorse, 2}
        };

        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            if (targets[0].Hero.Name.Contains("CaoCao") || (targets[0].Hero2 != null && targets[0].Hero2.Name.Contains("CaoCao"))) return 3;
            if (targets[0] == source) return 4;
            return YuanHuEffect;
        }

        public int YuanHuEffect { get; set; }
        public YuanHu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                   this,
                   Run,
                   TriggerCondition.OwnerIsSource
               ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
        }
    }
}
