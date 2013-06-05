using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 藏机-你死亡时，可将你装备区里所有的牌移出游戏；若如此做，你下一名角色登场时，将这些牌置于其装备区里。
    /// </summary>
    public class CangJi : TriggerSkill
    {
        public static DeckType JiJiDeck = DeckType.Register("JiJi");
        public CangJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.Equipments().Count > 0; },
                (p, e, a) =>
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>(p.Equipments());
                    move.To = new DeckPlace(p, JiJiDeck);
                    Game.CurrentGame.MoveCards(move);
                    Game.CurrentGame.RegisterTrigger(GameEvent.HeroDebut, new ZombieJiJiTrigger(p));
                },
                TriggerCondition.OwnerIsTarget
            );

            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = true;
        }

        private class ZombieJiJiTrigger : Trigger
        {
            Player player;
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Game.CurrentGame.Decks[player, JiJiDeck].Count > 0)
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>(Game.CurrentGame.Decks[player, JiJiDeck]);
                    move.To = new DeckPlace(player, DeckType.Equipment);
                    Game.CurrentGame.MoveCards(move);
                    Game.CurrentGame.UnregisterTrigger(GameEvent.HeroDebut, this);
                }
            }
            public ZombieJiJiTrigger(Player p)
            {
                player = p;
                Owner = null;
            }
        }
    }
}
