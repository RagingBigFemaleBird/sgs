using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class Jiu : LifeSaver
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            if (Game.CurrentGame.DyingPlayers.Count > 0)
            {
                Game.CurrentGame.RecoverHealth(source, dest, 1);
            }
            else
            {
                dest[JiuUsed] = 1;
                dest[Drank] = 1;
            }
        }

        public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
        {
            Trace.Assert(targets != null);
            if (targets == null) return VerifierResult.Fail;
            
            if (Game.CurrentGame.DyingPlayers.Count == 0)
            {
                if ((!isLooseVerify && targets.Count >= 1) || source[JiuUsed] == 1)
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if ((!isLooseVerify && targets.Count >= 1) || Game.CurrentGame.DyingPlayers.First() != source)
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }

        public override List<Player> ActualTargets(Player source, List<Player> targets, ICard card)
        {
            if (targets.Count > 0)
            {
                return new List<Player>(targets);
            }

            return new List<Player>() { source };
        }
        public static PlayerAttribute JiuUsed = PlayerAttribute.Register("JiuUsed", true);
        public static PlayerAttribute Drank = PlayerAttribute.Register("Drank", true);
        public static CardAttribute JiuSha = CardAttribute.Register("JiuSha");
    }

    public class JiuDamage : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DamageEventArgs;
            if (args.Source != null && args.ReadonlyCard != null && args.ReadonlyCard[Jiu.JiuSha] == 1)
            {
                args.Magnitude++;
            }
        }
    }

    public class JiuSha : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.ReadonlyCard != null && eventArgs.ReadonlyCard.Type is Sha && eventArgs.Source[Jiu.Drank] == 1)
            {
                eventArgs.ReadonlyCard[Jiu.JiuSha] = 1;
                eventArgs.Source[Jiu.Drank] = 0;
            }
        }
    }

}
