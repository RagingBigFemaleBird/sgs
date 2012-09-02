using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    public class GameEvent
    {
        public class EventType
        {
            public static EventType()
            {
                DoPlayer = new EventType();
                Shuffle = new EventType();

                PhaseBeginEvents = new Dictionary<Game.TurnPhase, EventType>();
                PhaseProceedEvents = new Dictionary<Game.TurnPhase, EventType>();
                PhaseEndEvents = new Dictionary<Game.TurnPhase, EventType>();
                PhaseOutEvents = new Dictionary<Game.TurnPhase, EventType>();

                foreach (Game.TurnPhase phase in
                    Enum.GetValues(typeof(Game.TurnPhase)).Cast<Game.TurnPhase>())
                {
                    if (phase >= Game.TurnPhase.BeforeTurnStart &&
                        phase <= Game.TurnPhase.AfterTurnFinish)
                    {
                        PhaseBeginEvents.Add(phase, new EventType());
                    }                    
                                        
                    if (phase >= Game.TurnPhase.TurnStart &&
                        phase <= Game.TurnPhase.TurnFinish)
                    {
                        PhaseProceedEvents.Add(phase, new EventType());
                    }

                    if (phase >= Game.TurnPhase.BeforeTurnStart &&
                        phase <= Game.TurnPhase.AfterTurnFinish)
                    {
                        PhaseEndEvents.Add(phase, new EventType());
                    }

                    if (phase >= Game.TurnPhase.TurnStart &&
                        phase < Game.TurnPhase.TurnFinish)
                    {
                        PhaseOutEvents.Add(phase, new EventType());
                    }
                }               
            }

            public static readonly EventType DoPlayer;
            public static readonly EventType Shuffle;

            // Events defined on dadao.net/sgs
            /// <summary>
            /// 游戏开始
            /// </summary>
            /// <remarks>
            /// 在游戏开始前亮出武将牌后发动的武将技能：【化身①】。
            /// 在游戏开始前分发起始手牌时发动的武将技能：【七星①】。
            /// 在游戏开始时发动的武将技能：【狂暴①】。
            /// </remarks>
            public static readonly EventType GameStart;

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
            protected static readonly Dictionary<Game.TurnPhase, EventType> PhaseBeginEvents;

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
            protected static readonly Dictionary<Game.TurnPhase, EventType> PhaseProceedEvents;

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
            protected static readonly Dictionary<Game.TurnPhase, EventType> PhaseEndEvents;

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
            protected static readonly Dictionary<Game.TurnPhase, EventType> PhaseOutEvents;

            // Card movement related
            /// <summary>
            /// 置入弃牌堆时 
            /// </summary>
            /// <remarks>
            /// 能发动的技能：【落英】、【巨象②】、【礼让】。
            /// </remarks>
            public static readonly GameEvent CardsEnteringDiscardDeck;

            /// <summary>
            /// 置入弃牌堆后/进入新的区域后 
            /// </summary>
            /// <remarks>
            /// 能发动的技能依次为：
            /// a、武将技能：【屯田①】、【伤逝】、【枭姬】、【连营】、【旋风】、
            ///              【明哲】、【死谏】、【琴音】、【忍戒】。
            /// b、装备技能：【白银狮子②】。
            /// </remarks>
            public static readonly GameEvent CardsEnteredDiscardDeck;

            /// <summary>
            /// 角色获得牌时
            /// </summary>
            /// <remarks>
            /// 能发动的技能：【伤逝】、【恩怨①】
            /// </remarks>
            public static readonly GameEvent CardsAcquired;

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
            public static readonly GameEvent DamageNatureConfirmed;

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
            /// 扣减体力前
            /// </summary>
            public static readonly GameEvent BeforeHealthReduced;
            
            /// <summary>
            /// 扣减体力后
            /// </summary>
            public static readonly GameEvent AfterHealthReduced;
            
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
        }

        protected EventType eventName;

        public EventType EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GameEvent))
            {
                return false;
            }
            GameEvent event2 = (GameEvent)obj;
            return eventName == event2.eventName;
        }
    }
}
