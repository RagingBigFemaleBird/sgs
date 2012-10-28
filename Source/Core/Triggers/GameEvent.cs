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
                }

                if (phase >= TurnPhase.Start &&
                    phase <= TurnPhase.End)
                {
                    PhaseProceedEvents.Add(phase, new GameEvent("PhaseProceedEvents" + (int)phase));
                }

                if (phase >= TurnPhase.BeforeStart &&
                    phase <= TurnPhase.PostEnd)
                {
                    PhaseEndEvents.Add(phase, new GameEvent("PhaseEndEvents" + (int)phase));
                }

                if (phase >= TurnPhase.Start &&
                    phase < TurnPhase.End)
                {
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
            PlayerIsCardTarget = new GameEvent("PlayerIsCardTarget");
            PlayerCanUseCard = new GameEvent("PlayerCanUseCard");
            PlayerUsedCard = new GameEvent("PlayerUsedCard");
            PlayerPlayedCard = new GameEvent("PlayerPlayedCard");
            PlayerCanDiscardCard = new GameEvent("PlayerCanDiscardCard");
        }

        [Serializable]
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

        // Events defined on dadao.net/sgs
        /// <summary>
        /// 游戏开始
        /// </summary>
        /// <remarks>
        /// 在游戏开始前亮出武将牌后发动的武将技能：【化身①】。
        /// 在游戏开始前分发起始手牌时发动的武将技能：【七星①】。
        /// 在游戏开始时发动的武将技能：【狂暴①】。
        /// </remarks>
        public static readonly GameEvent GameStart;

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
        /// 玩家成为卡牌的目标
        /// </summary>
        public static readonly GameEvent PlayerIsCardTarget;

        /// <summary>
        /// 玩家可以使用卡牌
        /// </summary>
        public static readonly GameEvent PlayerCanUseCard;

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
        /// 处于频死
        /// </summary>
        public static readonly GameEvent PlayerDying = new GameEvent("PlayerDying");

        /// <summary>
        /// 进入频死
        /// </summary>
        public static readonly GameEvent PlayerIsAboutToDie = new GameEvent("PlayerIsAboutToDie");

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
            return name == event2.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
