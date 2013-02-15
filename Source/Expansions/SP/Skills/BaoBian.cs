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
using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Wind.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 豹变-锁定技，若你的体力值为3或更少，你视为拥用技能“挑衅”；若你的体力值为2或更少，你视为拥有技能“咆哮”；若你的体力值为1，你视为拥有技能“神速”。
    /// </summary>
    public class BaoBian : AutoVerifiedActiveSkill
    {
        BaoBianTiaoXin bbTiaoXin;
        public BaoBian()
        {
            LinkedPassiveSkill = new BaoBianLinkedPassiveSkill();
            bbTiaoXin = new BaoBianTiaoXin();
            MaxCards = bbTiaoXin.MaxCards;
            MinCards = bbTiaoXin.MinCards;
            MaxPlayers = bbTiaoXin.MaxPlayers;
            MinPlayers = bbTiaoXin.MinPlayers;
            IsEnforced = true;
        }

        public override Player Owner
        {
            get { return base.Owner; }
            set
            {
                if (Owner == value) return;
                base.Owner = value;
                bbTiaoXin.Owner = value;
            }
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source.Health > 3) return false;
            return bbTiaoXin.AdditionalVerify(source, cards, players);
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return bbTiaoXin.VerifyCard(source, card);
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return bbTiaoXin.VerifyPlayer(source, player);
        }

        public override bool NotifyAndCommit(GameEventArgs arg)
        {
            return bbTiaoXin.NotifyAndCommit(arg);
        }

        public override bool Commit(GameEventArgs arg)
        {
            throw new NotImplementedException();
        }

        class BaoBianLinkedPassiveSkill : TriggerSkill
        {
            PaoXiao bbPaoXiao;
            ShenSu bbShenSu;
            void IntBaoBian(Player p)
            {
                if (p.Health <= 2) bbPaoXiao.Owner = p;
                else bbPaoXiao.Owner = null;
                if (p.Health == 1) bbShenSu.Owner = p;
                else bbShenSu.Owner = null;
            }
            public override Player Owner
            {
                get
                {
                    return base.Owner;
                }
                set
                {
                    if (Owner == value) return;
                    if (Owner != null) IntBaoBian(Owner);
                    base.Owner = value;
                    if (Owner != null) IntBaoBian(Owner);
                }
            }
            public BaoBianLinkedPassiveSkill()
            {
                bbPaoXiao = new PaoXiao();
                bbShenSu = new ShenSu();
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { if (e == GameEvent.AfterHealthChanged) return a.Targets[0] == p; return a.Source == p; },
                    (p, e, a) => { IntBaoBian(p); },
                    TriggerCondition.Global
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.AfterHealthChanged, trigger);
                Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            }
        }

        class BaoBianTiaoXin : TiaoXin
        {
            new public int MaxCards { get { return base.MaxCards; } }
            new public int MinCards { get { return base.MinCards; } }
            new public int MaxPlayers { get { return base.MaxPlayers; } }
            new public int MinPlayers { get { return base.MinPlayers; } }
            new public bool VerifyPlayer(Player source, Player player)
            {
                return base.VerifyPlayer(source, player);
            }

            new public bool VerifyCard(Player source, Card card)
            {
                return base.VerifyCard(source, card);
            }

            new public bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                return base.AdditionalVerify(source, cards, players);
            }

            public override bool NotifyAndCommit(GameEventArgs arg)
            {
                (new TiaoXin()).NotifyAction(arg.Source, arg.Targets, arg.Cards);
                return base.Commit(arg);
            }
        }
    }
}
