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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Woods.Skills;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 志继—觉醒技，回合开始阶段开始时，若你没有手牌，你须回复1点体力或摸两张牌，然后减1点体力上限，并获得技能“观星”。
    /// </summary>
    public class ZhiJi : TriggerSkill
    {
        public ZhiJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[ZhiJiAwaken] == 0 && Game.CurrentGame.Decks[p, DeckType.Hand].Count == 0; },
                (p, e, a) =>
                {
                    p[ZhiJiAwaken] = 1;
                    int answer = 0;
                    Game.CurrentGame.UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("ZhiJi"), new List<OptionPrompt>() { new OptionPrompt("MoPai"), new OptionPrompt("HuiFuTiLi") }, out answer);
                    if (answer == 1)
                    {
                        Game.CurrentGame.RecoverHealth(p, p, 1);
                    }
                    else
                    {
                        Game.CurrentGame.DrawCards(p, 2);
                    }
                    Game.CurrentGame.LoseMaxHealth(p, 1);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new GuanXing(), HeroTag);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }

        public static PlayerAttribute ZhiJiAwaken = PlayerAttribute.Register("ZhiJiAwaken", false);

    }
}
