using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{

    public class FangTianHuaJi : Weapon
    {
        public FangTianHuaJi()
        {
            EquipmentSkill = new FangTianHuaJiSkill() { ParentEquipment = this };
        }


        class FangTianHuaJiSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                List<Card> theList;
                if (args.Card is CompositeCard)
                {
                    theList = (args.Card as CompositeCard).Subcards;
                }
                else
                {
                    theList = new List<Card>() { (args.Card as Card) };
                }
                List<Card> handCards = new List<Card>(Game.CurrentGame.Decks[Owner, DeckType.Hand]);
                //你的"最后"一张手牌
                if (handCards.Count > 0)
                {
                    return;
                }
                //你的最后"一张"手牌
                if (theList.Count == 0)
                {
                    return;
                }
                foreach (Card c in theList)
                {
                    //"你的"最后一张"手牌"
                    if (c.Owner != Owner || c.Place.DeckType != DeckType.Hand)
                    {
                        return;
                    }
                }
                if (args.TargetApproval[0] == false)
                {
                    return;
                }
                int moreTargetsToApprove = 2;
                int i = 1;
                while (moreTargetsToApprove > 0 && i < args.TargetApproval.Count)
                {
                    if (args.TargetApproval[i] == true)
                    {
                        i++;
                        continue;
                    }
                    args.TargetApproval[i] = true;
                    i++;
                    moreTargetsToApprove--;
                    continue;

                }
            }
            public FangTianHuaJiSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        bool isLastHandCard = false;
                        Card c = a.Card as Card;
                        if (c != null) isLastHandCard = c[Card.IsLastHandCard] == 1;
                        else
                        {
                            CompositeCard cc = a.Card as CompositeCard;
                            Trace.Assert(cc != null);
                            isLastHandCard = cc.Subcards.All(card => card.HistoryPlace1.DeckType == DeckType.Hand) && cc.Subcards.Any(card => card[Card.IsLastHandCard] == 1);
                        }
                        return a.Targets.Count > 1 && isLastHandCard && a.Card.Type is Sha;
                    },
                    (p, e, a) => { },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false };
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    Run,
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false, AskForConfirmation = false, Type = TriggerType.Card};
                Triggers.Add(GameEvent.PlayerUsedCard, trigger);
                Triggers.Add(Sha.PlayerShaTargetValidation, trigger2);
            }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        public override int AttackRange
        {
            get { return 4; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
        }

    }
}
