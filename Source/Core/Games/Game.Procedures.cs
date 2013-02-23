using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Utils;
using Sanguosha.Core.Heroes;


namespace Sanguosha.Core.Games
{
    public abstract partial class Game
    {
        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="source">伤害来源</param>
        /// <param name="dest">伤害目标</param>
        /// <param name="originalTarget">最初的伤害目标</param>
        /// <param name="magnitude">伤害点数</param>
        /// <param name="elemental">伤害属性</param>
        /// <param name="cards">造成伤害的牌</param>
        public void DoDamage(Player source, Player dest, Player originalTarget, int magnitude, DamageElement elemental, ICard card, ReadOnlyCard readonlyCard)
        {
            if (dest.IsDead) return;
            var damageArgs = new DamageEventArgs() { Source = source, OriginalTarget = originalTarget, Targets = new List<Player>(), Magnitude = magnitude, Element = elemental };
            HealthChangedEventArgs healthChangedArgs;
            int ironShackledDamage = 0;
            DamageElement ironShackledDamageElement = DamageElement.None;
            if (readonlyCard == null)
            {
                readonlyCard = new ReadOnlyCard(new Card() { Place = new DeckPlace(null, null) });
            }
            damageArgs.ReadonlyCard = readonlyCard;
            if (card is CompositeCard)
            {
                if ((card as CompositeCard).Subcards != null)
                {
                    damageArgs.Cards = new List<Card>((card as CompositeCard).Subcards);
                }
            }
            else if (card is Card)
            {
                damageArgs.Cards = new List<Card>() { card as Card };
            }
            else
            {
                damageArgs.Cards = new List<Card>();
            }
            damageArgs.Targets.Add(dest);
            damageArgs.Card = card;

            try
            {
                //伤害来源与基数、属性的确定发生在伤害结算前，连环，以及转移的伤害不会重新确定来源与基数，所以不会多次触发【裸衣】，以及【酒】
                while (damageArgs.ReadonlyCard[SourceAndElementIsConfirmed] == 0)
                {
                    Emit(GameEvent.DamageSourceConfirmed, damageArgs);
                    Emit(GameEvent.DamageElementConfirmed, damageArgs);
                    damageArgs.ReadonlyCard[SourceAndElementIsConfirmed] = 1;
                    break;
                }
                Emit(GameEvent.BeforeDamageComputing, damageArgs);
                Emit(GameEvent.DamageComputingStarted, damageArgs);
                Emit(GameEvent.DamageCaused, damageArgs);
                Emit(GameEvent.DamageInflicted, damageArgs);
                if (damageArgs.Magnitude == 0)
                {
                    Trace.TraceInformation("Damage is 0, aborting");
                    return;
                }
                if (damageArgs.Targets[0].IsIronShackled && damageArgs.Element != DamageElement.None)
                {
                    ironShackledDamage = damageArgs.Magnitude;
                    Trace.TraceInformation("IronShackled damage {0}", ironShackledDamage);
                    ironShackledDamageElement = damageArgs.Element;
                    damageArgs.Targets[0].IsIronShackled = false;
                    // if this is TieSuo damage already, prevent itself from spreading...
                    if (readonlyCard[IsIronShackleDamage] == 1) ironShackledDamage = 0;
                }
                healthChangedArgs = new HealthChangedEventArgs(damageArgs);
                Emit(GameEvent.BeforeHealthChanged, healthChangedArgs);
                damageArgs.Magnitude = -healthChangedArgs.Delta;
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.End)
                {
                    //伤害结算完毕事件应该总是被触发
                    //受到伤害的角色如果存活能发动的技能/会执行的技能效果：【酒诗②】、执行【天香】摸牌的效果。
                    Emit(GameEvent.DamageComputingFinished, damageArgs);
                    Trace.TraceInformation("Damage Aborted");
                    return;
                }
                Trace.Assert(false);
                return;
            }

            Trace.Assert(damageArgs.Targets.Count == 1);
            damageArgs.Targets[0].Health -= damageArgs.Magnitude;
            Trace.TraceInformation("Player {0} Lose {1} hp, @ {2} hp", damageArgs.Targets[0].Id, damageArgs.Magnitude, damageArgs.Targets[0].Health);
            NotificationProxy.NotifyDamage(source, damageArgs.Targets[0], damageArgs.Magnitude, damageArgs.Element);
            GameDelays.Delay(GameDelayTypes.Damage);

