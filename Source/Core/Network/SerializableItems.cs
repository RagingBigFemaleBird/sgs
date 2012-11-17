using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;

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
    public struct InterruptedObject
    {
        public object obj;
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
        public string name;
        public Type additionalType;
        public string additionalTypeHorseName;
    }

    public class Translator
    {
        public static SkillItem Translate(ISkill skill)
        {
            SkillItem item = new SkillItem();
            item.playerId = skill.Owner.Id;
            item.name = skill.GetType().Name;
            if (skill is IAdditionalTypedSkill)
            {
                item.additionalType = (skill as IAdditionalTypedSkill).AdditionalType.GetType();
            }
            return item;
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
                foreach (var skill in Game.CurrentGame.Players[item.playerId].ActionableSkills)
                {
                    if (skill.GetType().Name.Equals(item.name))
                    {
                        if (skill is IAdditionalTypedSkill)
                        {
                            (skill as IAdditionalTypedSkill).AdditionalType = TranslateCardType(item.additionalType, item.additionalTypeHorseName);
                        }
                        return skill;
                    }
                }
            }
            return null;
        }
    }
}
