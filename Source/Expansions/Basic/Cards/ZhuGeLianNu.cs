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
    public class ZhuGeLianNu : Equipment
    {
        private Trigger trigger1, trigger2;

        protected override void RegisterEquipmentTriggers(Player p)
        {
            trigger1 = new ZhuGeLianNuTrigger(p);
            trigger2 = new ZhuGeLianNuAlwaysShaTrigger(p);
            Game.CurrentGame.RegisterTrigger(Sha.PlayerShaTargetValidation, trigger1);
            Game.CurrentGame.RegisterTrigger(Sha.PlayerNumberOfShaCheck, trigger2);
        }

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            Game.CurrentGame.UnregisterTrigger(Sha.PlayerShaTargetValidation, trigger1);
            Game.CurrentGame.UnregisterTrigger(Sha.PlayerNumberOfShaCheck, trigger2);
            trigger1 = null;
            trigger2 = null;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Weapon; }
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        class ZhuGeLianNuTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner)
                {
                    return;
                }
                args.TargetApproval[0] = true;
            }
            public ZhuGeLianNuTrigger(Player p)
            {
                Owner = p;
            }
        }

        class ZhuGeLianNuAlwaysShaTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == Owner)
                {
                    throw new TriggerResultException(TriggerResult.Success);
                }
            }
            public ZhuGeLianNuAlwaysShaTrigger(Player p)
            {
                Owner = p;
            }
        }

    }
}