            try
            {
                Emit(GameEvent.AfterHealthChanged, healthChangedArgs);
            }
            catch (TriggerResultException)
            {
            }
            Emit(GameEvent.AfterDamageCaused, damageArgs);
            Emit(GameEvent.AfterDamageInflicted, damageArgs);
            Emit(GameEvent.DamageComputingFinished, damageArgs);
            if (ironShackledDamage != 0)
            {
                List<Player> toProcess = new List<Player>(AlivePlayers);
                SortByOrderOfComputation(CurrentPlayer, toProcess);
                foreach (Player p in toProcess)
                {
                    if (p.IsIronShackled)
                    {
                        readonlyCard[IsIronShackleDamage] = 1;
                        DoDamage(damageArgs.Source, p, originalTarget, ironShackledDamage, ironShackledDamageElement, card, readonlyCard);
                    }
                }
            }
        }

        private static CardAttribute IsIronShackleDamage = CardAttribute.Register("IsIronShackleDamage");
        private static CardAttribute SourceAndElementIsConfirmed = CardAttribute.Register("SourceAndElementIsConfirmed");

        public void DoDamage(Player source, Player dest, int magnitude, DamageElement elemental, ICard card, ReadOnlyCard readonlyCard)
        {
            DoDamage(source, dest, dest, magnitude, elemental, card, readonlyCard);
        }

