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
    /// 鸡肋―当你受到伤害时，说出一种牌的类别（基本牌、锦囊牌、装备牌），对你造成伤害的角色不能使用、打出或弃掉该类别的手牌直到回合结束。
    /// </summary>
    public class JiLei : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            int answer = 0;
            List<OptionPrompt> JiLeiQuestion = new List<OptionPrompt>();
            JiLeiQuestion.Add(new OptionPrompt("JiBen"));
            JiLeiQuestion.Add(new OptionPrompt("JinNang"));
            JiLeiQuestion.Add(new OptionPrompt("ZhuangBei"));
            JiLeiQuestion.Add(Prompt.NoChoice);
            if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(
                new MultipleChoicePrompt("JiLei", eventArgs.Source), JiLeiQuestion, out answer))
            {
                Trace.Assert(answer >= 0 && answer <= 3);
                if (answer != 3)
                {
                    NotifySkillUse(new List<Player>());
                    eventArgs.Source[JiLeiInEffect] = 1;
                    JiLeiImplementation trigger = new JiLeiImplementation(eventArgs.Source, answer);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanDiscardCard, trigger);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, trigger);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanPlayCard, trigger);
                    JiLeiRemoval trigger2 = new JiLeiRemoval(eventArgs.Source, trigger);
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], trigger2);
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("JiLei", eventArgs.Source, JiLeiQuestion[answer]), new List<Player>() { Owner, eventArgs.Source });
                }
            }
        }

        public static readonly PlayerAttribute JiLeiInEffect = PlayerAttribute.Register("JiLei", false, false, true);
        class JiLeiImplementation : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                List<Card> toExamine = new List<Card>();
                if (eventArgs.Card is Card)
                {
                    toExamine.Add(eventArgs.Card as Card);
                }
                if (eventArgs.Card is CompositeCard)
                {
                    if ((eventArgs.Card as CompositeCard).Subcards != null)
                    {
                        toExamine.AddRange((eventArgs.Card as CompositeCard).Subcards);
                    }
                }
                foreach (Card c in toExamine)
                {
                    if (c.Place.DeckType == DeckType.Hand)
                    {
                        if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Basic) && type == 0)
                        {
                            throw new TriggerResultException(TriggerResult.Fail);
                        }
                        if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Tool) && type == 1)
                        {
                            throw new TriggerResultException(TriggerResult.Fail);
                        }
                        if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Equipment) && type == 2)
                        {
                            throw new TriggerResultException(TriggerResult.Fail);
                        }
                    }
                }
            }

            int type;
            public JiLeiImplementation(Player p, int type)
            {
                Owner = p;
                this.type = type;
            }
        }

        class JiLeiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, theTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanPlayCard, theTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanDiscardCard, theTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
                eventArgs.Source[JiLeiInEffect] = 0;
            }

            JiLeiImplementation theTrigger;
            public JiLeiRemoval(Player p, JiLeiImplementation t)
            {
                Owner = p;
                theTrigger = t;
            }
        }

        public JiLei()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                Run,
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.DamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}
