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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 攻心-出牌阶段，你可以观看一名其他角色的手牌，并可以展示其中一张红桃牌，然后将其弃置或置于牌堆顶。每阶段限一次。
    /// </summary>
    public class GongXin : ActiveSkill
    {
        private static PlayerAttribute GongXinUsed = PlayerAttribute.Register("GongXinUsed", true);
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[GongXinUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[GongXinUsed] = 1;
            Player target = arg.Targets[0];
            List<List<Card>> answer;
            int windowId = 0;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("GongXin"),
                    new List<DeckPlace>() { new DeckPlace(target, DeckType.Hand) },
                    new List<string>() { "PaiDuiDing", "QiPaiDui" },
                    new List<int>() { 1, 1 },
                    new GongXinVerifier(),
                    out answer,
                    new List<bool>() { false, false }, ref windowId))
            {
                if (answer[0] != null && answer[0].Count > 0)
                {
                    Game.CurrentGame.InsertBeforeDeal(target, answer[0]);
                }
                else if (answer[1] != null && answer[1].Count > 0)
                {
                    Game.CurrentGame.PlaceIntoDiscard(target, answer[1]);
                }
            }
            return true;
        }
        class GongXinVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if (answer != null)
                {
                    bool c1 = false, c2 = false;
                    if (answer.Count > 0 && answer[0] != null && answer[0].Count > 0)
                    {
                        c1 = true;
                    }
                    if (answer.Count > 1 && answer[1] != null && answer[1].Count > 0)
                    {
                        c2 = true;
                    }
                    if (c1 && c2)
                    {
                        return VerifierResult.Fail;
                    }
                }
                return VerifierResult.Success;
            }

        }

    }
}
