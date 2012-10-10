using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 刚烈-每当你受到一次伤害后，你可以进行一次判定，若结果不为红桃，则伤害来源选择一项：弃置两张手牌，或受到你对其造成的1点伤害。
    /// </summary>
    public class GangLie : PassiveSkill
    {
        class GangLieTrigger : Trigger
        {
            public Player Owner { get; set; }
            public class GangLieVerifier : ICardChoiceVerifier
            {

                public VerifierResult Verify(List<List<Card>> answer)
                {
                    Trace.Assert(answer.Count == 1);
                    if (answer[0].Count < 2)
                    {
                        return VerifierResult.Partial;
                    }
                    return VerifierResult.Success;
                    
                }
            }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                Card c = Game.CurrentGame.Judge(Owner);
                if (c.Suit != SuitType.Heart)
                {
                    List<DeckPlace> deck = new List<DeckPlace>();
                    deck.Add(new DeckPlace(eventArgs.Source, DeckType.Hand));
                    List<int> max = new List<int>();
                    max.Add(2);
                    List<List<Card>> result;
                    List<string> deckname = new List<string>();
                    deckname.Add("GangLie choice");
                    GangLieVerifier ver = new GangLieVerifier();
                    if (!Game.CurrentGame.UiProxies[eventArgs.Source].AskForCardChoice("GangLie", deck, deckname, max, ver, out result))
                    {
                        Game.CurrentGame.DoDamage(Owner, eventArgs.Source, 1, DamageElement.None, null);
                    }
                }
            }
            public GangLieTrigger(Player p)
            {
                Owner = p;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageInflicted, new GangLieTrigger(owner));
        }
    }
}
