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
    public enum GameResult
    {
        Ruler,
        Defector,
        Rebel,
    }
    public interface INotificationProxy
    {
        void NotifyCardMovement(List<CardsMovement> m, List<MovementHelper> notes);
        void NotifyDamage(Player source, Player target, int magnitude, DamageElement element);
        void NotifySkillUse(ActionLog log);
        void NotifyMultipleChoiceResult(Player p, string answer);
        void NotifyJudge(Player p, Card card, ActionLog log);
        void NotifyDeath(Player p, Player by);
        void NotifyGameOver(GameResult result);
        void NotifyActionComplete();
        void NotifyLoseHealth(Player player, int p);
        void NotifyShowCard(Player p, Card card);
        void NotifyCardChoiceCallback(object o);
    }

    public class DummyNotificationProxy : INotificationProxy
    {
        public void NotifyCardMovement(List<CardsMovement> m, List<MovementHelper> notes)
        {
        }

        public void NotifyDamage(Player source, Player target, int magnitude, DamageElement element)
        {
        }

        public void NotifySkillUse(ActionLog log)
        {
        }


        public void NotifyMultipleChoiceResult(Player p, string answer)
        {
        }


        public void NotifyJudge(Player p, Card card, ActionLog log)
        {
        }

        public void NotifyDeath(Player p, Player by)
        {
        }

        public void NotifyGameOver(GameResult result)
        {
        }

        public void NotifyActionComplete()
        {
        }


        public void NotifyLoseHealth(Player player, int p)
        {
        }

        public void NotifyShowCard(Player p, Card card)
        {
        }

        public void NotifyCardChoiceCallback(object o)
        {
        }
    }
}
