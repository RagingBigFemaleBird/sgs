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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Assassin.Skills
{
    /// <summary>
    /// 密诏-出牌阶段，你可以将所有手牌交给一名其他角色，令该角色与你选择的另一名角色拼点，视为点数大的角色对点数小的角色使用一张【杀】（无距离限制）。每阶段限一次。
    /// </summary>
    public class MiZhao : AutoVerifiedActiveSkill
    {
        class MiZhaoVerifier : CardsAndTargetsVerifier
        {
            Player target;
            public MiZhaoVerifier(Player target)
            {
                this.target = target;
                MaxCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != target && player.HandCards().Count > 0;
            }
        }

        public MiZhao()
        {
            MaxPlayers = 1;
            MinPlayers = 1;
            MaxCards = 0;
            Helper.NoCardReveal = true;
            LinkedPassiveSkill = new MiZhaoPassiveSkill();
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[MiZhaoUsed] != 0 || source.HandCards().Count == 0) return false;
            return true;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MiZhaoUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, arg.Targets[0], Owner.HandCards());
            List<Player> alivePlayers = Game.CurrentGame.AlivePlayers;
            if (!alivePlayers.Any(p => p.HandCards().Count > 0 && p != arg.Targets[0])) return true;
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!Owner.AskForCardUsage(new CardUsagePrompt("MiZhao", arg.Targets[0]), new MiZhaoVerifier(arg.Targets[0]), out skill, out cards, out players))
            {
                players = new List<Player>();
                foreach (Player p in alivePlayers)
                {
                    if (p.HandCards().Count > 0 && p != arg.Targets[0])
                    {
                        players.Add(p);
                        break;
                    }
                }
            }
            Player pindianTarget = players[0];
            var result = Game.CurrentGame.PinDian(arg.Targets[0], pindianTarget, this);
            Player winner, loser;
            if (result == null) return true;
            if (result == true) { winner = arg.Targets[0]; loser = pindianTarget; }
            else { winner = pindianTarget; loser = arg.Targets[0]; }
            if (!Game.CurrentGame.PlayerCanBeTargeted(winner, new List<Player>() { loser }, new CompositeCard() { Type = new Sha() })) return true;
            winner[Sha.NumberOfShaUsed]--;
            Sha.UseDummyShaTo(winner, loser, new RegularSha(), new CardUsagePrompt("MingCe", loser), MiZhaoSha);
            return true;
        }

        private static PlayerAttribute MiZhaoUsed = PlayerAttribute.Register("MiZhaoUsed", true);
        public static CardAttribute MiZhaoSha = CardAttribute.Register("MiZhaoSha");

        class MiZhaoPassiveSkill : TriggerSkill
        {
            public MiZhaoPassiveSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card[MiZhaoSha] != 0; },
                    (p, e, a) =>
                    {
                        ShaEventArgs args = a as ShaEventArgs;
                        args.RangeApproval[0] = true;
                    },
                    TriggerCondition.Global
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(Sha.PlayerShaTargetValidation, trigger);
            }
        }
    }
}
