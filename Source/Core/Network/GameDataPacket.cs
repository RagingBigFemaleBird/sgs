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
using Sanguosha.Core.UI;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Core.Network
{
    #region Basic Structures
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
            if (PlayerId >= Game.CurrentGame.Players.Count) return null;
            return Game.CurrentGame.Players[PlayerId];
        }

        public static PlayerItem Parse(int pid)
        {
            return new PlayerItem() { PlayerId = (byte)pid };
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

        public virtual ISkill ToSkill()
        {
            if (PlayerItem != null && PlayerItem.ToPlayer() != null)
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
    public class CardItem
    {
        [ProtoMember(1)]
        public DeckPlaceItem DeckPlaceItem { get; set; }

        [ProtoMember(2)]
        public int PlaceInDeck { get; set; }

        [ProtoMember(3)]
        public int CardId { get; set; }

        public Card ToCard(int wrtPlayerId)
        {            
            if (DeckPlaceItem == null) return null;
            var cardDeck = Game.CurrentGame.Decks[DeckPlaceItem.ToDeckPlace()];
            if (cardDeck == null || cardDeck.Count <= PlaceInDeck) return null;
            if (DeckPlaceItem.ToDeckPlace().Player != null && DeckPlaceItem.ToDeckPlace().DeckType == DeckType.Hand &&
                wrtPlayerId >= 0 && wrtPlayerId < Game.CurrentGame.Players.Count &&
                Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrtPlayerId]].Contains(DeckPlaceItem.ToDeckPlace().Player))
            {
                var theCard = cardDeck.FirstOrDefault(cd => cd.Id == CardId);
                return theCard;
            }

            if (DeckPlaceItem.ToDeckPlace().DeckType != DeckType.Equipment && DeckPlaceItem.ToDeckPlace().DeckType != DeckType.DelayedTools && CardId >= 0 && Game.CurrentGame.IsClient)
            {
                cardDeck[PlaceInDeck].Id = CardId;
                cardDeck[PlaceInDeck].Type = (CardHandler)(GameEngine.CardSet[CardId].Type.Clone());
                cardDeck[PlaceInDeck].Suit = GameEngine.CardSet[CardId].Suit;
                cardDeck[PlaceInDeck].Rank = GameEngine.CardSet[CardId].Rank;
                var bp = DeckPlaceItem.ToDeckPlace().Player;
                if (bp != null)
                {
                    Game.CurrentGame._FilterCard(bp, cardDeck[PlaceInDeck]);
                }
            }
            return cardDeck[PlaceInDeck];
        }
        
        public static CardItem Parse(Card card, int wrtPlayerId)
        {
            if (card == null) return null;
            var item = new CardItem()
            {
                DeckPlaceItem = DeckPlaceItem.Parse(card.Place),
                PlaceInDeck = Game.CurrentGame.Decks[card.Place].IndexOf(card)
            };
            if (card.Place.Player != null && card.Place.DeckType == DeckType.Hand && 
                wrtPlayerId >= 0 && wrtPlayerId < Game.CurrentGame.Players.Count &&
                Game.CurrentGame.HandCardVisibility[Game.CurrentGame.Players[wrtPlayerId]].Contains(card.Place.Player))
            {
                item.CardId = card.Id;
                return item;
            }
            else
            {
                if (card.RevealOnce)
                {
                    item.CardId = card.Id;
                    card.RevealOnce = false;
                }
                else { item.CardId = -1; }
                return item;
            }
        }
    }

    [ProtoContract]
    public class NestedCardList
    {
        [ProtoMember(1)]
        public List<int> ListSizes { get; set; }
        [ProtoMember(2)]
        public List<CardItem> AllCardItems { get; set; }
        
        public NestedCardList()
        {
        }

        public List<List<CardItem>> ToCardLists()
        {
            if (ListSizes == null || AllCardItems == null) return null;
            int i = 0;
            var list = new List<List<CardItem>>();
            foreach (var cardSize in ListSizes)
            {
                List<CardItem> cards = new List<CardItem>();
                for (int j = 0; j < cardSize; j++)
                {
                    cards.Add(AllCardItems[i++]);
                }
                list.Add(cards);
            }
            return list;
        }

        public static NestedCardList Parse(List<List<CardItem>> cardList)
        {
            if (cardList == null) return null;
            NestedCardList ncl = new NestedCardList();
            ncl.ListSizes = new List<int>();
            ncl.AllCardItems = new List<CardItem>();
            foreach (var cards in cardList)
            {
                ncl.ListSizes.Add(cards.Count);
                ncl.AllCardItems.AddRange(cards);
            }
            return ncl;
        }
    }

    #endregion
    #region GameDataPacket

    [ProtoContract]
    [ProtoInclude(1011, typeof(GameResponse))]
    [ProtoInclude(1012, typeof(CardRearrangementNotification))]
    [ProtoInclude(1013, typeof(HandCardMovementNotification))]
    [ProtoInclude(1014, typeof(GameUpdate))]
    public class GameDataPacket
    {
    }

    [ProtoContract]
    [ProtoInclude(1221, typeof(ConnectionRequest))]
    [ProtoInclude(1222, typeof(ConnectionResponse))]
    [ProtoInclude(1223, typeof(StatusSync))]
    [ProtoInclude(1224, typeof(CardSync))]
    [ProtoInclude(1225, typeof(UIStatusHint))]
    [ProtoInclude(1226, typeof(MultiCardUsageResponded))]
    [ProtoInclude(1227, typeof(SeedSync))]
    public class GameUpdate : GameDataPacket
    {
    }

    #region GameResponse
    [ProtoContract]
    [ProtoInclude(1111, typeof(AskForCardUsageResponse))]
    [ProtoInclude(1112, typeof(AskForCardChoiceResponse))]
    [ProtoInclude(1113, typeof(AskForMultipleChoiceResponse))]
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

        public static AskForCardUsageResponse Parse(int id, ISkill skill, List<Card> cards, List<Player> players, int wrtPlayerId)
        {            
            AskForCardUsageResponse response = new AskForCardUsageResponse();
            response.Id = id;
            response.SkillItem = SkillItem.Parse(skill);
            if (cards == null) response.CardItems = null;
            else
            {
                response.CardItems = new List<CardItem>();
                foreach (var card in cards)
                {
                    response.CardItems.Add(CardItem.Parse(card, wrtPlayerId));
                }
            }
            if (players == null) response.PlayerItems = null;
            else
            {
                response.PlayerItems = new List<PlayerItem>();
                foreach (var player in players)
                {
                    response.PlayerItems.Add(PlayerItem.Parse(player));
                }
            }
            return response;
        }

        public void ToAnswer(out ISkill skill, out List<Card> cards, out List<Player> players, int wrtPlayerId)
        {
            skill = null;
            if (SkillItem != null)
            {
                skill = SkillItem.ToSkill();
            }
            cards = new List<Card>();
            if (CardItems != null)
            {
                foreach (var card in CardItems)
                {
                    cards.Add(card.ToCard(wrtPlayerId));
                }
            }
            players = new List<Player>();
            if (PlayerItems != null)
            {
                players = new List<Player>();
                foreach (var player in PlayerItems)
                {
                    players.Add(player.ToPlayer());
                }
            }
        }
    }

    [ProtoContract]
    public class AskForCardChoiceResponse : GameResponse
    {
        [ProtoMember(1)]
        NestedCardList CardItems { get; set; }
        [ProtoMember(2)]
        int OptionId { get; set; }

        public static AskForCardChoiceResponse Parse(int id, List<List<Card>> cards, int optionId, int wrtPlayerId)
        {
            AskForCardChoiceResponse response = new AskForCardChoiceResponse();
            response.Id = id;
            if (cards == null) response.CardItems = null;
            else
            {
                var cardItems = new List<List<CardItem>>();
                foreach (var cardDeck in cards)
                {
                    Trace.Assert(cardDeck != null);
                    if (cardDeck == null) continue;
                    var items = new List<CardItem>();
                    foreach (var card in cardDeck)
                    {
                        items.Add(CardItem.Parse(card, wrtPlayerId));
                    }
                    cardItems.Add(items);
                }
                response.CardItems = NestedCardList.Parse(cardItems);
            }
            response.OptionId = optionId;
            return response;
        }

        public List<List<Card>> ToAnswer(int wrtPlayerId, out int option)
        {
            option = 0;
            if (CardItems == null) return null;
            var cardItemList = CardItems.ToCardLists();
            var result = new List<List<Card>>();
            if (cardItemList != null)
            {
                foreach (var cardDeck in cardItemList)
                {
                    // Invalid packet.
                    if (cardDeck == null) return null;

                    var cards = new List<Card>();
                    foreach (var card in cardDeck)
                    {
                        cards.Add(card.ToCard(wrtPlayerId));
                    }
                    result.Add(cards);
                }
            }
            option = OptionId;
            return result;
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
    #endregion
    #region Notifications
    [ProtoContract]
    public class HandCardMovementNotification : GameDataPacket
    {
        [ProtoMember(1)]
        public PlayerItem PlayerItem { get; set; }
        [ProtoMember(2)]
        public int From { get; set; }
        [ProtoMember(3)]
        public int To { get; set; }
    }

    [ProtoContract]
    public class ConnectionRequest : GameUpdate
    {
        [ProtoMember(1)]
        public LoginToken token { get; set; }
    }

    [ProtoContract]
    public class ConnectionResponse : GameUpdate
    {
        [ProtoMember(1)]
        public GameSettings Settings { get; set; }
        [ProtoMember(2)]
        public int SelfId { get; set; }
    }

    [ProtoContract]
    public class UIStatusHint : GameUpdate
    {
        [ProtoMember(1)]
        public bool IsDetached { get; set; }
    }

    [ProtoContract]
    public class MultiCardUsageResponded : GameUpdate
    {
        [ProtoMember(1)]
        public PlayerItem PlayerItem { get; set; }
    }

    [ProtoContract]
    public class StatusSync : GameUpdate
    {
        [ProtoMember(1)]
        public int Status { get; set; }
    }

    [ProtoContract]
    public class CardSync : GameUpdate
    {
        [ProtoMember(1)]
        public CardItem Item { get; set; }
    }

    [ProtoContract]
    public class SeedSync : GameUpdate
    {
        public SeedSync(int seed)
        {
            Seed = Misc.MagicAnimal.ToString("X8") + seed.ToString("X8");
        }
        public SeedSync()
        {
        }
        [ProtoMember(1)]
        public String Seed { get; set; }
    }

    [ProtoContract]
    public class CardRearrangementNotification : GameDataPacket
    {
        [ProtoMember(1)]
        public CardRearrangement CardRearrangement { get; set; }
    }
    #endregion
    #endregion
}
