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

        public virtual DamageElement ShaDamageElement
        {
            get { return DamageElement.None; }
        }

        public class ShanCardChoiceVerifier : ICardUsageVerifier
        {
            public VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players)
            {
                // todo: skill != null
                if (skill != null || cards == null || cards.Count != 1 || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Type != new Shan().CardType)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
        }

        protected override void Process(Player source, Player dest)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = source;
            arg.Targets = new List<Player>();
            arg.Targets.Add(dest);
            arg.StringArg = this.CardType;

            Game.CurrentGame.Emit(GameEvent.PlayerIsCardTarget, arg);
            
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

            while (numberOfShanRequired-- > 0 && !cannotUseShan)
            {
                foreach (var player in arg.Targets)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[player];
                    ShanCardChoiceVerifier v1 = new ShanCardChoiceVerifier();
                    ISkill s;
                    List<Player> p;
                    List<Card> cards;
                    if (!ui.AskForCardUsage("Shan", v1, out s, out cards, out p))
                    {
                        goto invalidAnswer;
                    }
                }
            }
            invalidAnswer:
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
            /* todo: 杀目标结算*/
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            Player player = targets[0];

            return VerifierResult.Success;
        }
    }
}
