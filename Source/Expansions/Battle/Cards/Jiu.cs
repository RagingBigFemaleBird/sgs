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
            if (Game.CurrentGame.IsDying.Count > 0)
            {
                Game.CurrentGame.RecoverHealth(source, dest, 1);
            }
            else
            {
                source[JiuUsed] = 1;
                source[Drank] = 1;
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (Game.CurrentGame.IsDying.Count == 0 && targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.IsDying.Count > 0 && (targets == null || targets.Count != 1))
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.IsDying.Count == 0)
            {
                if (source[JiuUsed] == 1)
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if (targets[0] != source)
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
            return new List<Player>() {source};
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
                args.ReadonlyCard[Jiu.JiuSha] = 0;
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
