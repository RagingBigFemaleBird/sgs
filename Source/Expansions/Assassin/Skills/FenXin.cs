using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Utils;

namespace Sanguosha.Expansions.Assassin.Skills
{
    /// <summary>
    /// 焚心-限定技，若你的身份不是主公，当你杀死一名非主公角色时，在其亮出身份牌前，你可以与其交换身份牌。
    /// </summary>
    public class FenXin : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Owner[FenXinUsed] = 1;
            Player target = eventArgs.Targets[0];
            DeckType RoleDeck = new DeckType("Role");
            Card role1 = Game.CurrentGame.Decks[Owner, RoleDeck][0];
            Card role2 = Game.CurrentGame.Decks[target, RoleDeck][0];
            Game.CurrentGame.SyncCard(target, ref role1);
            Game.CurrentGame.SyncCard(Owner, ref role2);

            List<CardsMovement> moves = new List<CardsMovement>();
            CardsMovement move1 = new CardsMovement();
            move1.Cards = new List<Card>() { role1 };
            move1.To = new DeckPlace(target, RoleDeck);
            moves.Add(move1);

            CardsMovement move2 = new CardsMovement();
            move2.Cards = new List<Card>() { role2 };
            move2.To = new DeckPlace(Owner, RoleDeck);
            moves.Add(move2);

            Game.CurrentGame.MoveCards(moves);

            var role = role2.Type as RoleCardHandler;
            if (role != null)
            {
                Owner.Role = role.Role;
            }
            role = role1.Type as RoleCardHandler;
            if (role != null)
            {
                target.Role = role.Role;
            }
            GameDelays.Delay(GameDelayTypes.RoleDistribute);
        }

        public FenXin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[FenXinUsed] == 0 && p.Role != Role.Ruler && a.Targets[0].Role != Role.Ruler; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.BeforeRevealRole, trigger);
            IsSingleUse = true;
        }

        private static PlayerAttribute FenXinUsed = PlayerAttribute.Register("FenXinUsed");
    }
}
