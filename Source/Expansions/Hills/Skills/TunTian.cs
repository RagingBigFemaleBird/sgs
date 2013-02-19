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
using Sanguosha.Core.Utils;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 屯田-你的回合外，每当失去牌时，可进行一次判定，将非红桃结果的判定牌置于你的武将牌上，称为“田”；每有一张"田"，你计算与其他角色的距离便-1。
    /// </summary>
    public class TunTian : TriggerSkill
    {
        public static PrivateDeckType TianDeck = new PrivateDeckType("Tian", true);
        public class TunTianGetJudgeCardTrigger : GetJudgeCardTrigger
        {
            Hero tag;
            protected override void GetJudgeCards(List<Card> list)
            {
                if (list[0].Suit == SuitType.Heart) return;
                GameDelays.Delay(GameDelayTypes.JudgeEnd);
                GameDelays.Delay(GameDelayTypes.JudgeEnd);
                CardsMovement move = new CardsMovement();
                move.Cards = new List<Card>(list);
                move.To = new DeckPlace(Owner, TianDeck);
                move.Helper.PrivateDeckHeroTag = tag;
                Game.CurrentGame.MoveCards(move);
            }
            public TunTianGetJudgeCardTrigger(Player p, ISkill s, ICard c, Hero tag) : base(p, s, c) { this.tag = tag; }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Cards.Any(cd => cd.HistoryPlace1.DeckType == DeckType.Hand || cd.HistoryPlace1.DeckType == DeckType.Equipment))
            {
                NotifySkillUse();
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeDone, new TunTianGetJudgeCardTrigger(Owner, this, null, HeroTag) { Priority = int.MinValue });
                Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Heart; });
            }
        }

        public TunTian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.PhasesOwner != p; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.CardsLost, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { var args = a as AdjustmentEventArgs; args.AdjustmentAmount = -Game.CurrentGame.Decks[p, TianDeck].Count; },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerDistanceAdjustment, trigger2);
            IsAutoInvoked = true;
            DeckCleanup.Add(TianDeck);
        }
    }
}