        public void PlayerAcquireAdditionalSkill(Player p, ISkill skill, Hero tag, bool undeletable = false)
        {
            if (p.IsDead) return;
            p.AcquireAdditionalSkill(skill, tag, undeletable);
            SkillSetChangedEventArgs args = new SkillSetChangedEventArgs();
            args.Source = p;
            args.Skills.Add(skill);
            args.IsLosingSkill = false;
            Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public void PlayerLoseAdditionalSkill(Player p, ISkill skill, bool undeletable = false)
        {
            if (!undeletable && !p.AdditionalSkills.Contains(skill)) return;
            p.LoseAdditionalSkill(skill, undeletable);
            SkillSetChangedEventArgs args = new SkillSetChangedEventArgs();
            args.Source = p;
            args.Skills.Add(skill);
            args.IsLosingSkill = true;
            Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public void HandleGodHero(Player p)
        {
            if (p.Allegiance == Heroes.Allegiance.God)
            {
                int answer = 0;
                UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("ChooseAllegiance"), Prompt.AllegianceChoices, out answer);
                if (answer == 0) p.Allegiance = Heroes.Allegiance.Qun;
                if (answer == 1) p.Allegiance = Heroes.Allegiance.Shu;
                if (answer == 2) p.Allegiance = Heroes.Allegiance.Wei;
                if (answer == 3) p.Allegiance = Heroes.Allegiance.Wu;
            }
        }


        public ReadOnlyCard Judge(Player player, ISkill skill = null, ICard handler = null, JudgementResultSucceed del = null)
        {
            ActionLog log = new ActionLog();
            log.SkillAction = skill;
            log.CardAction = handler;
            log.Source = player;
            log.GameAction = GameAction.Judge;
            CardsMovement move = new CardsMovement();
            Card c;
            int initCount = decks[player, DeckType.JudgeResult].Count;
            SyncImmutableCardAll(PeekCard(0));
            c = DrawCard();
            c.Log = log;
            move = new CardsMovement();
            move.Cards = new List<Card>();
            move.Cards.Add(c);
            move.To = new DeckPlace(player, DeckType.JudgeResult);
            MoveCards(move, false, GameDelayTypes.None);
            GameEventArgs args = new GameEventArgs();
            args.Source = player;
            if (triggers.ContainsKey(GameEvent.PlayerJudgeBegin) && triggers[GameEvent.PlayerJudgeBegin].Count > 0)
            {
                NotifyIntermediateJudgeResults(player, log, del);
            }
            Emit(GameEvent.PlayerJudgeBegin, args);
            c = Decks[player, DeckType.JudgeResult].Last();
            args.ReadonlyCard = new ReadOnlyCard(c);
            args.Cards = new List<Card>() { c };
            args.Skill = skill;
            args.Card = handler;
            bool? succeed = null;
            if (del != null)
            {
                succeed = del(args.ReadonlyCard);
            }

            Card uiCard = new Card(args.ReadonlyCard);
            uiCard.Id = (args.ReadonlyCard as ReadOnlyCard).Id;
            if (uiCard.Log == null)
            {
                uiCard.Log = new ActionLog();
            }
            uiCard.Log = log;
            NotificationProxy.NotifyJudge(player, uiCard, log, succeed);
            Emit(GameEvent.PlayerJudgeDone, args);
            Trace.Assert(args.Source == player);
            Trace.Assert(args.ReadonlyCard is ReadOnlyCard);

            if (decks[player, DeckType.JudgeResult].Count > initCount)
            {
                c = decks[player, DeckType.JudgeResult].Last();
                move = new CardsMovement();
                move.Cards = new List<Card>();
                move.Cards.Add(c);
                List<Card> backup = new List<Card>(move.Cards);
                move.To = new DeckPlace(null, DeckType.Discard);
                move.Helper = new MovementHelper();
                PlayerAboutToDiscardCard(player, move.Cards, DiscardReason.Judge);
                MoveCards(move, false, GameDelayTypes.None);
                PlayerDiscardedCard(player, backup, DiscardReason.Judge);
            }
            GameDelays.Delay(GameDelayTypes.JudgeEnd);
            return args.ReadonlyCard as ReadOnlyCard;
        }

        public void RecoverHealth(Player source, Player target, int magnitude)
        {
            if (target.IsDead) return;
            if (target.Health >= target.MaxHealth)
            {
                return;
            }
            var args = new HealthChangedEventArgs() { Source = source, Delta = magnitude };
            args.Targets.Add(target);

            Emit(GameEvent.BeforeHealthChanged, args);

            Trace.Assert(args.Targets.Count == 1);
            if (args.Targets[0].Health + args.Delta > args.Targets[0].MaxHealth)
            {
                args.Targets[0].Health = args.Targets[0].MaxHealth;
            }
            else
            {
                args.Targets[0].Health += args.Delta;
            }

            Trace.TraceInformation("Player {0} gain {1} hp, @ {2} hp", args.Targets[0].Id, args.Delta, args.Targets[0].Health);
            NotificationProxy.NotifyRecoverHealth(args.Targets[0], args.Delta);

            try
            {
                Emit(GameEvent.AfterHealthChanged, args);
            }
            catch (TriggerResultException)
            {
            }
        }

        public void LoseHealth(Player source, int magnitude)
        {
            if (source.IsDead) return;
            var args = new HealthChangedEventArgs() { Source = null, Delta = -magnitude };
            args.Targets.Add(source);

            Emit(GameEvent.BeforeHealthChanged, args);

            Trace.Assert(args.Targets.Count == 1);
            args.Targets[0].Health += args.Delta;
            Trace.TraceInformation("Player {0} lose {1} hp, @ {2} hp", args.Targets[0].Id, -args.Delta, args.Targets[0].Health);
            NotificationProxy.NotifyLoseHealth(args.Targets[0], -args.Delta);
            GameDelays.Delay(GameDelayTypes.Damage);

            try
            {
                Emit(GameEvent.AfterHealthChanged, args);
            }
            catch (TriggerResultException)
            {
            }

        }

        public void LoseMaxHealth(Player source, int magnitude)
        {
            if (source.IsDead) return;
            int result = source.MaxHealth - magnitude;
            bool trigger = false;
            if (source.Health > result)
            {
                source.Health = result;
                trigger = true;
            }
            source.MaxHealth = result;
            if (source.MaxHealth <= 0) Emit(GameEvent.GameProcessPlayerIsDead, new GameEventArgs() { Source = null, Targets = new List<Player>() { source } });
            if (trigger && !source.IsDead) Game.CurrentGame.Emit(Triggers.GameEvent.AfterHealthChanged, new HealthChangedEventArgs() { Source = null, Delta = 0, Targets = new List<Player>() { source } });
        }

        /// <summary>
        /// 处理玩家打出卡牌事件。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="c"></param>
        public void PlayerPlayedCard(Player source, List<Player> targets, ICard c)
        {
            Trace.Assert(c != null);
            try
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = source;
                arg.Targets = targets;
                arg.Card = c;
                arg.ReadonlyCard = new ReadOnlyCard(c);

                Emit(GameEvent.PlayerPlayedCard, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 处理玩家打出卡牌
        /// </summary>
        /// <param name="p"></param>
        /// <param name="skill"></param>
        /// <param name="cards"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public bool HandleCardPlay(Player p, ISkill skill, List<Card> cards, List<Player> targets)
        {
            Trace.Assert(cards != null);
            CardsMovement m = new CardsMovement();
            ICard result;
            bool status = CommitCardTransform(p, skill, cards, out result, targets);
            if (!status)
            {
                return false;
            }
            if (skill != null)
            {
                var r = result as CompositeCard;
                Trace.Assert(r != null);
                cards.Clear();
                cards.AddRange(r.Subcards);
            }
            m.Cards = new List<Card>(cards);
            m.To = new DeckPlace(null, DeckType.Discard);
            Player isDoingAFavor = p;
            foreach (var checkFavor in m.Cards)
            {
                if (checkFavor.Owner != p)
                {
                    Trace.TraceInformation("Acting on behalf of others");
                    isDoingAFavor = checkFavor.Owner;
                    break;
                }
            }
            result.Type.TagAndNotify(p, targets, result, GameAction.Play);
            List<Card> backup = new List<Card>(m.Cards);
            if (isDoingAFavor != p)
            {
                PlayerAboutToDiscardCard(isDoingAFavor, m.Cards, DiscardReason.Play);
                MoveCards(m, false, GameDelayTypes.PlayerAction);
                PlayerLostCard(p, m.Cards);
                PlayerPlayedCard(isDoingAFavor, targets, result);
                PlayerPlayedCard(p, targets, result);
                PlayerDiscardedCard(isDoingAFavor, backup, DiscardReason.Play);
            }
            else
            {
                PlayerAboutToDiscardCard(p, m.Cards, DiscardReason.Play);
                MoveCards(m, false, GameDelayTypes.PlayerAction);
                PlayerLostCard(p, m.Cards);
                PlayerPlayedCard(p, targets, result);
                PlayerDiscardedCard(p, backup, DiscardReason.Play);
            }
            Game.CurrentGame.LastAction = skill;
            return true;
        }

        public void PlayerDiscardedCard(Player p, List<Card> cards, DiscardReason reason)
        {
            try
            {
                var arg = new DiscardCardEventArgs();
                arg.Source = p;
                arg.Targets = null;
                arg.Cards = cards;
                arg.Reason = reason;
                Emit(GameEvent.CardsEnteredDiscardDeck, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        public void PlayerAboutToDiscardCard(Player p, List<Card> cards, DiscardReason reason)
        {
            SyncCardsAll(cards);
            try
            {
                var arg = new DiscardCardEventArgs();
                arg.Source = p;
                arg.Targets = null;
                arg.Cards = cards;
                arg.Reason = reason;
                Emit(GameEvent.CardsEnteringDiscardDeck, arg, true);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        public void PlayerLostCard(Player p, List<Card> cards)
        {
            try
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = p;
                arg.Targets = null;
                arg.Cards = cards;
                Emit(GameEvent.CardsLost, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        public void PlayerAcquiredCard(Player p, List<Card> cards)
        {
            try
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = p;
                arg.Targets = null;
                arg.Cards = cards;
                Emit(GameEvent.CardsAcquired, arg);
            }
            catch (TriggerResultException)
            {
                throw new NotImplementedException();
            }
        }

        public void HandleCardDiscard(Player p, List<Card> cards, DiscardReason reason = DiscardReason.Discard)
        {
            cards = new List<Card>(cards);
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            foreach (Card c in cards)
            {
                c.Log.Source = p;
                if (reason == DiscardReason.Discard)
                    c.Log.GameAction = GameAction.Discard;
                else if (reason == DiscardReason.Play)
                    c.Log.GameAction = GameAction.Play;
                else if (reason == DiscardReason.Use)
                    c.Log.GameAction = GameAction.Use;
            }
            List<Card> backup = new List<Card>(move.Cards);
            move.To = new DeckPlace(null, DeckType.Discard);
            PlayerAboutToDiscardCard(p, move.Cards, reason);
            MoveCards(move, false, GameDelayTypes.Discard);
            if (p != null)
            {
                PlayerLostCard(p, move.Cards);
                PlayerDiscardedCard(p, backup, reason);
            }
        }

        public void HandleCardTransferToHand(Player from, Player to, List<Card> cards, MovementHelper helper = null)
        {
            cards = new List<Card>(cards);
            if (to.IsDead)
            {
                if (cards.Any(cd => cd.Place.DeckType != DeckType.Hand && cd.Place.DeckType != DeckType.Equipment && cd.Place.DeckType != DeckType.DelayedTools))
                {
                    CardsMovement move1 = new CardsMovement();
                    move1.Cards = new List<Card>(cards);
                    move1.To = new DeckPlace(null, DeckType.Discard);
                    MoveCards(move1);
                    PlayerLostCard(from, cards);
                }
                return;
            }
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(to, DeckType.Hand);
            if (helper != null)
            {
                move.Helper = helper;
            }
            MoveCards(move);
            EnterAtomicContext();
            PlayerLostCard(from, cards);
            PlayerAcquiredCard(to, cards);
            ExitAtomicContext();
        }

        public void HandleCardTransfer(Player from, Player to, DeckType target, List<Card> cards, Hero tag = null)
        {
            cards = new List<Card>(cards);
            if (to.IsDead)
            {
                if (cards.Any(cd => cd.Place.DeckType != DeckType.Hand && cd.Place.DeckType != DeckType.Equipment && cd.Place.DeckType != DeckType.DelayedTools))
                {
                    CardsMovement move1 = new CardsMovement();
                    move1.Cards = new List<Card>(cards);
                    move1.To = new DeckPlace(null, DeckType.Discard);
                    MoveCards(move1);
                    PlayerLostCard(from, cards);
                }
                return;
            }
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(to, target);
            move.Helper = new MovementHelper();
            move.Helper.PrivateDeckHeroTag = tag;
            MoveCards(move);
            bool triggerAcquiredCard = target == DeckType.Hand || target == DeckType.Equipment;
            EnterAtomicContext();
            PlayerLostCard(from, cards);
            if (triggerAcquiredCard) PlayerAcquiredCard(to, cards);
            ExitAtomicContext();
        }


        public void ForcePlayerDiscard(Player player, NumberOfCardsToForcePlayerDiscard numberOfCards, bool canDiscardEquipment, bool atOnce = true)
        {
            if (player.IsDead) return;
            Trace.TraceInformation("Player {0} discard.", player);
            int cannotBeDiscarded = 0;
            int numberOfCardsDiscarded = 0;
            while (true)
            {
                int handCardCount = Decks[player, DeckType.Hand].Count; // 玩家手牌数
                int equipCardCount = Decks[player, DeckType.Equipment].Count; // 玩家装备牌数
                int toDiscard = numberOfCards(player, numberOfCardsDiscarded);
                // Have we finished discarding everything?
                // We finish if 
                //      玩家手牌数 小于等于 我们要强制弃掉的数目
                //  或者玩家手牌数 (小于)等于 不可弃的牌的数目（此时装备若可弃，必须弃光）
                if (toDiscard == 0 || (handCardCount <= cannotBeDiscarded && (!canDiscardEquipment || equipCardCount == 0)))
                {
                    break;
                }
                Trace.Assert(UiProxies.ContainsKey(player));
                IUiProxy proxy = UiProxies[player];
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                cannotBeDiscarded = 0;
                foreach (Card c in Decks[player, DeckType.Hand])
                {
                    if (!PlayerCanDiscardCard(player, c))
                    {
                        cannotBeDiscarded++;
                    }
                }
                //如果玩家无法达到弃牌要求 则 摊牌
                bool status = (canDiscardEquipment ? equipCardCount : 0) + handCardCount - toDiscard >= cannotBeDiscarded;
                SyncConfirmationStatus(ref status);
                if (!status)
                {
                    SyncImmutableCardsAll(Decks[player, DeckType.Hand]);
                    ShowHandCards(player, Decks[player, DeckType.Hand]);
                    if (Game.CurrentGame.IsClient)
                    {
                        //刷新所有客户端该玩家不可弃掉的牌的数目
                        cannotBeDiscarded = 0;
                        foreach (Card c in Decks[player, DeckType.Hand])
                        {
                            if (!PlayerCanDiscardCard(player, c))
                            {
                                cannotBeDiscarded++;
                            }
                        }
                    }
                }
                int minimum;
                if (!atOnce) minimum = 1;
                else minimum = status ? toDiscard : (canDiscardEquipment ? equipCardCount : 0) + handCardCount - cannotBeDiscarded;
                PlayerForceDiscardVerifier v = new PlayerForceDiscardVerifier(toDiscard, canDiscardEquipment, minimum);
                if (!proxy.AskForCardUsage(new Prompt(Prompt.DiscardPhasePrompt, toDiscard),
                                           v, out skill, out cards, out players))
                {
                    //玩家没有回应(default)
                    Trace.TraceInformation("Invalid answer, choosing for you");
                    cards = new List<Card>();
                    int cardsDiscarded = 0;
                    var chooseFrom = new List<Card>(Decks[player, DeckType.Hand]);
                    if (canDiscardEquipment)
                    {
                        chooseFrom.AddRange(Decks[player, DeckType.Equipment]);
                    }
                    foreach (Card c in chooseFrom)
                    {
                        if (PlayerCanDiscardCard(player, c))
                        {
                            cards.Add(c);
                            cardsDiscarded++;
                        }
                        if (cardsDiscarded == toDiscard)
                        {
                            SyncCardsAll(cards);
                            break;
                        }
                    }
                }
                numberOfCardsDiscarded += cards.Count;
                HandleCardDiscard(player, cards);
            }
        }


        public void InsertBeforeDeal(Player target, List<Card> list, MovementHelper helper = null)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(list);
            move.Cards.Reverse();
            move.To = new DeckPlace(null, DeckType.Dealing);
            if (helper != null)
            {
                move.Helper = helper;
            }
            MoveCards(move, true, GameDelayTypes.None);
            if (target != null)
            {
                PlayerLostCard(target, list);
            }
        }

        public void InsertAfterDeal(Player target, List<Card> list, MovementHelper helper = null)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(list);
            move.To = new DeckPlace(null, DeckType.Dealing);
            move.Helper.IsFakedMove = true;
            if (helper != null)
            {
                move.Helper = helper;
            }
            MoveCards(move, false, GameDelayTypes.None);
            if (target != null)
            {
                PlayerLostCard(target, list);
            }
        }

        public void PlaceIntoDiscard(Player target, List<Card> list)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(list);
            move.To = new DeckPlace(null, DeckType.Discard);
            move.Helper = new MovementHelper();
            MoveCards(move);
            if (target != null)
            {
                PlayerLostCard(target, list);
            }
        }

        public bool PlayerCanDiscardCards(Player p, List<Card> cards)
        {
            foreach (Card c in cards)
            {
                if (!PlayerCanDiscardCard(p, c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool PlayerCanBeTargeted(Player source, List<Player> targets, ICard card)
        {
            GameEventArgs arg = new GameEventArgs();
            arg.Source = source;
            arg.Targets = targets;
            arg.Card = card;
            try
            {
                Emit(GameEvent.PlayerCanBeTargeted, arg);
                return true;
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Fail)
                {
                    Trace.TraceInformation("Players cannot be targeted by {0}", card.Type.CardType);
                    return false;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            return true;
        }

        public void PinDianReturnCards(Player from, Player to, out Card c1, out Card c2, ISkill skill)
        {
            NotificationProxy.NotifyPinDianStart(from, to, skill);
            Dictionary<Player, ISkill> aSkill;
            Dictionary<Player, List<Card>> aCards;
            Dictionary<Player, List<Player>> aPlayers;

            GlobalProxy.AskForMultipleCardUsage(new CardUsagePrompt("PinDian"), new PinDianVerifier(), new List<Player>() { from, to }, out aSkill, out aCards, out aPlayers);
            Card card1, card2;
            if (!aCards.ContainsKey(from) || aCards[from].Count == 0)
            {
                card1 = Decks[from, DeckType.Hand][0];
                SyncImmutableCardAll(card1);
            }
            else
            {
                card1 = aCards[from][0];
            }
            if (!aCards.ContainsKey(to) || aCards[to].Count == 0)
            {
                card2 = Decks[to, DeckType.Hand][0];
                SyncImmutableCardAll(card2);
            }
            else
            {
                card2 = aCards[to][0];
            }
            c1 = card1;
            c2 = card2;
            NotificationProxy.NotifyPinDianEnd(c1, c2);
        }

        public bool? PinDian(Player from, Player to, ISkill skill)
        {
            Card card1, card2;
            PinDianReturnCards(from, to, out card1, out card2, skill);
            EnterAtomicContext();
            PlaceIntoDiscard(from, new List<Card>() { card1 });
            PlaceIntoDiscard(to, new List<Card>() { card2 });
            ExitAtomicContext();
            if (card1.Rank > card2.Rank) return true;
            if (card1.Rank < card2.Rank) return false;
            return null;
        }

        public Card SelectACardFrom(Player from, Player ask, Prompt prompt, String resultdeckname, bool equipExcluded = false, bool delayedToolsExcluded = true, bool noReveal = false)
        {
            var deck = from.HandCards();
            if (!equipExcluded) deck = new List<Card>(deck.Concat(from.Equipments()));
            if (!delayedToolsExcluded) deck = new List<Card>(deck.Concat(from.DelayedTools()));
            if (deck.Count == 0) return null;
            List<DeckPlace> places = new List<DeckPlace>();
            places.Add(new DeckPlace(from, DeckType.Hand));
            if (!equipExcluded) places.Add(new DeckPlace(from, DeckType.Equipment));
            if (!delayedToolsExcluded) places.Add(new DeckPlace(from, DeckType.DelayedTools));
            List<List<Card>> answer;

            if (!ask.AskForCardChoice(prompt, places, new List<string>() { resultdeckname }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(noReveal), out answer))
            {
                Trace.TraceInformation("Player {0} Invalid answer", ask);
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(deck.First());
            }
            Card theCard = answer[0][0];
            if (noReveal)
            {
                SyncCard(from, ref theCard);
            }
            else
            {
                SyncCardAll(ref theCard);
            }
            Trace.Assert(answer.Count == 1 && answer[0].Count == 1);
            return theCard;
        }

        public void HideHandCard(Card c)
        {
            if (IsClient && GameClient.SelfId != c.Place.Player.Id && c.Place.DeckType == DeckType.Hand)
            {
                c.Id = -1;
            }
        }

        public void ShowHandCards(Player p, List<Card> cards)
        {
            if (cards.Count == 0) return;
            NotificationProxy.NotifyShowCardsStart(p, cards);
            Dictionary<Player, int> answers;
            GlobalProxy.AskForMultipleChoice(new MultipleChoicePrompt("ShowCards", p), new List<OptionPrompt>() { OptionPrompt.YesChoice }, AlivePlayers, out answers);
            NotificationProxy.NotifyShowCardsEnd();
            foreach (Card c in cards) Game.CurrentGame.HideHandCard(c);
        }

        public List<Card> PickDefaultCardsFrom(List<DeckPlace> places, int n = 1)
        {
            List<Card> cards = new List<Card>();
            foreach (var pl in places)
            {
                cards.AddRange(Decks[pl]);
            }
            List<Card> result = new List<Card>();
            while (n-- > 0)
            {
                if (cards.Count == 0) return result;
                var theCard = cards.First();
                cards.Remove(theCard);
                if (theCard.Place.DeckType == DeckType.Hand)
                {
                    SyncCard(theCard.Place.Player, ref theCard);
                }
                result.Add(theCard);
            }
            return result;
        }

        public void RegisterSkillCleanup(ISkill skill, DeckType deck)
        {
            cleanupSquad.CalldownCleanupCrew(skill, deck);
        }

        public void RegisterMarkCleanup(ISkill skill, PlayerAttribute attr)
        {
            cleanupSquad.CalldownCleanupCrew(skill, attr);
        }

        public bool IsMainHero(Hero h, Player p)
        {
            return h == p.Hero;
        }

        public ISkill LastAction { get; set; }
    }
}
