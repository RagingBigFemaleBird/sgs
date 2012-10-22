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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 鸡肋―当你受到伤害时，说出一种牌的类别（基本牌、锦囊牌、装备牌），对你造成伤害的角色不能使用、打出或弃掉该类别的手牌直到回合结束。
    /// </summary>
    public class JiLei : PassiveSkill
    {
        class JiLeiTrigger : Trigger
        {
            public Player Owner { get; set; }
            public class FanKuiVerifier : ICardChoiceVerifier
            {

                public VerifierResult Verify(List<List<Card>> answer)
                {
                    Trace.Assert(answer.Count == 1);
                    if (answer[0].Count < 1)
                    {
                        return VerifierResult.Partial;
                    }
                    return VerifierResult.Success;

                }
            }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                int answer = 0;
                List<string> JiLeiQuestion = new List<string>();
                JiLeiQuestion.Add(Prompt.MultipleChoiceOptionPrefix + "JiBen");
                JiLeiQuestion.Add(Prompt.MultipleChoiceOptionPrefix + "JinNang");
                JiLeiQuestion.Add(Prompt.MultipleChoiceOptionPrefix + "ZhuangBei");
                JiLeiQuestion.Add(Prompt.NoChoice);
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(
                    new MultipleChoicePrompt("JiLei"), JiLeiQuestion, out answer))
                {
                    Trace.Assert(answer >= 0 && answer <= 3);
                    if (answer != 3)
                    {
                        JiLeiImplementation trigger = new JiLeiImplementation(eventArgs.Source, answer);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanDiscardCard, trigger);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, trigger);
                        JiLeiRemoval trigger2 = new JiLeiRemoval(eventArgs.Source, trigger);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], trigger2);
                    }
                }
            }

            public JiLeiTrigger(Player p)
            {
                Owner = p;
            }
        }

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

            public Player Owner { get; set; }
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
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanDiscardCard, theTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            public Player Owner { get; set; }
            JiLeiImplementation theTrigger;
            public JiLeiRemoval(Player p, JiLeiImplementation t)
            {
                Owner = p;
                theTrigger = t;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageInflicted, new JiLeiTrigger(owner));
        }
    }
}
