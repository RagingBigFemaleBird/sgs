using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Expansions.Wind.Skills;
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 落雁-锁定技，若你的武将牌上有牌，你视为拥有技能“天香”和“流离”。
    /// </summary>
    public class LuoYan : TriggerSkill
    {
        ISkill lyTianXiang;
        ISkill lyLiuLi;

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                if (Owner == value)
                {
                    return;
                }
                if (Owner != null && lyLiuLi != null && lyTianXiang != null)
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, lyLiuLi, true);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, lyTianXiang, true);
                    lyLiuLi = null;
                    lyTianXiang = null;
                }
                base.Owner = value;
            }
        }

        public LuoYan()
        {
            var acquiredSkills = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Cards.Any(c => c.Place.DeckType == XingWu.XingWuDeck) && lyLiuLi == null && lyTianXiang == null; },
                (p, e, a) =>
                {
                    lyLiuLi = new LiuLi();
                    lyTianXiang = new TianXiang();
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, lyLiuLi, HeroTag, true);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, lyTianXiang, HeroTag, true);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardsAcquired, acquiredSkills);

            var loseSkills = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Cards.Any(c => c.HistoryPlace1.DeckType == XingWu.XingWuDeck) && Game.CurrentGame.Decks[p, XingWu.XingWuDeck].Count == 0; },
                (p, e, a) => 
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, lyLiuLi, true);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, lyTianXiang, true);
                    lyLiuLi = null;
                    lyTianXiang = null;
                } ,
                TriggerCondition.OwnerIsSource
            );
            IsEnforced = true;
            Triggers.Add(GameEvent.CardsLost, loseSkills);
        }
    }
}
