using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProtoBuf;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.Network
{
    [ProtoContract]
    public class PlayerItem
    {
        [ProtoMember(1)]
        public byte PlayerId { get; set; }
        public static PlayerItem Parse(Player p)
        {
            if (p == null) return null;
            return new PlayerItem() { PlayerId = (byte)p.Id };
        }
        public Player ToPlayer()
        {
            return Game.CurrentGame.Players[PlayerId];
        }
    }

    [ProtoContract]
    [ProtoInclude(1021, typeof(AdditionalTypedSkillItem))]
    [ProtoInclude(1022, typeof(CheatSkillItem))]    
    public class SkillItem
    {
        [ProtoMember(1)]
        public PlayerItem PlayerItem { get; set; }
        [ProtoMember(2)]
        public byte SkillId { get; set; }

        public static SkillItem Parse(ISkill skill)
        {
            if (skill == null) return null;
            SkillItem result;
            if (skill is CheatSkill)
            {
                CheatSkillItem csi = new CheatSkillItem();
                CheatSkill cs = skill as CheatSkill;
                result = csi;
                csi.CardId = cs.CardId;
                csi.CheatType = (int)cs.CheatType;
                csi.SkillName = cs.SkillName;
            }
            else if (skill is IAdditionalTypedSkill)
            {
                var atsi = new AdditionalTypedSkillItem();
                var ats = skill as IAdditionalTypedSkill;
                result = atsi;
                atsi.AdditionalTypeId = GameEngine.Serialize(ats.AdditionalType);
            }
            else
            {
                result = new SkillItem();
            }

            result.PlayerItem = PlayerItem.Parse(skill.Owner);
            if (skill.Owner != null)
            {              
                result.SkillId = (byte)skill.Owner.ActionableSkills.IndexOf(skill);
            }            
            return result;
        }

        [ProtoContract]
        public virtual ISkill ToSkill()
        {
            if (PlayerItem != null)
            {
                var skills = PlayerItem.ToPlayer().ActionableSkills;
                if (skills.Count <= SkillId)
                {
                    return null;
                }

                ISkill skill = skills[SkillId];

                return skill;
            }
            return null;
        }
    }

    [ProtoContract]
    public class AdditionalTypedSkillItem : SkillItem
    {
        [ProtoMember(1)]
        public int AdditionalTypeId { get; set; }

        public override ISkill ToSkill()
        {
            var skill = base.ToSkill() as IAdditionalTypedSkill;
            Trace.Assert(skill != null);
            skill.AdditionalType = GameEngine.DeserializeCardHandler(AdditionalTypeId);
            return skill;
        }
    }

    [ProtoContract]
    public class CheatSkillItem : SkillItem
    {
        [ProtoMember(1)]
        public int CheatType { get; set; }
        [ProtoMember(2)]
        public int CardId { get; set; }
        [ProtoMember(3)]
        public string SkillName { get; set; }
        public override ISkill ToSkill()
        {
            var result = new CheatSkill();
            result.CheatType = (CheatType)CheatType;
            result.CardId = CardId;
            result.SkillName = SkillName;
            return result;
        }
    }

    [ProtoContract]
    public class DeckPlaceItem
    {
        [ProtoMember(1)]
        public PlayerItem PlayerItem { get; set; }
        [ProtoMember(2)]
        public string DeckName { get; set; }
        
        public static DeckPlaceItem Parse(DeckPlace dp)
        {
            DeckPlaceItem dpi = new DeckPlaceItem();
            dpi.PlayerItem = PlayerItem.Parse(dp.Player);
            dpi.DeckName = dp.DeckType.AbbriviatedName;
            return dpi;
        }
        public DeckPlace ToDeckPlace()
        {
            DeckPlace place = new DeckPlace(PlayerItem == null ? null : PlayerItem.ToPlayer(), DeckType.Register(DeckName));
            return place;
        }
    }

    [ProtoContract]
    [ProtoInclude(1031, typeof(CardByIdItem))]
    [ProtoInclude(1032, typeof(CardByPlaceItem))]    
    public abstract class CardItem
    {
        public abstract Card ToCard();
        public static CardItem Parse(Card card, int wrt)
        {
            if (card.Place.Player != null && card.Place.DeckType == DeckType.Hand && wrt < Game.CurrentGame.Players.Count && Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrt]].Contains(card.Place.Player))
            {
                return new CardByIdItem() { CardId = card.Id };
            }
            return CardByPlaceItem.Parse(card);
        }
    }

    [ProtoContract]
    public class CardByIdItem : CardItem
    {
        [ProtoMember(1)]
        public int CardId { get; set; }
        public override Card ToCard()
        {
            return GameEngine.CardSet[CardId];
        }
    }

    [ProtoContract]
    public class CardByPlaceItem : CardItem
    {
        [ProtoMember(1)]
        public DeckPlaceItem DeckPlaceItem { get; set; }
        [ProtoMember(2)]
        public int PlaceInDeck { get; set; }

        public static CardByPlaceItem Parse(Card card)
        {
            return new CardByPlaceItem()
            {
                DeckPlaceItem = DeckPlaceItem.Parse(card.Place),
                PlaceInDeck = Game.CurrentGame.Decks[card.Place].IndexOf(card)
            };
        }
        public override Card ToCard()
        {
            return Game.CurrentGame.Decks[DeckPlaceItem.ToDeckPlace()][PlaceInDeck];
        }
    }




    [ProtoContract]
    [ProtoInclude(1011, typeof(AskForCardUsageResponse))]
    [ProtoInclude(1012, typeof(AskForCardChoiceResponse))]
    [ProtoInclude(1013, typeof(AskForMultipleChoiceResponse))]
    public class GameDataPacket
    {
    }

    [ProtoContract]
    public class GameResponse : GameDataPacket
    {
        [ProtoMember(1)]
        public int Id { get; set; }
    }

    [ProtoContract]
    public class AskForCardUsageResponse : GameResponse
    {
        [ProtoMember(1)]
        SkillItem SkillItem { get; set; }
        [ProtoMember(2)]
        List<CardItem> CardItems { get; set; }
        [ProtoMember(3)]
        List<PlayerItem> PlayerItems { get; set; }
        
        public static AskForCardUsageResponse Parse(int id, ISkill skill, List<Card> cards, List<Player> players)
        {            
            AskForCardUsageResponse response = new AskForCardUsageResponse();
            response.Id = id;
            response.SkillItem = SkillItem.Parse(skill);
            response.CardItems = new List<CardItem>();
            foreach (var card in cards)
            {
                response.CardItems.Add(CardItem.Parse(card));
            }
            response.PlayerItems = new List<PlayerItem>();
            foreach (var player in players)
            {
                response.PlayerItems.Add(PlayerItem.Parse(player));
            }
            return response;
        }

        public void ToAnswer(out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            skill = SkillItem.ToSkill();
            cards = new List<Card>();
            foreach (var card in CardItems)
            {
                cards.Add(card.ToCard());
            }
            players = new List<Player>();
            foreach (var player in PlayerItems)
            {
                players.Add(player.ToPlayer());
            }
        }
    }

    [ProtoContract]
    public class AskForCardChoiceResponse : GameResponse
    {
        [ProtoMember(1)]
        List<List<CardItem>> CardItems { get; set; }

        public static AskForCardChoiceResponse Parse(int id, List<List<Card>> cards)
        {
            AskForCardChoiceResponse response = new AskForCardChoiceResponse();
            response.Id = id;            
            response.CardItems = new List<List<CardItem>>();
            foreach (var cardDeck in cards)
            {
                var items = new List<CardItem>();
                foreach (var card in cardDeck)
                {
                    items.Add(CardItem.Parse(card));
                }
                response.CardItems.Add(items);
            }            
            return response;
        }

        public void ToAnswer(out List<List<Card>> result)
        {
            result = new List<List<Card>>();
            foreach (var cardDeck in CardItems)
            {
                var cards = new List<Card>();
                foreach (var card in cardDeck)
                {
                    cards.Add(card.ToCard());
                }
                result.Add(cards);
            }            
        }
    }

    [ProtoContract]
    public class AskForMultipleChoiceResponse : GameResponse
    {
        [ProtoMember(1)]
        public int ChoiceIndex { get; set; }
        public AskForMultipleChoiceResponse() { }
        public AskForMultipleChoiceResponse(int choiceIndex) { this.ChoiceIndex = choiceIndex; }
    }
}
