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


namespace Sanguosha.Core.Games
{
    public abstract partial class Game
    {
        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="source">伤害来源</param>
        /// <param name="dest">伤害目标</param>
        /// <param name="magnitude">伤害点数</param>
        /// <param name="elemental">伤害属性</param>
        /// <param name="cards">造成伤害的牌</param>
        public void DoDamage(Player source, Player dest, int magnitude, DamageElement elemental, ICard card, ReadOnlyCard readonlyCard)
        {
            var damageArgs = new DamageEventArgs() { Source = source, Targets = new List<Player>(), Magnitude = magnitude, Element = elemental };
            HealthChangedEventArgs healthChangedArgs;
            int ironShackledDamage = 0;
            DamageElement ironShackledDamageElement = DamageElement.None;
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
                Game.CurrentGame.Emit(GameEvent.DamageSourceConfirmed, damageArgs);
                Game.CurrentGame.Emit(GameEvent.DamageElementConfirmed, damageArgs);
                Game.CurrentGame.Emit(GameEvent.BeforeDamageComputing, damageArgs);
                Game.CurrentGame.Emit(GameEvent.DamageComputingStarted, damageArgs);
                Game.CurrentGame.Emit(GameEvent.DamageCaused, damageArgs);
                Game.CurrentGame.Emit(GameEvent.DamageInflicted, damageArgs);
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
                }
                healthChangedArgs = new HealthChangedEventArgs(damageArgs);
                Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, healthChangedArgs);
                damageArgs.Magnitude = -healthChangedArgs.Delta;
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.End)
                {
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

            try
            {
                Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, healthChangedArgs);
            }
            catch (TriggerResultException)
            {
            }
            Game.CurrentGame.Emit(GameEvent.AfterDamageCaused, damageArgs);
            Game.CurrentGame.Emit(GameEvent.AfterDamageInflicted, damageArgs);
            Game.CurrentGame.Emit(GameEvent.DamageComputingFinished, damageArgs);
            if (ironShackledDamage != 0)
            {
                List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
                Game.CurrentGame.SortByOrderOfComputation(damageArgs.Targets[0], toProcess);
                foreach (Player p in toProcess)
                {
                    if (p.IsIronShackled)
                    {
                        DoDamage(damageArgs.Source, p, ironShackledDamage, ironShackledDamageElement, card, readonlyCard);
                    }
                }
            }
        }

        public void PlayerAcquireSkill(Player p, ISkill skill, bool undeletable = false)
        {
            p.AcquireAdditionalSkill(skill, undeletable);
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public void PlayerLoseSkill(Player p, ISkill skill, bool undeletable = false)
        {
            p.LoseAdditionalSkill(skill, undeletable);
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, args);
            _ResetCards(p);
        }

        public void HandleGodHero(Player p)
        {
            if (p.Allegiance == Heroes.Allegiance.God)
            {
                int answer = 0;
                Game.CurrentGame.UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("ChooseAllegiance"), Prompt.AllegianceChoices, out answer);
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
            CardsMovement move = new CardsMovement();
            Card c;
            int initCount = decks[player, DeckType.JudgeResult].Count;
            SyncImmutableCardAll(PeekCard(0));
            c = Game.CurrentGame.DrawCard();
            move = new CardsMovement();
            move.Cards = new List<Card>();
            move.Cards.Add(c);
            move.To = new DeckPlace(player, DeckType.JudgeResult);
            MoveCards(move);
            GameEventArgs args = new GameEventArgs();
            args.Source = player;
            if (Game.CurrentGame.triggers.ContainsKey(GameEvent.PlayerJudgeBegin) && Game.CurrentGame.triggers[GameEvent.PlayerJudgeBegin].Count > 0)
            {
                NotifyIntermediateJudgeResults(player, log, del);
            }
            Game.CurrentGame.Emit(GameEvent.PlayerJudgeBegin, args);
            c = Game.CurrentGame.Decks[player, DeckType.JudgeResult].Last();
            args.ReadonlyCard = new ReadOnlyCard(c);
            args.Cards = new List<Card>() { c };
            args.Skill = skill;
            args.Card = handler;
            Game.CurrentGame.Emit(GameEvent.PlayerJudgeDone, args);
            Trace.Assert(args.Source == player);
            Trace.Assert(args.ReadonlyCard is ReadOnlyCard);

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
            uiCard.Log.Source = player;
            uiCard.Log.CardAction = handler;
            uiCard.Log.SkillAction = skill;
            uiCard.Log.GameAction = GameAction.Judge;
            Game.CurrentGame.NotificationProxy.NotifyJudge(player, uiCard, log, succeed);

            if (decks[player, DeckType.JudgeResult].Count > initCount)
            {
                c = decks[player, DeckType.JudgeResult].Last();
                move = new CardsMovement();
                move.Cards = new List<Card>();
                List<Card> backup = new List<Card>(move.Cards);
                move.Cards.Add(c);
                move.To = new DeckPlace(null, DeckType.Discard);
                move.Helper = new MovementHelper();
                PlayerAboutToDiscardCard(player, move.Cards, DiscardReason.Judge);
                MoveCards(move);
                PlayerDiscardedCard(player, backup, DiscardReason.Judge);
            }
            Thread.Sleep(500);
            return args.ReadonlyCard as ReadOnlyCard;
        }

        public void RecoverHealth(Player source, Player target, int magnitude)
        {
            if (target.Health >= target.MaxHealth)
            {
                return;
            }
            var args = new HealthChangedEventArgs() { Source = source, Delta = magnitude };
            args.Targets.Add(target);

            Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

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
            Game.CurrentGame.NotificationProxy.NotifyRecoverHealth(args.Targets[0], args.Delta);

            try
            {
                Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
            }
            catch (TriggerResultException)
            {
            }
        }

        public void LoseHealth(Player source, int magnitude)
        {
            var args = new HealthChangedEventArgs() { Source = source, Delta = -magnitude };
            args.Targets.Add(source);

            Game.CurrentGame.Emit(GameEvent.BeforeHealthChanged, args);

            Trace.Assert(args.Targets.Count == 1);
            args.Targets[0].Health += args.Delta;
            Trace.TraceInformation("Player {0} lose {1} hp, @ {2} hp", args.Targets[0].Id, -args.Delta, args.Targets[0].Health);
            Game.CurrentGame.NotificationProxy.NotifyLoseHealth(args.Targets[0], -args.Delta);

            try
            {
                Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, args);
            }
            catch (TriggerResultException)
            {
            }

        }

        public void LoseMaxHealth(Player source, int magnitude)
        {
            int result = source.MaxHealth - magnitude;
            if (source.Health > result) source.Health = result;
            source.MaxHealth = result;
            if (source.MaxHealth <= 0) Game.CurrentGame.Emit(GameEvent.GameProcessPlayerIsDead, new GameEventArgs() { Source = null, Targets = new List<Player>() { source } });
        }

        /// <summary>
        /// 处理玩家打出卡牌事件。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="c"></param>
        public void PlayerPlayedCard(Player source, List<Player> targets, ICard c)
        {
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
                MoveCards(m);
                PlayerLostCard(p, backup);
                PlayerPlayedCard(isDoingAFavor, targets, result);
                PlayerPlayedCard(p, targets, result);
                PlayerDiscardedCard(isDoingAFavor, backup, DiscardReason.Play);
            }
            else
            {
                PlayerAboutToDiscardCard(p, m.Cards, DiscardReason.Play);
                MoveCards(m);
                PlayerLostCard(p, backup);
                PlayerPlayedCard(p, targets, result);
                PlayerDiscardedCard(p, backup, DiscardReason.Play);
            }
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
            if (atomic)
            {
                if (!cards.Any(cc => cc.Place.DeckType == DeckType.Hand || cc.Place.DeckType == DeckType.Equipment)) return;
            }
            else
            {
                if (!cards.Any(cc => cc.HistoryPlace1.DeckType == DeckType.Hand || cc.HistoryPlace1.DeckType == DeckType.Equipment)) return;
            }
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
            MoveCards(move);
            if (p != null)
            {
                PlayerLostCard(p, backup);
                PlayerDiscardedCard(p, backup, reason);
            }
        }

        public void HandleCardTransferToHand(Player from, Player to, List<Card> cards, MovementHelper helper = null)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(to, DeckType.Hand);
            if (helper != null)
            {
                move.Helper = helper;
            }
            MoveCards(move);
            PlayerLostCard(from, cards);
            PlayerAcquiredCard(to, cards);
        }

        public void HandleCardTransfer(Player from, Player to, DeckType target, List<Card> cards)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(to, target);
            move.Helper = new MovementHelper();
            MoveCards(move);
            PlayerLostCard(from, cards);
            PlayerAcquiredCard(to, cards);
        }


        public void ForcePlayerDiscard(Player player, NumberOfCardsToForcePlayerDiscard numberOfCards, bool canDiscardEquipment)
        {
            Trace.TraceInformation("Player {0} discard.", player);
            int cannotBeDiscarded = 0;
            int numberOfCardsDiscarded = 0;
            while (true)
            {
                int handCardCount = Game.CurrentGame.Decks[player, DeckType.Hand].Count; // 玩家手牌数
                int equipCardCount = Game.CurrentGame.Decks[player, DeckType.Equipment].Count; // 玩家装备牌数
                int toDiscard = numberOfCards(player, numberOfCardsDiscarded);
                // Have we finished discarding everything?
                // We finish if 
                //      玩家手牌数 小于等于 我们要强制弃掉的数目
                //  或者玩家手牌数 (小于)等于 不可弃的牌的数目（此时装备若可弃，必须弃光）
                if (toDiscard == 0 || (handCardCount <= cannotBeDiscarded && (!canDiscardEquipment || equipCardCount == 0)))
                {
                    break;
                }
                Trace.Assert(Game.CurrentGame.UiProxies.ContainsKey(player));
                IUiProxy proxy = Game.CurrentGame.UiProxies[player];
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                PlayerForceDiscardVerifier v = new PlayerForceDiscardVerifier(toDiscard, canDiscardEquipment);
                cannotBeDiscarded = 0;
                foreach (Card c in Game.CurrentGame.Decks[player, DeckType.Hand])
                {
                    if (!Game.CurrentGame.PlayerCanDiscardCard(player, c))
                    {
                        cannotBeDiscarded++;
                    }
                }
                //如果玩家无法达到弃牌要求 则 摊牌
                bool status = (canDiscardEquipment ? equipCardCount : 0) + handCardCount - toDiscard >= cannotBeDiscarded;
                Game.CurrentGame.SyncConfirmationStatus(ref status);
                if (!status)
                {
                    Game.CurrentGame.SyncImmutableCardsAll(Game.CurrentGame.Decks[player, DeckType.Hand]);
                }

                if (!proxy.AskForCardUsage(new Prompt(Prompt.DiscardPhasePrompt, toDiscard),
                                           v, out skill, out cards, out players))
                {
                    //玩家没有回应(default)
                    //如果玩家有不可弃掉的牌(这个只有服务器知道） 则通知所有客户端该玩家手牌
                    status = (cannotBeDiscarded == 0);
                    Game.CurrentGame.SyncConfirmationStatus(ref status);
                    if (!status)
                    {
                        Game.CurrentGame.SyncImmutableCardsAll(Game.CurrentGame.Decks[player, DeckType.Hand]);
                    }
                    cannotBeDiscarded = 0;
                    foreach (Card c in Game.CurrentGame.Decks[player, DeckType.Hand])
                    {
                        if (!Game.CurrentGame.PlayerCanDiscardCard(player, c))
                        {
                            cannotBeDiscarded++;
                        }
                    }

                    Trace.TraceInformation("Invalid answer, choosing for you");
                    cards = new List<Card>();
                    int cardsDiscarded = 0;
                    var chooseFrom = new List<Card>(Game.CurrentGame.Decks[player, DeckType.Hand]);
                    if (canDiscardEquipment)
                    {
                        chooseFrom.AddRange(Game.CurrentGame.Decks[player, DeckType.Equipment]);
                    }
                    foreach (Card c in chooseFrom)
                    {
                        if (Game.CurrentGame.PlayerCanDiscardCard(player, c))
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
                Game.CurrentGame.HandleCardDiscard(player, cards);
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
            MoveCards(move, true);
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
            MoveCards(move);
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
                Game.CurrentGame.Emit(GameEvent.PlayerCanBeTargeted, arg);
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
            Game.CurrentGame.NotificationProxy.NotifyPinDianStart(from, to, skill);
            Dictionary<Player, ISkill> aSkill;
            Dictionary<Player, List<Card>> aCards;
            Dictionary<Player, List<Player>> aPlayers;

            GlobalProxy.AskForMultipleCardUsage(new CardUsagePrompt("PinDian"), new PinDianVerifier(), new List<Player>() { from, to }, out aSkill, out aCards, out aPlayers);
            Card card1, card2;
            if (!aCards.ContainsKey(from) || aCards[from].Count == 0)
            {
                card1 = Game.CurrentGame.Decks[from, DeckType.Hand][0];
                SyncImmutableCardAll(card1);
            }
            else
            {
                card1 = aCards[from][0];
            }
            if (!aCards.ContainsKey(to) || aCards[to].Count == 0)
            {
                card2 = Game.CurrentGame.Decks[to, DeckType.Hand][0];
                SyncImmutableCardAll(card2);
            }
            else
            {
                card2 = aCards[to][0];
            }
            c1 = card1;
            c2 = card2;
            Game.CurrentGame.NotificationProxy.NotifyPinDianEnd(c1, c2);
        }

        public bool PinDian(Player from, Player to, ISkill skill)
        {
            Card card1, card2;
            PinDianReturnCards(from, to, out card1, out card2, skill);
            EnterAtomicContext();
            PlaceIntoDiscard(from, new List<Card>() { card1 });
            PlaceIntoDiscard(to, new List<Card>() { card2 });
            PlayerLostCard(from, new List<Card>() { card1 });
            PlayerLostCard(to, new List<Card>() { card2 });
            ExitAtomicContext();
            return card1.Rank > card2.Rank;
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

            if (!ask.AskForCardChoice(prompt, places, new List<string>() {resultdeckname}, new List<int>() {1}, new RequireOneCardChoiceVerifier(noReveal), out answer))
            {
                Trace.TraceInformation("Player {0} Invalid answer", ask);
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(deck.First());
            }
            Card theCard = answer[0][0];
            if (noReveal)
            {
                Game.CurrentGame.SyncCard(from, ref theCard);
            }
            else
            {
                Game.CurrentGame.SyncCardAll(ref theCard);
            }
            Trace.Assert(answer.Count == 1 && answer[0].Count == 1);
            return theCard;
        }

        public void HideHandCard(Card c)
        {
            if (Game.CurrentGame.IsClient && Game.CurrentGame.GameClient.SelfId != c.Place.Player.Id && c.Place.DeckType == DeckType.Hand)
            {
                c.Id = -1;
            }
        }
    }
}
