using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.UI
{
    public delegate bool JudgementResultSucceed(ICard card);

    public interface INotificationProxy
    {
        void NotifyGameStart();
        void NotifyCardMovement(List<CardsMovement> m);
        void NotifyDamage(Player source, Player target, int magnitude, DamageElement element);
        void NotifySkillUse(ActionLog log);
        void NotifyMultipleChoiceResult(Player p, OptionPrompt answer);
        void NotifyJudge(Player p, Card card, ActionLog log, bool? isSuccess, bool finalResult = true);
        void NotifyDeath(Player p, Player by);
        void NotifyGameOver(bool isDraw, List<Player> winners);
        void NotifyActionComplete();
        void NotifyLoseHealth(Player player, int p);
        void NotifyRecoverHealth(Player player, int p);
        void NotifyReforge(Player p, ICard card);
        void NotifyLogEvent(Prompt custom, List<Player> players = null, bool isKeyEvent = true, bool useUICard = true);
        void NotifyShowCard(Player p, Card card);
        void NotifyCardChoiceCallback(CardRearrangement o);
        void NotifyImpersonation(Player p, Hero impersonator, Hero impersonatedHero, ISkill acquiredSkill);
        void NotifyWuGuStart(Prompt prompt, DeckPlace place);
        void NotifyWuGuEnd();
        void NotifyPinDianStart(Player from, Player to, ISkill skill);
        void NotifyMultipleCardUsageResponded(Player player);
        void NotifyPinDianEnd(Card c1, Card c2);
        void NotifyShowCardsStart(Player p, List<Card> cards);
        void NotifyShowCardsEnd();
        void NotifyUiAttached();
        void NotifyUiDetached();
    }

    public class DummyNotificationProxy : INotificationProxy
    {
        public void NotifyCardMovement(List<CardsMovement> m)
        {
        }

        public void NotifyDamage(Player source, Player target, int magnitude, DamageElement element)
        {
        }

        public void NotifySkillUse(ActionLog log)
        {
        }

        public void NotifyMultipleChoiceResult(Player p, OptionPrompt answer)
        {
        }

        public void NotifyDeath(Player p, Player by)
        {
        }

        public void NotifyGameOver(bool isDraw, List<Player> winners)
        {
        }

        public void NotifyActionComplete()
        {
        }

        public void NotifyLoseHealth(Player player, int p)
        {
        }

        public void NotifyRecoverHealth(Player player, int p)
        {
        }

        public void NotifyReforge(Player p, ICard card)
        {
        }

        public void NotifyLogEvent(Prompt custom, List<Player> players = null, bool isKeyEvent = true, bool useUICard = true)
        {
        }

        public void NotifyShowCard(Player p, Card card)
        {
        }

        public void NotifyImpersonation(Player p, Hero impersonator, Hero impersonatedHero, ISkill acquiredSkill)
        {
        }

        public void NotifyJudge(Player p, Card card, ActionLog log, bool? isSuccess, bool f)
        {
        }

        public void NotifyGameStart()
        {            
        }

        public void NotifyWuGuStart(Prompt prompt, DeckPlace place)
        {
        }

        public void NotifyWuGuEnd()
        {
        }

        public void NotifyPinDianStart(Player from, Player to, ISkill skill)
        {
        }

        public void NotifyMultipleCardUsageResponded(Player player)
        {
        }

        public void NotifyPinDianEnd(Card c1, Card c2)
        {
        }
        
        public void NotifyCardChoiceCallback(CardRearrangement o)
        {
        }


        public void NotifyShowCardsStart(Player p, List<Card> cards)
        {
        }

        public void NotifyShowCardsEnd()
        {
        }


        public void NotifyUiAttached()
        {
        }

        public void NotifyUiDetached()
        {
        }
    }
}
