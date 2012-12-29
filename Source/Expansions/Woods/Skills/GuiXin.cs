using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 归心-每当你受到1点伤害后，你可以从所有其他角色区域里各获得一张牌，然后将武将牌翻面。
    /// </summary>
    public class GuiXin : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DamageEventArgs;
            int damage = args.Magnitude;
            while (damage-- > 0)
            {
                var players = Game.CurrentGame.AlivePlayers;
                players.Remove(Owner);
                Game.CurrentGame.SortByOrderOfComputation(Owner, players);
                bool invoke = false;
                foreach (Player p in players)
                {
                    if (p.HandCards().Count != 0
                         || p.Equipments().Count != 0
                         || p.DelayedTools().Count != 0)
                    {
                        invoke = true;
                        break;
                    }
                }
                if (invoke && AskForSkillUse())
                {
                    NotifySkillUse(new List<Player>());
                    foreach (Player p in players)
                    {
                        if (p.IsDead) continue;
                        if (p.HandCards().Count == 0
                            && p.Equipments().Count == 0
                            && p.DelayedTools().Count == 0)
                            continue;
                        List<List<Card>> answer;
                        List<DeckPlace> places = new List<DeckPlace>();
                        places.Add(new DeckPlace(p, DeckType.Hand));
                        places.Add(new DeckPlace(p, DeckType.Equipment));
                        places.Add(new DeckPlace(p, DeckType.DelayedTools));
                        if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("GuiXin", p, Owner), places,
                             new List<string>() { "GuiXin" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(), out answer))
                        {
                            answer = new List<List<Card>>();
                            answer.Add(new List<Card>());
                            answer[0].Add(Game.CurrentGame.Decks[p, DeckType.Hand][0]);
                        }
                        Card theCard = answer[0][0];
                        Game.CurrentGame.SyncCard(p, ref theCard);
                        Game.CurrentGame.HandleCardTransferToHand(p, Owner, new List<Card>() {theCard});
                    }
                    Owner.IsImprisoned = !Owner.IsImprisoned;
                }
            }
        }

        public GuiXin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        }
    }
}