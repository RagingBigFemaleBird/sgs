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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 明策-出牌阶段，你可以交给一名其他角色一张装备牌或【杀】，该角色选择一项：
    /// 1. 视为对其攻击范围内你选择的另一名角色使用一张【杀】。
    /// 2. 摸一张牌。
    /// 每阶段限一次。
    /// </summary>
    public class MingCe : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[MingCeUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            foreach (Card card in cards)
            {
                if (!(card.Type is Sha || CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment)))
                    return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets.Count == 1 && arg.Targets[0] == Owner)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        class MingCeShaTargetVerifier : CardsAndTargetsVerifier
        {
            public MingCeShaTargetVerifier(Player t)
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
                Discarding = false;
                target = t;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != target && Game.CurrentGame.DistanceTo(target, player) <= target[Player.AttackRange] + 1;
            }
            Player target;
        }

        class MingCeShaComposerSkill : CardTransformSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
            {
                card = new CompositeCard();
                card.Type = new Sha();
                return VerifierResult.Success;
            }
            protected override void NotifyAction(Player source, List<Player> targets, CompositeCard card)
            {
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MingCeUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, arg.Targets[0], arg.Cards);
            List<Player> check = new List<Player>();
            foreach (Player p in Game.CurrentGame.AlivePlayers)
            {
                if (arg.Targets[0] != p && Game.CurrentGame.DistanceTo(arg.Targets[0], p) <= arg.Targets[0][Player.AttackRange] + 1)
                {
                    check.Add(p);
                    break;
                }
            }
            if (check.Count == 0)
            {
                Game.CurrentGame.DrawCards(arg.Targets[0], 1);
                return true;
            }
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!Owner.AskForCardUsage(new CardUsagePrompt("MingCe"), new MingCeShaTargetVerifier(arg.Targets[0]), out skill, out cards, out players))
            {
                players = new List<Player>(check);
            }
            int answer = 0;
            arg.Targets[0].AskForMultipleChoice(new MultipleChoicePrompt("MingCe"), new List<OptionPrompt>() { new OptionPrompt("MingCeSha", players[0]), new OptionPrompt("MingCeMoPai") }, out answer);
            if (answer == 0)
            {
                try
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = arg.Targets[0];
                    args.Targets = new List<Player>() { players[0]};
                    args.Skill = new MingCeShaComposerSkill();
                    args.Cards = cards;
                    Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.Retry);
                }
            }
            else
            {
                Game.CurrentGame.DrawCards(arg.Targets[0], 1);
            }
            return true;
        }

        private static PlayerAttribute MingCeUsed = PlayerAttribute.Register("MingCeUsed", true);
    }
}
