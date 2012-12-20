using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Games;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.Triggers
{
    
    public class GameEvent
    {
        static GameEvent()
        {
            DoPlayer = new GameEvent("DoPlayer");
            Shuffle = new GameEvent("Shuffle");
            GameStart = new GameEvent("GameStart");

            PhaseBeginEvents = new Dictionary<TurnPhase, GameEvent>();
            PhaseProceedEvents = new Dictionary<TurnPhase, GameEvent>();
            PhaseEndEvents = new Dictionary<TurnPhase, GameEvent>();
            PhaseOutEvents = new Dictionary<TurnPhase, GameEvent>();

            foreach (TurnPhase phase in
                Enum.GetValues(typeof(TurnPhase)).Cast<TurnPhase>())
            {
                if (phase >= TurnPhase.BeforeStart &&
                    phase <= TurnPhase.PostEnd)
                {
                    PhaseBeginEvents.Add(phase, new GameEvent("PhaseBeginEvents" + (int)phase));
                    PhaseProceedEvents.Add(phase, new GameEvent("PhaseProceedEvents" + (int)phase));
                    PhaseEndEvents.Add(phase, new GameEvent("PhaseEndEvents" + (int)phase));
                    PhaseOutEvents.Add(phase, new GameEvent("PhaseOutEvents" + (int)phase));
                }
            }
            PlayerCanBeTargeted = new GameEvent("PlayerCanBeTargeted");
            CommitActionToTargets = new GameEvent("CommitActionToTargets");
            DamageSourceConfirmed = new GameEvent("DamageSourceConfirmed");
            DamageElementConfirmed = new GameEvent("DamageElementConfirmed");
            BeforeDamageComputing = new GameEvent("BeforeDamageComputing");
            DamageComputingStarted = new GameEvent("DamageComputingStarted");
            DamageCaused = new GameEvent("DamageCaused");
            DamageInflicted = new GameEvent("DamageInflicted");
            BeforeHealthChanged = new GameEvent("BeforeHealthChanged");
            AfterHealthChanged = new GameEvent("AfterHealthChanged");
            AfterDamageCaused = new GameEvent("AfterDamageCaused");
            AfterDamageInflicted = new GameEvent("AfterDamageInflicted");
            DamageComputingFinished = new GameEvent("DamageComputingFinished");
            CardUsageTargetConfirming = new GameEvent("PlayerIsCardTarget");
            PlayerCanUseCard = new GameEvent("PlayerCanUseCard");
            PlayerUsedCard = new GameEvent("PlayerUsedCard");
            PlayerPlayedCard = new GameEvent("PlayerPlayedCard");
            PlayerCanDiscardCard = new GameEvent("PlayerCanDiscardCard");
        }

        
        public class DuplicateGameEventException : SgsException { }

        public List<GameEvent> DefinedGameEvents
        {
            get { return definedGameEvents.Values.ToList(); }
        }

        public GameEvent GetEvent(string name)
        {
            return definedGameEvents[name];
        }

        public GameEvent(string name)
        {
            this.name = name;
            if (definedGameEvents.ContainsKey(name))
            {
                throw new DuplicateGameEventException();
            }
            definedGameEvents[name] = this;
        }

        string name;

        /// <summary>
        /// Unique name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        static Dictionary<string, GameEvent> definedGameEvents = new Dictionary<string, GameEvent>();

        public static readonly GameEvent DoPlayer;
        public static readonly GameEvent Shuffle;
        public static readonly GameEvent GameStart;


        // Events defined on dadao.net/sgs
        /// <summary>
        /// 游戏开始
        /// </summary>
        /// <remarks>
        /// 在游戏开始前亮出武将牌后发动的武将技能：【化身①】。
        /// 在游戏开始前分发起始手牌时发动的武将技能：【七星①】。
        /// 在游戏开始时发动的武将技能：【狂暴①】。
        /// </remarks>
        public static readonly GameEvent PlayerGameStartAction = new GameEvent("PlayerGameStartAction");

        /// <summary>
        /// Cleanup triggers only 例如双雄
        /// </summary>
        public static readonly GameEvent PhaseBeforeStart = new GameEvent("PhaseBeforeStart");
        public static readonly GameEvent PhasePostEnd = new GameEvent("PhasePostEnd");

        /// <summary>
        /// 回合开始前，XX阶段开始时。 
        /// </summary>
        /// <remarks>
        /// 回合开始前：该角色的回合即将开始。若角色的武将牌处于背面朝上的状态，则将之翻转至正面朝上，
        ///             然后跳过该角色的这一回合，直接进入下一名角色的回合开始前。
        /// 回合开始阶段开始时：【洛神】、【凿险】、【自立】、【秘计】、【观星】、【志继】、【若愚】、
        ///                     【神智】、【英魂】、【魂姿】、【拜印】。
        /// 判定阶段开始时：暂时没有作用。
        /// 摸牌阶段开始时：【突袭】、【再起】、【眩惑】、【父魂】、【双雄】、【涉猎】。
        /// 出牌阶段开始时：【双刃】。
        /// 弃牌阶段开始时：【庸肆②】。
        /// 回合结束阶段开始时：【据守】、【秘计】、【骁果】、【举荐】、【醇醪①】、【闭月】、【崩坏】、
        ///                     【狂风】、【大雾】。
        /// </remarks>
        public static readonly Dictionary<TurnPhase, GameEvent> PhaseBeginEvents;

        /// <summary>
        /// 回合开始时，XX阶段进行中，回合结束时。
        /// </summary>
        /// <remarks>
        /// 回合开始时：【当先】、【化身②】。
        /// 回合开始阶段：暂时没有作用。
        /// 判定阶段：角色须进行其判定区里的延时类锦囊牌的使用结算。
        /// 摸牌阶段：角色摸两张牌。 摸牌时能发动影响摸牌数量的武将技能：【裸衣】、【将驰】、【英姿】、
        ///           【好施】、【弘援】、【自守】、【庸肆①】、【绝境①】。
        /// 出牌阶段：进行游戏的主要阶段。出牌阶段的空闲时间点能发动的武将技能（一般的转化类技能列于下
        ///           文“角色声明使用的牌名”处）：【强袭】、【驱虎】、【急袭】、【排异】、【奇策】、
        ///           【仁德】、【挑衅】、【心战】、【制衡】、【苦肉】、【反间】、【结姻】、【天义】、
        ///           【缔盟】、【制霸】、【直谏】、【甘露】、【安恤】、【奋迅】、【青囊】、【离间】、
        ///           【黄天】、【乱武】、【明策】、【陷阵】、【祸水①】、【倾城】、【雄异】、【攻心】、
        ///           【业炎】、【无前】、【神愤】、【极略（制衡）】、【极略（完杀）】。
        /// 弃牌阶段：检查角色的手牌数是否超出角色的手牌上限，若超出须弃置一张手牌。如此反复，
        ///           直到检查其手牌数等于其手牌上限为止。
        /// 回合结束阶段：暂时没有作用。
        /// 回合结束时：该角色的回合所有阶段均执行完毕，回合即将结束。另外，如果刘禅之前已经发动【放权】，
        ///             在此时机可以弃置一张手牌令一名其他角色进行一个额外的回合。
        /// </remarks>
        public static readonly Dictionary<TurnPhase, GameEvent> PhaseProceedEvents;

        /// <summary>
        /// 回合结束后，XX阶段结束时
        /// </summary>
        /// <remarks>
        /// 回合开始后：未定义
        /// 回合开始阶段结束时：暂时没有作用。
        /// 判定阶段结束时：暂时没有作用。
        /// 摸牌阶段结束时：【七星②】。
        /// 出牌阶段结束时：角色不想使用或无法使用牌时，便进入此时机，暂时没有作用。
        /// 弃牌阶段结束时：一旦进入此时机，表示角色的手牌数已不超过手牌上限，但也有可能会有武将技能使该
        ///                 角色获得手牌从而超过手牌上限。即便如此，该角色也无需再弃置手牌：【固政】。
        /// 回合结束阶段结束时：暂时没有作用。
        /// 回合结束后：【化身②】、【连破】。在此时机还应确认下一个行动的角色，【放权】技能发动，下一个
        ///              进行回合的角色会改为【放权】的目标。此时机过后，便是下一名角色的“回合开始前”。
        /// </remarks>
        public static readonly Dictionary<TurnPhase, GameEvent> PhaseEndEvents;
        
        /// <summary>
        /// Inter-phase events.
        /// </summary>
        /// <remarks>
        /// 回合开始阶段与判定阶段间：【神速①】、【巧变】。
        /// 判定阶段与摸牌阶段间：【巧变】。
        /// 摸牌阶段与出牌阶段间：【神速②】、【巧变】、【放权】。 
        /// 出牌阶段与弃牌阶段间：能发动的武将技能：【巧变】、【克己】。
        /// 弃牌阶段与回合结束阶段间：【伤逝】。
        /// </remarks>
        public static readonly Dictionary<TurnPhase, GameEvent> PhaseOutEvents;

        // Card movement related
        /// <summary>
        /// 置入弃牌堆时 
        /// </summary>
        /// <remarks>
        /// 能发动的技能：【落英】、【巨象②】、【礼让】。
        /// </remarks>
        public static readonly GameEvent CardsEnteringDiscardDeck = new GameEvent("CardsEnteringDiscardDeck");

        /// <summary>
        /// 置入弃牌堆后/进入新的区域后 
        /// </summary>
        /// <remarks>
        /// 能发动的技能依次为：
        /// a、武将技能：【屯田①】、【伤逝】、【枭姬】、【连营】、【旋风】、
        ///              【明哲】、【死谏】、【琴音】、【忍戒】。
        /// b、装备技能：【白银狮子②】。
        /// </remarks>
        public static readonly GameEvent CardsEnteredDiscardDeck = new GameEvent("CardsEnteredDiscardDeck");

        /// <summary>
        /// 角色获得牌时
        /// </summary>
        /// <remarks>
        /// 能发动的技能：【伤逝】、【恩怨①】
        /// </remarks>
        public static readonly GameEvent CardsAcquired = new GameEvent("CardsAcquired");

        /// <summary>
        /// 角色失去牌时
        /// </summary>
        public static readonly GameEvent CardsLost = new GameEvent("CardsLost");

        /// <summary>
        /// 角色获得武将技能时能
        /// </summary>
        /// <remarks>
        /// 能发动的技能：【伤逝】。
        /// </remarks>
        public static readonly GameEvent SkillAcquired;

        // Damage related
        /// <summary>
        /// 确定伤害来源
        /// </summary>
        public static readonly GameEvent DamageSourceConfirmed;

        /// <summary>
        /// 确定伤害基数和属性
        /// </summary>
        public static readonly GameEvent DamageElementConfirmed;

        /// <summary>
        /// 造成伤害前
        /// </summary>
        public static readonly GameEvent BeforeDamageComputing;

        /// <summary>
        /// 伤害结算开始时
        /// </summary>
        public static readonly GameEvent DamageComputingStarted;

        /// <summary>
        /// 造成伤害时
        /// </summary>
        public static readonly GameEvent DamageCaused;

        /// <summary>
        /// 受到伤害时
        /// </summary>
        public static readonly GameEvent DamageInflicted;

        /// <summary>
        /// 体力变化前
        /// </summary>
        public static readonly GameEvent BeforeHealthChanged;

        /// <summary>
        /// 体力变化后
        /// </summary>
        public static readonly GameEvent AfterHealthChanged;

        /// <summary>
        /// 造成伤害后
        /// </summary>
        public static readonly GameEvent AfterDamageCaused;

        /// <summary>
        /// 受到伤害后
        /// </summary>
        public static readonly GameEvent AfterDamageInflicted;

        /// <summary>
        /// 伤害结算完毕
        /// </summary>
        public static readonly GameEvent DamageComputingFinished;

        /// <summary>
        /// 玩家可以成为卡牌的目标
        /// </summary>
        public static readonly GameEvent PlayerCanBeTargeted;

        /// <summary>
        /// 卡牌效果生效
        /// </summary>
        public static readonly GameEvent CommitActionToTargets;

        /// <summary>
        /// 指定/成为目标时：目标有可能改变。
        /// 能发动的技能：【享乐】、【流离】、【天命】。
        /// </summary>
        public static readonly GameEvent CardUsageTargetConfirming = new GameEvent("CardUsageTargetConfirming");

        /// <summary>
        /// 指定/成为目标后：目标确定，不会再改变。
        /// 能发动的技能依次为： a、武将技能：【啖酪】、【铁骑】、【烈弓】、【祸首②】、【救援】、【激昂】、【无双】、【肉林】、【谋溃】。b、装备技能：【青釭剑】、【雌雄双股剑】。
        /// </summary>
        public static readonly GameEvent CardUsageTargetConfirmed = new GameEvent("CardUsageTargetConfirmed");

        /// <summary>
        /// 至此使用结算开始，首先须检测该牌对目标的有效性，会产生影响的技能：【仁王盾】、【藤甲①】、【毅重】、【啖酪】、【祸首①】、【巨象①】、【享乐】、
        /// 【智迟】、发动【陷阵】获得的技能、【无前】。如果该牌对目标无效，则中止对该目标的结算，使用结算完毕；如果该牌对目标有效，则继续对该目标进行结算。
        /// </summary>
        public static readonly GameEvent CardUsageTargetValidating = new GameEvent("CardUsageTargetValidating");

        /// <summary>
        /// 生效前：目标可以对该牌进行响应。
        /// 能进行响应的牌/技能依次为：（使用【杀】）首先可以发动武将技能【护驾】，其次可以发动装备技能【八卦阵】，如果上述方式未能成功使用【闪】，仍可以用下列方式使用一张【闪】：
        /// 使用手牌里的一张【闪】或发动【倾国】、【龙胆】、【蛊惑】、【龙魂】；（使用锦囊牌）使用一张【无懈可击】/【无懈可击·国】或发动【看破】、【蛊惑】、【龙魂】。
        /// 会产生影响的技能：（使用【杀】）【无双①】、【肉林】。
        /// ◆对于延时类锦囊牌来说，“生效前”这个时机即翻开判定牌前。
        /// ◆该牌的效果被抵消则该牌的使用结算完毕。其中【杀】的效果被抵消时能发动的技能依次为：a、武将技能：【猛进】。b、装备技能：【贯石斧】、【青龙偃月刀】。另外，如果穆顺之前已经发动【谋溃】，在此时机须执行“该角色弃置你的一张牌”的效果。
        /// </summary>
        public static readonly GameEvent CardUsageBeforeEffected = new GameEvent("CardUsageBeforeEffected");

        /// <summary>
        /// 玩家可以使用卡牌
        /// </summary>
        public static readonly GameEvent PlayerCanUseCard;

        /// <summary>
        /// 玩家可以打出卡牌
        /// </summary>
        public static readonly GameEvent PlayerCanPlayCard = new GameEvent("PlayerCanPlayCard");

        /// <summary>
        /// 玩家可以弃掉卡牌
        /// </summary>
        public static readonly GameEvent PlayerCanDiscardCard;

        /// <summary>
        /// 玩家使用牌
        /// </summary>
        public static readonly GameEvent PlayerUsedCard;

        /// <summary>
        /// 玩家打出牌
        /// </summary>
        public static readonly GameEvent PlayerPlayedCard;

        /// <summary>
        /// 玩家判定生效前
        /// </summary>
        public static readonly GameEvent PlayerJudgeBegin = new GameEvent("PlayerJudgeBegin");

        /// <summary>
        /// 玩家判定生效后
        /// </summary>
        public static readonly GameEvent PlayerJudgeDone = new GameEvent("PlayerJudgeDone");

        /// <summary>
        /// 玩家需要打出一张牌
        /// </summary>
        public static readonly GameEvent PlayerRequireCard = new GameEvent("PlayerRequireCard");

        /// <summary>
        /// 处于濒死
        /// </summary>
        public static readonly GameEvent PlayerDying = new GameEvent("PlayerDying");

        /// <summary>
        /// 进入濒死
        /// </summary>
        public static readonly GameEvent PlayerIsAboutToDie = new GameEvent("PlayerIsAboutToDie");

        /// <summary>
        /// 玩家死亡 此trigger只供游戏本身使用
        /// </summary>
        public static readonly GameEvent GameProcessPlayerIsDead = new GameEvent("PlayerIsDead");

        /// <summary>
        /// 玩家死亡时, 死亡时：能发动的技能：【行殇】、【挥泪】、【追忆】、【断肠】、【随势②】、【武魂②】。此外【连破】的发动条件是否满足是根据此时是否为一名角色的回合内来判断的。
        /// </summary>
        public static readonly GameEvent PlayerIsDead = new GameEvent("PlayerIsDying");

        /// <summary>
        /// 牌的使用距离加成
        /// </summary>
        public static readonly GameEvent CardRangeModifier = new GameEvent("CardRangeModifier");

        /// <summary>
        /// 锁定计：你的X视为Y
        /// </summary>
        public static readonly GameEvent EnforcedCardTransform = new GameEvent("EnforcedCardTransform");

        /// <summary>
        /// 玩家失去技能
        /// </summary>
        public static readonly GameEvent PlayerLoseSkill = new GameEvent("PlayerLoseSkill");

        /// <summary>
        /// 玩家改变国籍
        /// </summary>
        public static readonly GameEvent PlayerChangedAllegiance = new GameEvent("PlayerChangedAllegiance");

        /// <summary>
        /// 玩家手牌上限调整
        /// </summary>
        public static readonly GameEvent PlayerHandCardCapacityAdjustment = new GameEvent("PlayerHandCardCapacityAdjustment");


        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is GameEvent))
            {
                return false;
            }
            GameEvent event2 = (GameEvent)obj;
            return name.Equals(event2.name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
