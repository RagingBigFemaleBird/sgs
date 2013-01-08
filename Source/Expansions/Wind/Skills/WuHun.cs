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
    /// 武魂-锁定技，每当你造成1点伤害后，伤害来源获得一枚"梦魇"标记，你死亡时，令拥有最多该标记的一名角色判定，若结果不为【桃】或者【桃园结义】，则该角色死亡。
    /// </summary>
    public class WuHun : TriggerSkill
    {

        class WuHunVerifier : CardsAndTargetsVerifier
        {
            private List<Player> maxMarkPlayers;

            public WuHunVerifier(List<Player> maxMarkPlayers)
            {
                this.maxMarkPlayers = maxMarkPlayers;
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return maxMarkPlayers.Contains(player);
            }
        }

        public void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var toProcess = Game.CurrentGame.AlivePlayers;
            int maxMark = 0;
            List<Player> maxMarkPlayers = new List<Player>();
            foreach (var p in toProcess)
            {
                if (p[MengYanMark] > 0 && p[MengYanMark] > maxMark)
                {
                    maxMark = p[MengYanMark];
                    maxMarkPlayers.Clear();
                    maxMarkPlayers.Add(p);
                }
                else if (p[MengYanMark] > 0 && p[MengYanMark] == maxMark)
                {
                    maxMarkPlayers.Add(p);
                }
            }
            if (maxMark > 0)
            {

                if (maxMarkPlayers.Count > 1)
                {
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (!Game.CurrentGame.UiProxies[owner].AskForCardUsage(new CardUsagePrompt("WuHun"), new WuHunVerifier(maxMarkPlayers), out skill, out cards, out players))
                    {
                        players = new List<Player>() { maxMarkPlayers[0] };
                    }
                    maxMarkPlayers = new List<Player>() { players[0] };
                }
                Player target = maxMarkPlayers[0];
                var result = Game.CurrentGame.Judge(target, this, null, (judgeResultCard) => { return !(judgeResultCard.Type is Tao || judgeResultCard.Type is TaoYuanJieYi); });
                if (!(result.Type is Tao || result.Type is TaoYuanJieYi))
                {
                    target.IsDead = true;
                    GameEventArgs args = new GameEventArgs();
                    args.Source = null;
                    args.Targets = new List<Player>() { target };
                    Game.CurrentGame.Emit(GameEvent.GameProcessPlayerIsDead, args);
                }
            }
            foreach (var p in Game.CurrentGame.Players)
            {
                p[MengYanMark] = 0;
            }
        }

        public WuHun()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                (p, e, a) => { a.Source[MengYanMark]++; },
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            Triggers.Add(GameEvent.PlayerIsDead, trigger2);
            IsEnforced = true;
        }

        public static PlayerAttribute MengYanMark = PlayerAttribute.Register("MengYan", false, true);
    }
}
