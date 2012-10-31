using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public interface INotificationProxy
    {
        void NotifyCardMovement(List<CardsMovement> m, List<UI.IGameLog> notes);
        void NotifyDamage(Player source, Player target, int magnitude);
        void NotifySkillUse(ActionLog log);
    }

    public class DummyNotificationProxy : INotificationProxy
    {
        public void NotifyCardMovement(List<CardsMovement> m, List<IGameLog> notes)
        {
        }

        public void NotifyDamage(Player source, Player target, int magnitude)
        {
        }

        public void NotifySkillUse(ActionLog log)
        {
        }
    }
}
