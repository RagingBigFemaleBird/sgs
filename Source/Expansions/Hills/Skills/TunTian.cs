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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 屯田-你的回合外，每当失去牌时，可进行一次判定，将非红桃结果的判定牌置于你的武将牌上，称为“田”；每有一张"田"，你计算与其他角色的距离便-1。
    /// </summary>
    public class TunTian : TriggerSkill
    {
        public static PrivateDeckType TianDeck = new PrivateDeckType("Tian", true);

        public class GetJudgeCardTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerJudgeDone, this);
                //someone already took it...
                if (Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult].Count == 0)
                {
                    return;
                }
                if (eventArgs.Card.Suit != SuitType.Heart)
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]);
                    move.To = new DeckPlace(Owner, TianDeck);
                    Game.CurrentGame.MoveCards(move);
                }
                return;
            }
            public GetJudgeCardTrigger(Player p)
            {
                Owner = p;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeDone, new GetJudgeCardTrigger(Owner) { Priority = int.MinValue });
            Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Heart; });
        }

        public TunTian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPlayer != p; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardsLost, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { var args = a as AdjustmentEventArgs; args.AdjustmentAmount = -Game.CurrentGame.Decks[p, TianDeck].Count; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerDistanceAdjustment, trigger2);
            IsAutoInvoked = true;
        }
    }
}
