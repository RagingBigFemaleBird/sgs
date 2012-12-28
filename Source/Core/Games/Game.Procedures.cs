using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;


namespace Sanguosha.Core.Games
{
    public abstract partial class Game
    {
        void _FilterCard(Player p, Card card)
        {
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            args.Card = card;
            Emit(GameEvent.EnforcedCardTransform, args);
        }

        void _ResetCard(Card card)
        {
            if (card.Id > 0)
            {
                card.Type = GameEngine.CardSet[card.Id].Type;
                card.Suit = GameEngine.CardSet[card.Id].Suit;
                card.Rank = GameEngine.CardSet[card.Id].Rank;
            }
        }

        void _ResetCards(Player p)
        {
            foreach (var card in decks[p, DeckType.Hand])
            {
                if (card.Id > 0)
                {
                    _ResetCard(card);
                    _FilterCard(p, card);
                }
            }
        }

        public void PlayerAcquireSkill(Player p, ISkill skill, bool undeletable = false)
        {
            p.AcquireAdditionalSkill(skill, undeletable);
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public void PlayerLoseSkill(Player p, ISkill skill, bool undeletable = false)
        {
            p.LoseAdditionalSkill(skill, undeletable);
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public int NumberOfAliveAllegiances
        {
            get
            {
                var ret =
                (from p in Game.CurrentGame.AlivePlayers select p.Allegiance).Distinct().Count();
                return ret;
            }
        }

        public void HandleGodHero(Player p)
        {
            if (p.Allegiance == Heroes.Allegiance.God)
            {
                int answer = 0;
                Game.CurrentGame.UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("ChooseAllegiance"), Prompt.AllegianceChoices, out answer);
                if (answer == 0) p.Allegiance = Heroes.Allegiance.Qun;
                if (answer == 1) p.Allegiance = Heroes.Allegiance.Shu;
                if (answer == 2) p.Allegiance = Heroes.Allegiance.Wei;
                if (answer == 3) p.Allegiance = Heroes.Allegiance.Wu;
            }
        }
    }
}
