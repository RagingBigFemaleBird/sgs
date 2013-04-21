using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Triggers
{
    /// <summary>
    /// The source of trigger definition.
    /// </summary>
    /// <remarks>
    /// 武将技能的说明＞游戏牌的说明＞游戏规则。
    /// </remarks>
    public enum TriggerType
    {
        /// <summary>
        /// 游戏规则
        /// </summary>
        GameRule = 1,
        /// <summary>
        /// 游戏牌的说明
        /// </summary>
        Card = 2,
        /// <summary>
        /// 武将技能的说明
        /// </summary>
        Skill = 3,
    }

    
    public class Trigger
    {
        public virtual Player Owner { get; set; }

        public Trigger()
        {
            type = TriggerType.GameRule;
            conflicting = false;
            enabled = true;
            priority = 0;
        }
        
        TriggerType type;
        /// <summary>
        /// The source of trigger definition. Helps decide whether which triggers
        /// will be ignored when in conflict.
        /// </summary>
        public TriggerType Type
        {
            get { return type; }
            set { type = value; }
        }

        bool conflicting;
        /// <summary>
        /// Whether triggers in the same category will conflict with each other.
        /// </summary>
        public bool Conflicting
        {
            get { return conflicting; }
            set { conflicting = value; }
        }

        bool enabled;
        /// <summary>
        /// Whether the trigger is active. A disabled trigger will never be called.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        int priority;
        /// <summary>
        /// Priority of trigger invokation. Greater value means higher priority. When
        /// there are multiple triggers of the same priority. They will be executed in
        /// the order of their registration.
        /// </summary>
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        /// <summary>
        /// Main loop of the trigger logic. Invoked when the event it listens on is emitted.
        /// </summary>
        /// <param name="gameEvent">Game event that invokes the trigger.</param>
        /// <param name="eventArgs">Parameters of the game event.</param>
        public virtual void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!enabled)
            {
                return;
            }
        }
    }
}
