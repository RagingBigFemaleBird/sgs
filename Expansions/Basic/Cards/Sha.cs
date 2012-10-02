using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class Sha : CardHandler
    {
        static Sha()
        {
            PlayerShaTargetValidation = new GameEvent("PlayerShaTargetValidation");
        }

        public virtual DamageElement ShaDamageElement
        {
            get { return DamageElement.None; }
        }

        protected override void Process(Player source, Player dest)
        {
            
            /* todo: 
                四、杀指定目标时的锁定技：青釭剑、无双、肉林
                五、杀指定目标时，杀使用者的武将技：铁骑、烈弓（仅检验目标角色是否可以闪避，并不立即造成伤害）
                六、杀指定目标时，杀使用者的武器技：雌雄双股剑
                七、强制不能结算闪的情况－－普通杀藤甲：终止；黑杀仁王盾：终止；在步骤五确定无法闪避：跳至步骤九
                八-１、目标使用闪：手牌闪、护驾、龙胆、倾国、八卦阵
                八-２、目标使用闪，结算闪之前的技能：雷击
                八-３、闪抵消了杀，杀使用者的武将技：猛进
                八-４、闪抵消了杀，杀使用者的武器技（无此步骤则终止）－－贯石斧：至步骤九；青龙刀：回步骤二            
             */
            int numberOfShanRequired = 1;
            bool cannotUseShan = false;

            while (numberOfShanRequired > 0 && !cannotUseShan)
            {
                IUiProxy ui = Game.CurrentGame.UiProxies[dest];
                SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Shan; });
                ISkill skill;
                List<Player> p;
                List<Card> cards;
                if (!ui.AskForCardUsage("Shan", v1, out skill, out cards, out p))
                {
                    break;
                }
                if (!HandleCardUseWithSkill(dest, skill, cards))
                {
                    continue;
                }
                numberOfShanRequired--;
            }
            if (numberOfShanRequired > 0)
            {
                Game.CurrentGame.DoDamage(source, dest, 1, ShaDamageElement, Game.CurrentGame.Decks[DeckType.Compute]);
            }
            else
            {
                Trace.TraceInformation("Successfully dodged");
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {

            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            ShaEventArgs args = new ShaEventArgs();
            args.RangeApproval = new List<bool>(targets.Count);
            args.TargetApproval = new List<bool>(targets.Count);
            int i = 0;
            foreach (Player t in targets)
            {
                if (Game.CurrentGame.DistanceTo(source, t) <= source[PlayerAttribute.RangeAttack])
                {
                    args.RangeApproval[i] = true;
                }
                else
                {
                    args.RangeApproval[i] = false;
                }
                args.TargetApproval[i] = false;
            }
            if (source[NumberOfShaUsed] == 0)
            {
                args.TargetApproval[0] = true;
            }

            try
            {
                Game.CurrentGame.Emit(PlayerShaTargetValidation, args);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }

            foreach (bool b in args.TargetApproval.Concat(args.RangeApproval))
            {
                if (!b)
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
        public static string NumberOfShaUsed = "NumberOfShaUsed";
        /// <summary>
        /// 玩家使用杀的目标检测
        /// </summary>
        public static readonly GameEvent PlayerShaTargetValidation;

    }


    public class ShaEventArgs : GameEventArgs
    {
        List<bool> rangeApproval;

        public List<bool> RangeApproval
        {
            get { return rangeApproval; }
            set { rangeApproval = value; }
        }

        List<bool> targetApproval;

        public List<bool> TargetApproval
        {
            get { return targetApproval; }
            set { targetApproval = value; }
        }
    }
}
