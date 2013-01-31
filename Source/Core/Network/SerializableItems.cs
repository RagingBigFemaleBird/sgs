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
        Detach,
        Attach,
    }

    public enum ItemType
    {
        Int,
        CardItem,
        CommandItem,
        Player,
        SkillItem,
        HandCardMovement,
        CardRearrangement,
        CardUsageResponded,
        ValueType,
        Serializable,
    }

    [Serializable]
    public struct CardItem
    {
        public int playerId;
        public int place;
        public int rank;
        public int suit;
        public int id;
        public string deckName;
        public string typeName;
        public string typeHorseName;
    }

    [Serializable]
    public struct CommandItem
    {
        public Command command;
        public ItemType type;
        public object data;
    }

    [Serializable]
    public struct SkillItem
    {
        public int playerId;
        public int skillId;
        public string name;
        public string additionalTypeName;
        public string additionalTypeHorseName;
    }

    [Serializable]
    public struct CardUsageResponded
    {
        public int playerId;
    }

    public class Translator
    {
        public static SkillItem EncodeSkill(ISkill skill)
        {
            SkillItem item = new SkillItem();
            item.playerId = skill.Owner.Id;
            item.name = skill.GetType().Name;
            item.skillId = Game.CurrentGame.Players[skill.Owner.Id].ActionableSkills.IndexOf(skill);
            Trace.Assert(item.skillId >= 0);
            if (skill is IAdditionalTypedSkill)
            {
                var type = (skill as IAdditionalTypedSkill).AdditionalType;
                string horseName;
                EncodeCardHandler(type, out item.additionalTypeName, out horseName);
            }
            return item;
        }

        public static CardItem EncodeCard(Card card)
        {
            CardItem item = new CardItem();
            item.playerId = Game.CurrentGame.Players.IndexOf(card.Place.Player);
            item.deckName = card.Place.DeckType.Name;
            item.place = Game.CurrentGame.Decks[card.Place.Player, card.Place.DeckType].IndexOf(card);
            Translator.EncodeCardHandler(card.Type, out item.typeName, out item.typeHorseName);
            Trace.Assert(item.place >= 0);
            item.rank = card.Rank;
            item.suit = (int)card.Suit;
            item.id = card.Id;
            return item;
        }

        public static CardItem EncodeServerCard(Card card, int wrt)
        {
            CardItem item = EncodeCard(card);

            if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
            {
                item.id = -1;
            }
            // this is a card that the client knows. keep the id anyway
            else if (card.Place.Player != null && card.Place.DeckType == DeckType.Hand && wrt < Game.CurrentGame.Players.Count && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(card.Place.Player))
            {
            }
            else if (!card.RevealOnce)
            {
                item.id = -1;
            }
            card.RevealOnce = false;
            return item;
        }

        public static Card DecodeServerCard(CardItem i, int wrt)
        {
            DeckType deck = new DeckType(i.deckName);
            // you know this hand card. therefore the deserialization look up this card in particular
            if (i.playerId >= 0 && deck == DeckType.Hand && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(Game.CurrentGame.Players[i.playerId]))
            {
                foreach (Card c in Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], deck])
                {
                    if (c.Id == i.id)
                    {
                        return c;
                    }
                }
                Trace.Assert(false);
            }
            Card ret;
            if (i.playerId < 0)
            {
                ret = Game.CurrentGame.Decks[null, deck][i.place];
            }
            else
            {
                ret = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], deck][i.place];
            }
            return ret;
        }

        public static Card DecodeCard(CardItem i, int wrt)
        {
            DeckType deck = new DeckType(i.deckName);
            // you know this hand card. therefore the deserialization look up this card in particular
            if (i.id >= 0 && i.playerId >= 0 && deck == DeckType.Hand && wrt < Game.CurrentGame.Players.Count && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(Game.CurrentGame.Players[i.playerId]))
            {
                foreach (Card c in Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], deck])
                {
                    if (c.Id == i.id)
                    {
                        return c;
                    }
                }
                Trace.Assert(false);
            }
            if (i.id >= 0)
            {
                Trace.TraceInformation("Identify {0}{1}{2} is {3}{4}{5}", i.playerId, deck, i.place, i.suit, i.rank, i.typeName);
                Card gameCard;
                if (i.playerId < 0)
                {
                    gameCard = Game.CurrentGame.Decks[null, deck][i.place];
                }
                else
                {
                    gameCard = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], deck][i.place];
                }
                var place = gameCard.Place;
                gameCard.CopyFrom(GameEngine.CardSet[i.id]);
                gameCard.Place = place;
                gameCard.Rank = i.rank;
                gameCard.Suit = (SuitType)i.suit;
                if (!string.IsNullOrEmpty(i.typeName))
                {
                    gameCard.Type = Translator.DecodeCardHandler(i.typeName, i.typeHorseName);
                }
            }
            Card ret;
            if (i.playerId < 0)
            {
                ret = Game.CurrentGame.Decks[null, deck][i.place];
            }
            else
            {
                ret = Game.CurrentGame.Decks[Game.CurrentGame.Players[i.playerId], deck][i.place];
            }
            return ret;
        }

        public static CardHandler DecodeCardHandler(string typeName, string horseName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            CardHandler ret;
            if (string.IsNullOrEmpty(horseName))
            {
                ret = Activator.CreateInstance(Type.GetType(typeName)) as CardHandler;
            }
            else
            {
                ret = Activator.CreateInstance(Type.GetType(typeName), horseName) as CardHandler;
            }
            
            return ret;
        }

        public static void EncodeCardHandler(CardHandler handler, out string typeName, out string horse)
        {
            if (handler is RoleCardHandler || handler is Heroes.HeroCardHandler || handler == null)
            {
                typeName = string.Empty;
                horse = string.Empty;
                return;
            }

            typeName = handler.GetType().AssemblyQualifiedName;

            if (handler is OffensiveHorse || handler is DefensiveHorse) horse = handler.CardType;
            else horse = string.Empty;
        }

        public static ISkill EncodeSkill(SkillItem item)
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
                    (skill as IAdditionalTypedSkill).AdditionalType = DecodeCardHandler(item.additionalTypeName, item.additionalTypeHorseName);
                }
                return skill;
            }
            return null;
        }
    }
}
