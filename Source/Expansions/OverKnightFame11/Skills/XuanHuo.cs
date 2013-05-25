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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 眩惑-摸牌阶段，你可以放弃摸牌，改为令一名其他角色摸两张牌，然后该角色需对其攻击范围内，由你指定的另一名角色使用一张【杀】，否则你获得其两张牌。
    /// </summary>
    public class XuanHuo : TriggerSkill
    {
        class XuanHuoVerifier : CardsAndTargetsVerifier
        {
            public XuanHuoVerifier()
            {
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 2;
            }

            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (players.Count == 1 && players[0] == source) return false;
                if (players.Count == 1)
                {
                    var pl = Game.CurrentGame.AlivePlayers;
                    if (pl.Any(test => test != players[0] && Game.CurrentGame.DistanceTo(players[0], test) <= players[0][Player.AttackRange] + 1 &&
                        Game.CurrentGame.PlayerCanBeTargeted(players[0], new List<Player>() { test }, new CompositeCard() { Type = new Sha() })))
                    { 
                        return null;
                    }
                }
                if (players.Count == 2)
                {
                    if (!Game.CurrentGame.PlayerCanBeTargeted(players[0], new List<Player>() { players[1] }, new CompositeCard() { Type = new Sha() }))
                    {
                        return false;
                    }
                    if (Game.CurrentGame.DistanceTo(players[0], players[1]) > players[0][Player.AttackRange] + 1) return false;
                }
                return true;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            NotifySkillUse(players);
            Game.CurrentGame.DrawCards(players[0], 2);
            ISkill skill;
            List<Card> nCard;
            List<Player> nPlayer;
            while (true)
            {
                if (players.Count == 2 && !players[1].IsDead) Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToUseCard, new GameEventArgs() { Source = players[0] });
                if (players.Count == 2 && !players[1].IsDead && players[0].AskForCardUsage(new CardUsagePrompt("XuanHuoSha", players[1]), new JieDaoShaRen.JieDaoShaRenVerifier(players[1]), out skill, out nCard, out nPlayer))
                {
                    try
                    {
                        players[0][Sha.NumberOfShaUsed]--;
                        GameEventArgs args = new GameEventArgs();
                        args.Source = players[0];
                        args.Targets = nPlayer;
                        args.Targets.Add(players[1]);
                        args.Skill = skill;
                        args.Cards = nCard;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    }
                    catch (TriggerResultException e)
                    {
                        Trace.Assert(e.Status == TriggerResult.Retry);
                        continue;
                    }
                }
                else
                {
                    int cardCount = players[0].HandCards().Count + players[0].Equipments().Count;
                    if (cardCount == 0) break;
                    List<Card> toGet = new List<Card>();
                    if (cardCount <= 2)
                    {
                        toGet.AddRange(players[0].HandCards());
                        toGet.AddRange(players[0].Equipments());
                    }
                    else
                    {
                        List<List<Card>> answer;
                        List<DeckPlace> sourcePlace = new List<DeckPlace>();
                        sourcePlace.Add(new DeckPlace(players[0], DeckType.Hand));
                        sourcePlace.Add(new DeckPlace(players[0], DeckType.Equipment));
                        if (!Owner.AskForCardChoice(new CardChoicePrompt("XuanHuo", players[0], Owner),
                            sourcePlace,
                            new List<string>() { "HuoDe" },
                            new List<int>() { 2 },
                            new RequireCardsChoiceVerifier(2, true),
                            out answer,
                            null,
                            CardChoiceCallback.GenericCardChoiceCallback))
                        {
                            answer = new List<List<Card>>();
                            answer.Add(Game.CurrentGame.PickDefaultCardsFrom(sourcePlace, 2));
                        }
                        Game.CurrentGame.SyncImmutableCards(Owner, answer[0]);
                        toGet = answer[0];
                    }
                    Game.CurrentGame.HandleCardTransferToHand(players[0], Owner, toGet);
                }
                break;
            }
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
        }

        public XuanHuo()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource,
                new XuanHuoVerifier()
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }
    }
}
