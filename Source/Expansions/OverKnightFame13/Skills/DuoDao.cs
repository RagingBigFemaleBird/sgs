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

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 夺刀-你的出牌阶段，你可以和任意一名装备区有武器牌的武将拼点，若你赢，则获得其该武器牌：若你没赢，则你此回合不可以出杀。
    /// </summary>
    public class DuoDao : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[DuoDaoUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (Owner.Weapon() == null)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets[0] == Owner || Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public class DuoDaoLoseTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (eventArgs.Card.Type is Sha)
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }

            public DuoDaoLoseTrigger(Player p)
            {
                Owner = p;
            }
        }

        class DuoDaoRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            Trigger loseTrigger;
            public DuoDaoRemoval(Player p, Trigger lose)
            {
                Owner = p;
                loseTrigger = lose;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[DuoDaoUsed] = 1;
            var result = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            if (result == true)
            {
                var theWeapon = arg.Targets[0].Weapon();
                if (theWeapon != null)
                {
                    Game.CurrentGame.HandleCardTransferToHand(arg.Targets[0], Owner, new List<Card>() { theWeapon });
                }
            }
            else
            {
                var loseTrigger = new DuoDaoLoseTrigger(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new DuoDaoRemoval(Owner, loseTrigger));
            }
            return true;
        }

        private static PlayerAttribute DuoDaoUsed = PlayerAttribute.Register("DuoDaoUsed", true);
    }
}
