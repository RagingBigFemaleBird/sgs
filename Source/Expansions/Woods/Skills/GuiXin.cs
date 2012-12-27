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
            List<Player> players = Game.CurrentGame.AlivePlayers;
            players.Remove(Owner);
            Game.CurrentGame.EnterAtomicContext();
            bool invoke = false;
            foreach (Player p in players)
            {
                List<DeckPlace> places = new List<DeckPlace>();
                if (Game.CurrentGame.Decks[p, DeckType.Hand].Count != 0
                     || Game.CurrentGame.Decks[p, DeckType.Equipment].Count != 0
                     || Game.CurrentGame.Decks[p, DeckType.DelayedTools].Count != 0)
                {
                    invoke = true;
                    break;
                }
            }
            if (invoke
                && AskForSkillUse())
            {
                foreach (Player p in players)
                {
                    List<List<Card>> answer;
                    List<DeckPlace> places = new List<DeckPlace>();
                    places.Add(new DeckPlace(p, DeckType.Hand));
                    places.Add(new DeckPlace(p, DeckType.Equipment));
                    places.Add(new DeckPlace(p, DeckType.DelayedTools));
                    if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("GuiXin"), places,
                         new List<string>() { "GuiXin" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(), out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(Game.CurrentGame.Decks[p, DeckType.Hand][0]);
                    }
                    Game.CurrentGame.HandleCardTransferToHand(p, Owner, answer[0]);
                }
                Owner.IsImprisoned = !Owner.IsImprisoned;
            }
            Game.CurrentGame.ExitAtomicContext();
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
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