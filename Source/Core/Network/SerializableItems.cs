using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Network
{
    [Serializable]
    public enum Command
    {
        WhoAmI,
        QaId,
        GameStart,
        Interrupt,
    }

    [Serializable]
    public struct CardItem
    {
        public int playerId;
        public DeckType deck;
        public int place;
        public int rank;
        public int suit;
        public int Id;
        public Type type;
        public string typeHorseName;
    }

    [Serializable]
    public struct CommandItem
    {
        public Command command;
        public int data;
        public object obj;
    }

    [Serializable]
    public struct PlayerItem
    {
        public int id;
    }

    [Serializable]
    public struct SkillItem
    {
        public int playerId;
        public int skillId;
        public string name;
        public Type additionalType;
        public string additionalTypeHorseName;
    }

    [Serializable]
    public struct HandCardMovement
    {
        public int playerId;
        public int from;
        public int to;
    }

    [Serializable]
    public struct CardChoiceCallback
    {
        public object o;
    }

    [Serializable]
    public struct CardUsageResponded
    {
        public int playerId;
    }

    public class Translator
    {
        public static SkillItem Translate(ISkill skill)
        {
            SkillItem item = new SkillItem();
            item.playerId = skill.Owner.Id;
            item.name = skill.GetType().Name;
            item.skillId = Game.CurrentGame.Players[skill.Owner.Id].ActionableSkills.IndexOf(skill);
            Trace.Assert(item.skillId >= 0);
            if (skill is IAdditionalTypedSkill)
            {
                item.additionalType = (skill as IAdditionalTypedSkill).AdditionalType.GetType();
            }
            return item;
        }

        public static CardItem TranslateForClient(Card card)
        {
            CardItem item = new CardItem();
            item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
            item.deck = card.Place.DeckType;
            item.place = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
            Translator.TranslateCardType(ref item.type, ref item.typeHorseName, card.Type);
            Trace.Assert(item.place >= 0);
            item.rank = card.Rank;
            item.suit = (int)card.Suit;
            item.Id = card.Id;
            return item;
        }

        public static CardItem TranslateForServer(Card card, int wrt)
        {
            CardItem item = new CardItem();
            item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
            item.deck = card.Place.DeckType;
            item.place = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
            Translator.TranslateCardType(ref item.type, ref item.typeHorseName, card.Type);
            Trace.Assert(item.place >= 0);
            item.rank = card.Rank;
            item.suit = (int)card.Suit;
            item.Id = card.Id;

            // this is a card that the client knows. keep the id anyway
            if (card.Place.Player != null && item.deck == DeckType.Hand && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(card.Place.Player))
            {
                card.RevealOnce = false;
            }
            else
            {
                if (!card.RevealOnce)
                {
                    item.Id = -1;
                }
                card.RevealOnce = false;
            }                
            return item;
        }

        public static Card DecodeForServer(CardItem i, int wrt)
        {
            // you know this hand card. therefore the deserialization look up this card in particular
            if (i.playerId >= 0 && i.deck == DeckType.Hand && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(Game.CurrentGame.Players[i.playerId]))
            {
                foreach (Card c in Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck])
                {
                    if (c.Id == i.Id)
                    {
                        return c;
                    }
                }
                Trace.Assert(false);
            }
            Card ret;
            if (i.playerId < 0)
            {
                ret = Game.CurrentGame.Decks[null, i.deck][i.place];
            }
            else
            {
                ret = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
            }
            return ret;
        }

        public static Card DecodeForClient(CardItem i, int wrt)
        {
            // you know this hand card. therefore the deserialization look up this card in particular
            if (i.playerId >= 0 && i.deck == DeckType.Hand && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(Game.CurrentGame.Players[i.playerId]))
            {
                foreach (Card c in Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck])
                {
                    if (c.Id == i.Id)
                    {
                        return c;
                    }
                }
                Trace.Assert(false);
            }
            if (i.Id >= 0)
            {
                Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, i.deck, i.place, i.suit, i.rank, i.type);
                Card gameCard;
                if (i.playerId < 0)
                {
                    gameCard = Game.CurrentGame.Decks[null, i.deck][i.place];
                }
                else
                {
                    gameCard = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
                }
                var place = gameCard.Place;
                gameCard.CopyFrom(GameEngine.CardSet[i.Id]);
                gameCard.Place = place;
                gameCard.Rank = i.rank;
                gameCard.Suit = (SuitType)i.suit;
                if (i.type != null) gameCard.Type = Translator.TranslateCardType(i.type, i.typeHorseName);
            }
            Card ret;
            if (i.playerId < 0)
            {
                ret = Game.CurrentGame.Decks[null, i.deck][i.place];
            }
            else
            {
                ret = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], i.deck][i.place];
            }
            return ret;
        }

        public static CardHandler TranslateCardType(Type type, string horseName)
        {
            if (type == null) return null;
            if (horseName == null)
            {
                return Activator.CreateInstance(type) as CardHandler;
            }
            else
            {
                return Activator.CreateInstance(type, horseName) as CardHandler;
            }
        }

        public static void TranslateCardType(ref Type type, ref String horse, CardHandler handler)
        {
            if (handler is RoleCardHandler || handler is Heroes.HeroCardHandler || handler == null)
            {
                type = null;
                horse = null;
                return;
            }
            type = handler.GetType();
            if (handler is OffensiveHorse || handler is DefensiveHorse) horse = handler.CardType;
            else horse = null;
        }

        public static ISkill Translate(SkillItem item)
        {
            if (item.playerId >= 0 && item.playerId < Game.CurrentGame.Players.Count)
            {
                if (Game.CurrentGame.Players[item.playerId].ActionableSkills.Count <= item.skillId)
                {
                    Trace.TraceWarning("Client sending invalid skills");
                    return null;
                }

                ISkill skill = Game.CurrentGame.Players[item.playerId].ActionableSkills[item.skillId];
#if DEBUG
                Trace.Assert(item.name == skill.GetType().Name);
#endif
                if (skill is IAdditionalTypedSkill)
                {
                    (skill as IAdditionalTypedSkill).AdditionalType = TranslateCardType(item.additionalType, item.additionalTypeHorseName);
                }
                return skill;
            }
            return null;
        }
    }
}
