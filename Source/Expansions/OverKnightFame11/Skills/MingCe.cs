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
    public class MingCe : AutoVerifiedActiveSkill
    {
        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Type.IsCardCategory(CardCategory.Equipment) || card.Type is Sha;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (Owner[MingCeUsed] == 1) return false;
            // you can choose only 1 player as target, iff this target cannot SHA anyone
            // i.e. you need to STOP returning Success (return Partial instead) if we have chosen card and one player and this player can SHA anyone
            if (cards.Count == 1 && players.Count == 1)
            {
                var pl = Game.CurrentGame.AlivePlayers;
                if (pl.Any(test => test != players[0] && Game.CurrentGame.DistanceTo(players[0], test) <= players[0][Player.AttackRange] + 1)) return null;
            }
            if (players.Count == 2)
            {
                if (Game.CurrentGame.DistanceTo(players[0], players[1]) > players[0][Player.AttackRange] + 1) return false;
            }
            return true;
        }

        public MingCe()
        {
            MinCards = 1;
            MaxCards = 1;
            MinPlayers = 1;
            MaxPlayers = 2;
        }

        class MingCeShaComposerSkill : CardTransformSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
            {
                card = new CompositeCard();
                card.Type = new RegularSha();
                return VerifierResult.Success;
            }
            protected override void NotifyAction(Player source, List<Player> targets, CompositeCard card)
            {
            }
        }

        // slightly modified from JieDaoShaRen
        public class MingCeShaVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Any(p => p.IsDead))
                {
                    return VerifierResult.Fail;
                }
                if (players == null)
                {
                    players = new List<Player>();
                }
                List<Player> newList = new List<Player>(players);
                if (!newList.Contains(target))
                {
                    newList.Add(target);
                }
                else
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 0)
                {
                    return VerifierResult.Fail;
                }
                return (new Sha()).Verify(source, new MingCeShaComposerSkill(), cards, newList);
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            Player target;

            public MingCeShaVerifier(Player t)
            {
                target = t;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MingCeUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, arg.Targets[0], arg.Cards);
            if (arg.Targets.Count == 1)
            {
                Game.CurrentGame.DrawCards(arg.Targets[0], 1);
                return true;
            }
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            int answer = 0;
            arg.Targets[0].AskForMultipleChoice(new MultipleChoicePrompt("MingCe"), new List<OptionPrompt>() { new OptionPrompt("MingCeSha", arg.Targets[1]), new OptionPrompt("MingCeMoPai") }, out answer);
            if (answer == 0)
            {
                arg.Targets[0].AskForCardUsage(new CardUsagePrompt("MingCe", arg.Targets[1]), new MingCeShaVerifier(arg.Targets[1]), out skill, out cards, out players);
                try
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = arg.Targets[0];
                    args.Targets = new List<Player>(players);
                    args.Targets.Add(arg.Targets[1]);
                    args.Skill = new MingCeShaComposerSkill();
                    args.Cards = cards;
                    Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                }
                catch (TriggerResultException)
                {
                    // This must NOT happen if you are not asking user to provide the card
                    Trace.Assert(false);
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
