using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Utils;
using System.Diagnostics;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 挟缠―限定技，出牌阶段，你可与对手拼点。若你赢，视为你对其使用一张【决斗】，若你没赢，视为其对你使用一张【决斗】。
    /// </summary>
    public class XieChan : ActiveSkill
    {
        public XieChan()
        {
            IsSingleUse = true;
        }

        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[XieChanUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count == 1)
            {
                if (arg.Targets[0].HandCards().Count == 0) return VerifierResult.Fail;
                if (arg.Targets[0] == Owner) return VerifierResult.Fail;
                CompositeCard c = new CompositeCard();
                c.Type = new JueDou();
                c.Subcards = null;
                List<Player> dests = new List<Player>();
                dests.Add(arg.Targets[0]);
                if (!Game.CurrentGame.PlayerCanBeTargeted(null, dests, c))
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanUseCard(Owner, c))
                {
                    return VerifierResult.Fail;
                }
            }
            if (Owner.HandCards().Count == 0) return VerifierResult.Fail;

            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[XieChanUsed] = 1;
            var win = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            GameEventArgs args = new GameEventArgs();
            if (win == true)
            {
                args.Source = Owner;
                args.Targets = new List<Player>();
                args.Targets.Add(arg.Targets[0]);
            }
            else
            {
                args.Source = arg.Targets[0];
                args.Targets = new List<Player>();
                args.Targets.Add(Owner);
            }
            args.Skill = new CardWrapper(arg.Source, new JueDou());
            args.Cards = new List<Card>();
            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            return true;
        }

        public static PlayerAttribute XieChanUsed = PlayerAttribute.Register("XieChanUsed");

    }
}
