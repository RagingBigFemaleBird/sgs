using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;

using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using Sanguosha.Core.UI;
using System.Diagnostics;
using Sanguosha.Lobby.Core;

namespace Sanguosha.UI.Controls
{
    public class PlayerViewModel : SelectableItem, IAsyncUiProxy
    {
        #region Constructors
        public PlayerViewModel()
        {
            Hero1Model = new HeroViewModel();
            Hero2Model = new HeroViewModel();

            IsSelectionMode = false;
            AutoInvokeSkillCommands = new ObservableCollection<SkillCommand>();
            ActiveSkillCommands = new ObservableCollection<SkillCommand>();
            RulerGivenSkillCommands = new ObservableCollection<SkillCommand>();

            PrivateDecks = new ObservableCollection<PrivateDeckViewModel>();

            MultiChoiceCommands = new ObservableCollection<ICommand>();

            submitCardUsageCommand = new SimpleRelayCommand(SubmitCardUsageCommand);
            cancelCardUsageCommand = new SimpleRelayCommand(CancelCardUsageCommand);
            abortCardUsageCommand = new SimpleRelayCommand(AbortCardUsageCommand);

            SubmitAnswerCommand = DisabledCommand;
            CancelAnswerCommand = DisabledCommand;
            AbortAnswerCommand = DisabledCommand;

            _possibleRoles = new ObservableCollection<Role>();
            _updateCardUsageStatusHandler = (o, e) => { _UpdateCardUsageStatus(); };
            _onSkillCommandSelectedHandler = _OnSkillCommandSelected;
            IsCardChoiceQuestionShown = false;

            Marks = new ObservableCollection<MarkViewModel>();
            StatusMarks = new ObservableCollection<MarkViewModel>();
            HandCards = new ObservableCollection<CardViewModel>();
            verifierLock = new object();
            _lastSelectedPlayers = new List<Player>();
        }

        public PlayerViewModel(Player player, GameViewModel game)
            : this()
        {
            Player = player;
            GameModel = game;
        }
        #endregion

        #region Fields
        Player _player;

        public bool IsPlayable
        {
            get
            {
                return GameModel.IsPlayable && this == GameModel.MainPlayerModel;
            }
        }

        public bool CanSpectate
        {
            get
            {
                return !GameModel.IsPlayable;
            }
        }

        // @todo: to be deprecated. use HostPlayer instead.
        public Player Player
        {
            get { return _player; }
            set
            {
                if (_player == value) return;
                if (_player != null)
                {
                    _player.PropertyChanged -= _PropertyChanged;
                }
                _player = value;
                if (_player != null)
                {
                    _PropertyChanged = _OnPlayerPropertyChanged;
                    _player.PropertyChanged += _PropertyChanged;
                }
                var properties = typeof(Player).GetProperties();
                foreach (var property in properties)
                {
                    _OnPlayerPropertyChanged(_player, new PropertyChangedEventArgs(property.Name));
                }
            }
        }

        private GameViewModel _game;

        public GameViewModel GameModel
        {
            get { return _game; }
            set
            {
                if (_game == value) return;
                bool changed = (_game == null || _game.GetType() != value.GetType());
                _game = value;
                _game.Game.PropertyChanged += new PropertyChangedEventHandler(_game_PropertyChanged);
                if (changed)
                {
                    _UpdatePossibleRoles();
                    OnPropertyChanged("PossibleRoles");
                }
            }
        }

        void _game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            if (name == "CurrentPlayer")
            {
                OnPropertyChanged("IsCurrentPlayer");
            }
            else if (name == "CurrentPhase")
            {
                OnPropertyChanged("CurrentPhase");
            }
        }

        private PropertyChangedEventHandler _PropertyChanged;

        private void _UpdateSkills()
        {
            // SkillCommands.Clear();
            ActiveSkillCommands.Clear();
            AutoInvokeSkillCommands.Clear();

            if (_player == null) return;

            var backup = new List<SkillCommand>(DockedSkillCommands);
            backup.AddRange(RulerGivenSkillCommands);

            foreach (ISkill skill in _player.Skills)
            {
                if (!backup.Any(s => s.Skill == skill))
                {
                    SkillCommand command;

                    if (skill is IAdditionalTypedSkill)
                    {
                        command = new GuHuoSkillCommand() { Skill = skill, IsEnabled = false };
                    }
                    else
                    {
                        command = new SkillCommand() { Skill = skill, IsEnabled = false };
                    }

                    if (command.IsAutoInvokeSkill)
                    {
                        command.IsEnabled = true;
                        command.OnSelectedChanged += triggerSkill_OnSelectedChanged;
                    }

                    if (command.HeroName != null)
                    {
                        RulerGivenSkillCommands.Add(command);
                    }
                    else
                    {
                        var hero = GetHeroModel(skill.HeroTag);
                        Trace.Assert(hero != null);
                        hero.SkillCommands.Add(command);
                    }

                }
            }

            foreach (var skillCommand in backup)
            {
                if (!_player.Skills.Any(s => skillCommand.Skill == s))
                {
                    AutoInvokeSkillCommands.Remove(skillCommand);
                    if (Hero1Model != null) Hero1Model.SkillCommands.Remove(skillCommand);
                    if (Hero2Model != null) Hero2Model.SkillCommands.Remove(skillCommand);
                    RulerGivenSkillCommands.Remove(skillCommand);
                }
            }

            foreach (var command in SkillCommands)
            {
                if (command.IsAutoInvokeSkill)
                {
                    AutoInvokeSkillCommands.Add(command);
                }
                else if ((command.Skill is ActiveSkill || command.Skill is CardTransformSkill))
                {
                    ActiveSkillCommands.Add(command);
                }
            }

            Hero1Model.UpdateSkillNames();
            Hero2Model.UpdateSkillNames();
        }

        private void _OnPlayerPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            if (name == "Role")
            {
                _UpdatePossibleRoles();
                OnPropertyChanged("Role");
                OnPropertyChanged("PossibleRoles");
            }
            else if (name == "Hero")
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    Hero1Model.Hero = Hero;
                    OnPropertyChanged("Hero");
                });
            }
            else if (name == "Hero2")
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    Hero2Model.Hero = Hero2;
                    OnPropertyChanged("Hero2");
                });
            }
            else if (name == "Skills")
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateSkills();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate() { _UpdateSkills(); });
                }
            }
            else if (name == "Attributes")
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateAttributes();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate() { _UpdateAttributes(); });
                }
            }
            else
            {
                var propNames = from prop in this.GetType().GetProperties() select prop.Name;
                if (propNames.Contains(name))
                {
                    OnPropertyChanged(name);
                }
            }
        }

        private void _UpdateAttributes()
        {
            if (_player == null)
            {
                IsDrank = false;
                IsDying = false;
                StatusMarks.Clear();
                Marks.Clear();
                return;
            }

            foreach (var attribute in _player.Attributes.Keys)
            {
                if (attribute == Sanguosha.Expansions.Battle.Cards.Jiu.Drank)
                {
                    IsDrank = (_player[attribute] == 1);
                }
                else if (attribute == Player.IsDying)
                {
                    IsDying = (_player[attribute] == 1);
                }
                else if (attribute.IsMark)
                {
                    int count = _player[attribute];
                    MarkViewModel model = null;
                    foreach (var mark in Marks)
                    {
                        if (mark.PlayerAttribute == attribute)
                        {
                            model = mark;
                            break;
                        }
                    }

                    if (model == null)
                    {
                        model = new MarkViewModel() { PlayerAttribute = attribute };
                        Marks.Add(model);
                    }

                    model.Number = _player[attribute];
                }
                else if (attribute.IsStatus)
                {
                    int count = _player[attribute];
                    MarkViewModel model = null;
                    foreach (var mark in StatusMarks)
                    {
                        if (mark.PlayerAttribute == attribute)
                        {
                            model = mark;
                            break;
                        }
                    }

                    if (model == null)
                    {
                        model = new MarkViewModel() { PlayerAttribute = attribute };
                        StatusMarks.Add(model);
                    }

                    model.Number = _player[attribute];
                }
            }
        }

        #endregion

        #region Decks
        public ObservableCollection<PrivateDeckViewModel> PrivateDecks
        {
            get;
            private set;
        }
        #endregion

        #region Commands

        private ICommand _submitAnswerCommand;
        public ICommand SubmitAnswerCommand
        {
            get
            {
                return _submitAnswerCommand;
            }
            internal set
            {
                if (_submitAnswerCommand == value) return;
                _submitAnswerCommand = value;
                OnPropertyChanged("SubmitAnswerCommand");
            }
        }

        private ICommand _cancelAnswerCommand;
        public ICommand CancelAnswerCommand
        {
            get
            {
                return _cancelAnswerCommand;
            }
            internal set
            {
                if (_cancelAnswerCommand == value) return;
                _cancelAnswerCommand = value;
                OnPropertyChanged("CancelAnswerCommand");
            }
        }

        private ICommand _abortAnswerCommand;
        public ICommand AbortAnswerCommand
        {
            get
            {
                return _abortAnswerCommand;
            }
            internal set
            {
                if (_abortAnswerCommand == value) return;
                _abortAnswerCommand = value;
                OnPropertyChanged("AbortAnswerCommand");
            }
        }

        #endregion

        #region Interactions

        private double _timeOutSeconds;
        public double TimeOutSeconds
        {
            get
            {
                return _timeOutSeconds;
            }
            set
            {
                if (_timeOutSeconds == value) return;
                _timeOutSeconds = value;
                OnPropertyChanged("TimeOutSeconds");
            }
        }

        #endregion

        #region Player Properties
        public Allegiance Allegiance
        {
            get
            {
                if (_player == null)
                {
                    return Allegiance.Unknown;
                }
                else
                {
                    return _player.Allegiance;
                }
            }
        }

        public Role Role
        {
            get
            {
                if (_player == null) return Role.Unknown;
                else return _player.Role;
            }
        }

        public int Health
        {
            get
            {
                if (_player == null)
                {
                    return 0;
                }
                else
                {
                    return _player.Health;
                }
            }
        }

        public int MaxHealth
        {
            get
            {
                if (_player == null)
                {
                    return 0;
                }
                else
                {
                    return _player.MaxHealth;
                }
            }
        }

        public Hero Hero
        {
            get
            {
                if (_player == null) return null;
                return _player.Hero;
            }
        }

        public Hero Hero2
        {
            get
            {
                if (_player == null) return null;
                return _player.Hero2;
            }
        }

        public int Id
        {
            get
            {
                if (_player == null) return -1;
                return _player.Id;
            }
        }

        public bool IsFemale
        {
            get
            {
                if (_player == null) return false;
                return _player.IsFemale;
            }
        }

        public bool IsMale
        {
            get
            {
                if (_player == null) return false;
                return _player.IsMale;
            }
        }

        public bool IsIronShackled { get { if (_player == null) return false; return _player.IsIronShackled; } }

        public bool IsImprisoned { get { return _player != null && _player.IsImprisoned; } }

        private bool _isDrank;

        public bool IsDrank
        {
            get { return _isDrank; }
            set
            {
                if (_isDrank == value) return;
                _isDrank = value;
                OnPropertyChanged("IsDrank");
            }
        }

        public bool IsTargeted { get { return _player != null && _player.IsTargeted; } }

        public bool IsCurrentPlayer
        {
            get
            {
                if (_game == null) return false;
                else
                {
                    return _player == _game.Game.CurrentPlayer;
                }
            }
        }

        public TurnPhase CurrentPhase
        {
            get
            {
                if (!IsCurrentPlayer)
                {
                    return TurnPhase.Inactive;
                }
                return _game.Game.CurrentPhase;
            }
        }

        public bool IsDead
        {
            get
            {
                return _player != null && _player.IsDead;
            }
        }

        #endregion

        #region Derived Player Properties

        public HeroViewModel Hero1Model
        {
            get;
            set;
        }

        public HeroViewModel Hero2Model
        {
            get;
            set;
        }

        public ObservableCollection<MarkViewModel> Marks
        {
            get;
            private set;
        }

        public ObservableCollection<MarkViewModel> StatusMarks
        {
            get;
            private set;
        }

        public ObservableCollection<CardViewModel> HandCards
        {
            get;
            private set;
        }

        public ObservableCollection<SkillCommand> RulerGivenSkillCommands
        {
            get;
            private set;
        }

        public IEnumerable<SkillCommand> DockedSkillCommands
        {
            get
            {
                IEnumerable<SkillCommand> result = new List<SkillCommand>();
                if (Hero1Model != null) result = result.Concat(Hero1Model.SkillCommands);
                if (Hero2Model != null) result = result.Concat(Hero2Model.SkillCommands);
                return result;
            }
        }

        public ObservableCollection<SkillCommand> ActiveSkillCommands
        {
            get;
            private set;
        }

        public ObservableCollection<SkillCommand> AutoInvokeSkillCommands
        {
            get;
            private set;
        }

        public IEnumerable<SkillCommand> SkillCommands
        {
            get
            {
                return DockedSkillCommands.Concat(RulerGivenSkillCommands);
            }
        }

        private bool _isDying;

        public bool IsDying
        {
            get
            {
                return _isDying;
            }
            private set
            {
                if (_isDying == value) return;
                _isDying = value;
                OnPropertyChanged("IsDying");
            }
        }

        private PrivateDeckViewModel _currentPrivateDeck;

        public PrivateDeckViewModel CurrentPrivateDeck
        {
            get { return _currentPrivateDeck; }
            set
            {
                if (_currentPrivateDeck == value) return;
                _currentPrivateDeck = value;
                OnPropertyChanged("CurrentPrivateDeck");
            }
        }


        private static List<Role> roleGameRoles = new List<Role>() { Role.Loyalist, Role.Defector, Role.Rebel };

        ObservableCollection<Role> _possibleRoles;

        private void _UpdatePossibleRolesInternal()
        {
            _possibleRoles.Clear();
            _possibleRoles.Add(Role.Unknown);
            if (GameModel != null && _player != null)
            {
                if (GameModel.Game is RoleGame)
                {
                    if (_player.Role == Role.Unknown)
                    {
                        foreach (Role role in roleGameRoles)
                        {
                            _possibleRoles.Add(role);
                        }
                    }
                    else if (_player.Role == Role.Ruler)
                    {
                        _possibleRoles.Clear();
                        _possibleRoles.Add(_player.Role);
                    }
                    else
                    {
                        _possibleRoles.Add(_player.Role);
                    }
                }
                else
                {
                    // @todo: add other possibilities here.
                }
            }
        }

        private void _UpdatePossibleRoles()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _UpdatePossibleRolesInternal();
            }
            else
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)_UpdatePossibleRolesInternal);
            }
        }

        public ObservableCollection<Role> PossibleRoles
        {
            get
            {
                return _possibleRoles;
            }
        }

        private bool _isResponsing;

        public bool IsResponsing
        {
            get
            {
                return _isResponsing;
            }
            protected set
            {
                if (_isResponsing == value) return;
                _isResponsing = value;
                OnPropertyChanged("IsResponsing");
            }
        }

        #endregion

        #region Equipments and DelayedTools

        public EquipCommand WeaponCommand { get; set; }

        public EquipCommand ArmorCommand { get; set; }

        public EquipCommand DefensiveHorseCommand { get; set; }

        public EquipCommand OffensiveHorseCommand { get; set; }

        public IList<EquipCommand> EquipCommands
        {
            get
            {
                IList<EquipCommand> result = new List<EquipCommand>();
                if (WeaponCommand != null) result.Add(WeaponCommand);
                if (ArmorCommand != null) result.Add(ArmorCommand);
                if (DefensiveHorseCommand != null) result.Add(DefensiveHorseCommand);
                if (OffensiveHorseCommand != null) result.Add(OffensiveHorseCommand);
                return result;
            }
        }

        #endregion

        #region Commands

        private readonly SimpleRelayCommand DisabledCommand = new SimpleRelayCommand((o) => { })
        {
            CanExecuteStatus = false
        };

        #region SubmitCardUsageCommand

        private SimpleRelayCommand submitCardUsageCommand;
        public void SubmitCardUsageCommand(object parameter)
        {
            List<Card> cards = _GetSelectedNonEquipCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipSkill;
            SkillCommand skillCommand = _GetSelectedSkillCommand(out isEquipSkill);

            if (skillCommand != null)
            {
                skill = skillCommand.Skill;
            }

            // are we really able to use this equip as command?
            if (isEquipSkill)
            {
                Trace.Assert(skill != null);
                if (currentUsageVerifier.Verify(HostPlayer, skill, new List<Card>(), new List<Player>()) == VerifierResult.Fail)
                {
                    //nope, not really
                    isEquipSkill = false;
                    skill = null;
                }
            }

            foreach (var equipCommand in EquipCommands)
            {
                if (!isEquipSkill && equipCommand.IsSelected)
                {
                    cards.Add(equipCommand.Card);
                }
            }

            // Card usage question
            lock (verifierLock)
            {
                var guHuoSkill = skill as IAdditionalTypedSkill;
                if (guHuoSkill != null)
                {
                    // Reset all will also clear the GuHuoChoice recorded. So restore it after resetting the buttons.
                    var backup = guHuoSkill.AdditionalType;
                    _ResetAll();
                    guHuoSkill.AdditionalType = backup;
                }
                else
                {
                    _ResetAll();
                }
                if (currentUsageVerifier != null)
                {
                    currentUsageVerifier = null;
                    CardUsageAnsweredEvent(skill, cards, players);
                }
            }
        }

        #endregion

        #region CancelCardUsageCommand
        private SimpleRelayCommand cancelCardUsageCommand;
        public void CancelCardUsageCommand(object parameter)
        {
            lock (verifierLock)
            {
                Trace.Assert(currentUsageVerifier != null);
                CardUsageAnsweredEvent(null, null, null);
                currentUsageVerifier = null;
                _ResetAll();
            }
        }
        #endregion

        #region AbortCardUsageCommand
        private SimpleRelayCommand abortCardUsageCommand;
        public void AbortCardUsageCommand(object parameter)
        {
            lock (verifierLock)
            {
                if (currentUsageVerifier == null)
                {
                    return;
                }
                Trace.Assert(currentUsageVerifier != null);
                CardUsageAnsweredEvent(null, null, null);
                currentUsageVerifier = null;
                _ResetAll();
            }
        }
        #endregion

        #region MultiChoiceCommand

        public ObservableCollection<ICommand> MultiChoiceCommands
        {
            get;
            private set;
        }

        public void ExecuteMultiChoiceCommand(object parameter)
        {
            lock (verifierLock)
            {
                Trace.Assert(currentUsageVerifier == null);
                _ResetAll();
                IsMultiChoiceQuestionShown = false;
            }
            MultipleChoiceAnsweredEvent((int)parameter);
        }
        #endregion

        #region CardChoiceCommand
        private AdditionalCardChoiceOptions _currentChoiceOptions;
        public void ExecuteCardChoiceCommand(object parameter)
        {
            lock (verifierLock)
            {
                Trace.Assert(currentUsageVerifier == null);
                var model = CardChoiceModel;
                Trace.Assert(model != null);
                if (_currentChoiceOptions != null)
                {
                    _currentChoiceOptions.OptionResult = (int)parameter;
                }
                CardChoiceAnsweredEvent(model.Answer);
                CardChoiceModel.TimeOutSeconds = 0;
                IsCardChoiceQuestionShown = false;
            }
        }

        public void CancelCardChoiceCommand(object parameter)
        {
            lock (verifierLock)
            {
                Trace.Assert(currentUsageVerifier == null);
                var model = CardChoiceModel;
                Trace.Assert(model != null);
                CardChoiceAnsweredEvent(null);
                CardChoiceModel.TimeOutSeconds = 0;
                IsCardChoiceQuestionShown = false;
            }
        }
        #endregion

        #endregion

        #region Card Choice Questions
        private CardChoiceViewModel _cardChoiceModel;

        public CardChoiceViewModel CardChoiceModel
        {
            get { return _cardChoiceModel; }
            set
            {
                if (_cardChoiceModel == value) return;
                _cardChoiceModel = value;
                OnPropertyChanged("CardChoiceModel");
            }
        }


        #endregion

        #region View Related Fields

        private int _handCardCount;

        public int HandCardCount
        {
            get { return _handCardCount; }
            set
            {
                if (_handCardCount == value) return;
                _handCardCount = value;
                OnPropertyChanged("HandCardCount");
            }
        }

        private Account _account;

        public Account Account
        {
            get { return _account; }
            set
            {
                if (_account == value) return;
                _account = value;
                OnPropertyChanged("Account");
            }
        }

        private string _prompt;
        public string CurrentPromptString
        {
            get
            {
                return _prompt;
            }
            set
            {
                if (_prompt == value) return;
                _prompt = value;
                OnPropertyChanged("CurrentPromptString");
            }
        }

        #endregion

        #region IASyncUiProxy Helpers
        ICardUsageVerifier currentUsageVerifier;

        SkillCommand _GetSelectedSkillCommand(out bool isEquipSkill)
        {
            foreach (var skillCommand in ActiveSkillCommands)
            {
                if (skillCommand.IsSelected)
                {
                    isEquipSkill = false;
                    return skillCommand;
                }
            }
            foreach (EquipCommand equipCmd in EquipCommands)
            {
                if (equipCmd.IsSelected)
                {
                    if (equipCmd.SkillCommand.Skill != null && equipCmd.SkillCommand.Skill is CardTransformSkill)
                    {
                        isEquipSkill = true;
                        return equipCmd.SkillCommand;
                    }
                }
            }
            isEquipSkill = false;
            return null;
        }

        private List<Card> _GetSelectedNonEquipCards()
        {
            IEnumerable<CardViewModel> source = (CurrentPrivateDeck == null) ? HandCards : HandCards.Concat(CurrentPrivateDeck.Cards);
            var result = from c in source
                         where c.IsSelected
                         select c.Card;
            return new List<Card>(result);
        }

        private List<Player> _lastSelectedPlayers;

        private List<Player> _GetSelectedPlayers()
        {
            var currentSelection = new List<Player>();
            foreach (var playerModel in _game.PlayerModels)
            {
                if (playerModel.IsSelected)
                {
                    if (!playerModel.IsSelectionRepeatable)
                    {
                        currentSelection.Add(playerModel.Player);
                    }
                    else
                    {
                        for (int i = 0; i < playerModel.SelectedTimes; i++)
                        {
                            currentSelection.Add(playerModel.Player);
                        }
                    }
                }
            }

            var diff1 = new List<Player>(currentSelection);
            var diff2 = new List<Player>(_lastSelectedPlayers);

            foreach (var player in _lastSelectedPlayers)
            {
                diff1.Remove(player);
            }
            foreach (var player in currentSelection)
            {
                diff2.Remove(player);
            }

            _lastSelectedPlayers.AddRange(diff1);
            foreach (var player in diff2)
            {
                _lastSelectedPlayers.Remove(player);
            }
            Trace.Assert(_lastSelectedPlayers.Count == currentSelection.Count);
            return new List<Player>(_lastSelectedPlayers);
        }

        private void _ResetSkillsAndCards()
        {
            foreach (var equipCommand in EquipCommands)
            {
                equipCommand.OnSelectedChanged -= _updateCardUsageStatusHandler;
                equipCommand.IsSelectionMode = false;
            }

            foreach (var skillCommand in ActiveSkillCommands)
            {
                skillCommand.IsSelected = false;
                skillCommand.OnSelectedChanged -= _onSkillCommandSelectedHandler;
                skillCommand.IsEnabled = false;
            }

            foreach (CardViewModel card in HandCards)
            {
                card.OnSelectedChanged -= _updateCardUsageStatusHandler;
                card.IsSelectionMode = false;
            }

            foreach (var playerModel in _game.PlayerModels)
            {
                playerModel.OnSelectedChanged -= _updateCardUsageStatusHandler;
                playerModel.IsSelectionMode = false;
            }
            _lastSelectedPlayers.Clear();
            CurrentPrivateDeck = null;
            SubmitAnswerCommand = DisabledCommand;
            CancelAnswerCommand = DisabledCommand;
            AbortAnswerCommand = DisabledCommand;
        }

        private void _ResetAll()
        {
            MultiChoiceCommands.Clear();
            _ResetSkillsAndCards();
            CurrentPromptString = string.Empty;
            TimeOutSeconds = 0;
        }

        private EventHandler _onSkillCommandSelectedHandler;

        SkillCommand _lastSelectedCommand;
        bool _cleaningUp;
        private void _OnSkillCommandSelected(object sender, EventArgs args)
        {
            var skill = sender as SkillCommand;
            if (skill.IsSelected)
            {
                if (skill == _lastSelectedCommand)
                {
                    Trace.Assert(skill is GuHuoSkillCommand);
                }
                else if (_lastSelectedCommand != null)
                {
                    _cleaningUp = true;
                    _lastSelectedCommand.IsSelected = false;
                    _cleaningUp = false;
                    Trace.Assert(_lastSelectedCommand == null);
                }

                _lastSelectedCommand = skill;

            }
            else
            {
                if (skill != _lastSelectedCommand)
                {
                    Trace.Assert(skill is GuHuoSkillCommand);
                }
                else
                {
                    foreach (EquipCommand equipCmd in EquipCommands)
                    {
                        equipCmd.IsSelected = false;
                    }

                    foreach (CardViewModel card in HandCards)
                    {
                        card.IsSelected = false;
                    }

                    foreach (var playerModel in _game.PlayerModels)
                    {
                        playerModel.IsSelected = false;
                    }

                    _lastSelectedPlayers.Clear();
                    _lastSelectedCommand = null;
                }
            }

            if (!_cleaningUp && currentUsageVerifier != null)
            {
                _UpdateCardUsageStatus();
            }
        }

        private void _UpdateCardUsageStatus()
        {
            if (currentUsageVerifier == null)
            {
                return;
            }

            List<Card> cards = _GetSelectedNonEquipCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipCommand;
            SkillCommand command = _GetSelectedSkillCommand(out isEquipCommand);

            lock (verifierLock)
            {
                if (currentUsageVerifier == null)
                {
                    return;
                }

                if (currentUsageVerifier.Helper.IsActionStage)
                {
                    cancelCardUsageCommand.CanExecuteStatus = (cards.Count != 0 || players.Count != 0 || command != null);
                }

                if (command != null)
                {
                    skill = command.Skill;
                }

                // are we really able to use this equip as command?
                if (isEquipCommand)
                {
                    Trace.Assert(skill != null);
                    if (currentUsageVerifier.Verify(HostPlayer, skill, new List<Card>(), new List<Player>()) == VerifierResult.Fail)
                    {
                        //nope, not really
                        isEquipCommand = false;
                        skill = null;
                    }
                }

                string prompt = null;
                if (skill != null)
                {
                    prompt = Application.Current.TryFindResource(string.Format("Skill.{0}.Prompt", skill.GetType().Name)) as string;
                }
                if (prompt == null)
                {
                    prompt = PromptFormatter.Format(CurrentPrompt);
                }
                CurrentPromptString = prompt;

                if (!isEquipCommand)
                {
                    foreach (var equipCommand in EquipCommands)
                    {
                        if (equipCommand.IsSelected)
                            cards.Add(equipCommand.Card);
                    }
                }

                var sc = new List<SkillCommand>(ActiveSkillCommands);

                // Handle skill down            
                foreach (var skillCommand in sc)
                {
                    // Handle kurou, luanwu and yeyan
                    if (skillCommand.Skill != null && skillCommand.IsSelected)
                    {
                        var helper = skillCommand.Skill.Helper;

                        // Handle KuRou, LuanWu
                        if (helper.HasNoConfirmation)
                        {
                            SubmitAnswerCommand.Execute(null);
                            return;
                        }

                        // Handle YeYan
                        foreach (var player in _game.PlayerModels)
                        {
                            if (player.IsSelectionRepeatable == helper.IsPlayerRepeatable)
                            {
                                break;
                            }
                            player.IsSelectionRepeatable = helper.IsPlayerRepeatable;
                        }

                        // Handle JiXi, PaiYi
                        if (helper.OtherDecksUsed.Count > 0)
                        {
                            if (helper.OtherDecksUsed.Count != 1)
                            {
                                throw new NotImplementedException("Currently using more than one private decks is not supported");
                            }
                            var deck = helper.OtherDecksUsed[0];
                            var deckModel = PrivateDecks.FirstOrDefault(d => d.Name == deck.Name);
                            Trace.Assert(deckModel != null);
                            if (deckModel != CurrentPrivateDeck)
                            {
                                if (CurrentPrivateDeck != null)
                                {
                                    foreach (var card in CurrentPrivateDeck.Cards)
                                    {
                                        card.IsSelectionMode = false;
                                        card.OnSelectedChanged -= _updateCardUsageStatusHandler;
                                    }
                                }
                                foreach (var card in deckModel.Cards)
                                {
                                    card.IsSelectionMode = true;
                                    card.OnSelectedChanged += _updateCardUsageStatusHandler;
                                }
                                CurrentPrivateDeck = deckModel;
                            }
                        }
                        else
                        {
                            CurrentPrivateDeck = null;
                        }
                    }
                    else
                    {
                        skillCommand.IsEnabled = (currentUsageVerifier.Verify(HostPlayer, skillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
                    }

                    // Handler GuHuo, QiCe
                    GuHuoSkillCommand cmdGuhuo = skillCommand as GuHuoSkillCommand;
                    if (cmdGuhuo != null)
                    {
                        if (skillCommand.IsEnabled)
                        {
                            if (cmdGuhuo.GuHuoTypes.Count == 0 && cmdGuhuo.GuHuoChoice == null)
                            {
                                var trySkill = Activator.CreateInstance(cmdGuhuo.Skill.GetType()) as IAdditionalTypedSkill;
                                trySkill.Owner = cmdGuhuo.Skill.Owner;
                                foreach (var c in Game.CurrentGame.AvailableCards)
                                {
                                    trySkill.AdditionalType = c;
                                    if (currentUsageVerifier.Verify(HostPlayer, trySkill, new List<Card>(), new List<Player>()) != VerifierResult.Fail)
                                    {
                                        cmdGuhuo.GuHuoTypes.Add(c);
                                    }
                                }
                            }
                        }
                    }
                }

                var status = currentUsageVerifier.Verify(HostPlayer, skill, cards, players);

                if (status == VerifierResult.Fail)
                {
                    submitCardUsageCommand.CanExecuteStatus = false;
                    foreach (var playerModel in _game.PlayerModels)
                    {
                        playerModel.IsSelected = false;
                    }
                    players.Clear();
                    _lastSelectedPlayers.Clear();
                }

                else if (status == VerifierResult.Partial)
                {
                    submitCardUsageCommand.CanExecuteStatus = false;
                }
                else if (status == VerifierResult.Success)
                {
                    submitCardUsageCommand.CanExecuteStatus = true;
                }

                if (skill == null || (skill is CardTransformSkill) || (skill is ActiveSkill))
                {
                    List<Card> attempt = new List<Card>(cards);

                    var cardsToTry = CurrentPrivateDeck == null ? HandCards : HandCards.Concat(CurrentPrivateDeck.Cards);

                    foreach (var card in cardsToTry)
                    {
                        if (card.IsSelected)
                        {
                            continue;
                        }
                        attempt.Add(card.Card);
                        bool disabled = (currentUsageVerifier.Verify(HostPlayer, skill, attempt, players) == VerifierResult.Fail);
                        card.IsEnabled = !disabled;
                        attempt.Remove(card.Card);
                    }

                    foreach (var equipCommand in EquipCommands)
                    {
                        bool enabledAsSkill = false;
                        if (skill == null && equipCommand.SkillCommand.Skill != null && (equipCommand.SkillCommand.Skill is CardTransformSkill || equipCommand.SkillCommand.Skill is ActiveSkill))
                        {
                            enabledAsSkill = (currentUsageVerifier.Verify(HostPlayer, equipCommand.SkillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
                        }
                        if (!equipCommand.IsSelected)
                        {
                            attempt.Add(equipCommand.Card);
                            bool disabled = (currentUsageVerifier.Verify(HostPlayer, skill, attempt, players) == VerifierResult.Fail);
                            equipCommand.IsEnabled = (!disabled) | enabledAsSkill;
                        }
                        attempt.Remove(equipCommand.Card);
                    }
                }

                // Invalidate target selection
                List<Player> attempt2 = new List<Player>(players);
                int validCount = 0;
                bool[] enabledMap = new bool[_game.PlayerModels.Count];
                int i = 0;
                foreach (var playerModel in _game.PlayerModels)
                {
                    enabledMap[i] = false;
                    if (playerModel.IsSelected && !playerModel.IsSelectionRepeatable)
                    {
                        i++;
                        continue;
                    }
                    attempt2.Add(playerModel.Player);
                    bool disabled = (currentUsageVerifier.Verify(HostPlayer, skill, cards, attempt2) == VerifierResult.Fail);
                    if (!disabled)
                    {
                        validCount++;
                        enabledMap[i] = true;
                    }
                    attempt2.RemoveAt(attempt2.Count - 1);
                    i++;

                }
                i = 0;

                bool allowSelection = (cards.Count != 0 || validCount != 0 || skill != null);
                foreach (var playerModel in _game.PlayerModels)
                {
                    if (playerModel.IsSelected && !playerModel.IsSelectionRepeatable)
                    {
                        i++;
                        continue;
                    }

                    playerModel.IsSelectionMode = allowSelection;
                    if (allowSelection)
                    {
                        if (playerModel.IsSelected)
                        {
                            playerModel.CanBeSelectedMore = enabledMap[i];
                        }
                        else
                        {
                            playerModel.IsEnabled = enabledMap[i];
                        }
                    }
                    i++;
                }
            }
        }

        private void _AbortCardChoice()
        {
            lock (verifierLock)
            {
                CardChoiceModel.TimeOutSeconds = 0;
                IsCardChoiceQuestionShown = false;
                CurrentPrompt = null;
                CurrentPromptString = string.Empty;
            }
        }

        private void _AbortMultipleChoice()
        {
            lock (verifierLock)
            {
                SubmitAnswerCommand = DisabledCommand;
                CancelAnswerCommand = DisabledCommand;
                AbortAnswerCommand = DisabledCommand;
                MultiChoiceCommands.Clear();
                CurrentPromptString = string.Empty;
                CurrentPrompt = null;
                _currentMultiChoices = null;
            }
        }

        private EventHandler _updateCardUsageStatusHandler;


        #endregion

        #region IAsyncUiProxy
        public Player HostPlayer
        {
            get
            {
                return Player;
            }
            set
            {
                Player = value;
            }
        }

        public void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameModel.CurrentActivePlayer = this;
                lock (verifierLock)
                {
                    TimeOutSeconds = timeOutSeconds;
                    currentUsageVerifier = verifier;
                    Trace.Assert(currentUsageVerifier != null);
                    CurrentPrompt = prompt;
                    CurrentPromptString = PromptFormatter.Format(prompt);
                }

                if (prompt.Values.Count != 0 && prompt.Values[0] is TriggerSkill)
                {
                    var triggerSkill = (prompt.Values[0] as TriggerSkill);
                    foreach (var skill in SkillCommands)
                    {
                        if (skill.Skill is CardTransformSkill && (skill.Skill as CardTransformSkill).LinkedPassiveSkill != triggerSkill) continue;
                        if (skill.Skill is ActiveSkill && (skill.Skill as ActiveSkill).LinkedPassiveSkill != triggerSkill) continue;
                        if (skill.Skill is TriggerSkill && skill.Skill != triggerSkill) continue;
                        skill.IsHighlighted = true;
                        break;
                    }
                }

                if (verifier != null && verifier.Helper != null &&
                    verifier.Helper.OtherDecksUsed != null && verifier.Helper.OtherDecksUsed.Count != 0)
                {
                    var helper = verifier.Helper;
                    if (helper.OtherDecksUsed.Count > 0)
                    {
                        if (helper.OtherDecksUsed.Count != 1)
                        {
                            throw new NotImplementedException("Currently using more than one private decks is not supported");
                        }
                        var deck = helper.OtherDecksUsed[0];
                        var deckModel = PrivateDecks.FirstOrDefault(d => d.Name == deck.Name);
                        Trace.Assert(deckModel != null);
                        if (deckModel != CurrentPrivateDeck)
                        {
                            if (CurrentPrivateDeck != null)
                            {
                                foreach (var card in CurrentPrivateDeck.Cards)
                                {
                                    card.IsSelectionMode = false;
                                    card.OnSelectedChanged -= _updateCardUsageStatusHandler;
                                }
                            }
                            foreach (var card in deckModel.Cards)
                            {
                                card.IsSelectionMode = IsPlayable;
                                card.OnSelectedChanged += _updateCardUsageStatusHandler;
                            }
                            CurrentPrivateDeck = deckModel;
                        }
                    }
                }

                if (!IsPlayable)
                {
                    TimeOutSeconds = timeOutSeconds;
                    CardUsageAnsweredEvent(null, null, null);
                    return;
                }

                foreach (var equipCommand in EquipCommands)
                {
                    equipCommand.OnSelectedChanged += _updateCardUsageStatusHandler;
                    equipCommand.IsSelectionMode = true;
                }

                foreach (var card in HandCards)
                {
                    card.IsSelectionMode = true;
                    card.OnSelectedChanged += _updateCardUsageStatusHandler;
                }

                foreach (var playerModel in _game.PlayerModels)
                {
                    playerModel.IsSelectionMode = true;
                    playerModel.IsSelectionRepeatable = verifier.Helper.IsPlayerRepeatable;
                    playerModel.OnSelectedChanged += _updateCardUsageStatusHandler;
                }

                foreach (var skillCommand in ActiveSkillCommands)
                {
                    if (skillCommand is GuHuoSkillCommand)
                    {
                        (skillCommand as GuHuoSkillCommand).GuHuoChoice = null;
                    }
                    skillCommand.OnSelectedChanged += _onSkillCommandSelectedHandler;
                }

                // @todo: update this.
                SubmitAnswerCommand = submitCardUsageCommand;
                CancelAnswerCommand = cancelCardUsageCommand;
                AbortAnswerCommand = abortCardUsageCommand;
                abortCardUsageCommand.CanExecuteStatus = currentUsageVerifier.Helper.IsActionStage;
                cancelCardUsageCommand.CanExecuteStatus = !currentUsageVerifier.Helper.IsActionStage;

                _UpdateCardUsageStatus();
            });
        }

        private void _ConstructCardChoiceModel(List<DeckPlace> sourceDecks, List<string> resultDeckNames,
                                               List<int> resultDeckMaximums,
                                               AdditionalCardChoiceOptions options,
                                               ICardChoiceVerifier verifier,
                                               int timeOutSeconds,
                                               CardChoiceRearrangeCallback callback)
        {
            bool isSingleResult = (resultDeckMaximums.Sum() == 1);

            var choiceModel = new CardChoiceViewModel();

            int numLines = sourceDecks.Count;

            foreach (var deck in sourceDecks)
            {
                if (Game.CurrentGame.Decks[deck].Count == 0)
                {
                    continue;
                }
                CardChoiceLineViewModel line = new CardChoiceLineViewModel();
                line.DeckName = deck.DeckType.Name;
                line.IsResultDeck = false;
                int i = 0;
                int numCards = Game.CurrentGame.Decks[deck].Count;
                int maxColumns = Math.Max((numCards + 1) / 2, 5);
                bool firstRow = true;
                foreach (var card in Game.CurrentGame.Decks[deck])
                {
                    if (numLines == 1 && isSingleResult && i >= maxColumns && firstRow)
                    {
                        Trace.Assert(choiceModel.CardStacks.Count == 0);
                        choiceModel.CardStacks.Add(line);
                        line = new CardChoiceLineViewModel();
                        line.DeckName = deck.DeckType.Name;
                        line.IsResultDeck = false;
                        firstRow = false;
                    }
                    CardViewModel model = new CardViewModel()
                    {
                        Card = card,
                        IsSelectionMode = isSingleResult,
                        IsEnabled = true
                    };

                    line.Cards.Add(model);
                    i++;
                }
                choiceModel.CardStacks.Add(line);
            }

            if (!isSingleResult)
            {
                int k = 0;
                foreach (var deckName in resultDeckNames)
                {
                    CardChoiceLineViewModel line = new CardChoiceLineViewModel();
                    line.DeckName = deckName;
                    if (options != null)
                    {
                        if (options.Rearrangeable != null)
                        {
                            line.Rearrangable = options.Rearrangeable[k];
                        }
                        if (options.DefaultResult != null)
                        {
                            foreach (var card in options.DefaultResult[k])
                            {
                                line.Cards.Add(new CardViewModel() { Card = card });
                            }
                        }
                    }
                    line.Capacity = resultDeckMaximums[k++];
                    line.IsResultDeck = true;
                    choiceModel.CardStacks.Add(line);
                }
            }

            if (options != null && options.Options != null)
            {
                for (int i = 0; i < options.Options.Count; i++)
                {
                    MultiChoiceCommand command = new MultiChoiceCommand(ExecuteCardChoiceCommand)
                    {
                        CanExecuteStatus = false,
                        ChoiceKey = options.Options[i],
                        ChoiceIndex = i
                    };
                    choiceModel.MultiChoiceCommands.Add(command);
                }
            }
            else
            {
                MultiChoiceCommand command = new MultiChoiceCommand(ExecuteCardChoiceCommand)
                {
                    CanExecuteStatus = false,
                    ChoiceKey = new OptionPrompt("Confirm")
                };
                choiceModel.MultiChoiceCommands.Add(command);
            }
            if (options != null && options.IsCancellable)
            {
                MultiChoiceCommand command = new MultiChoiceCommand(CancelCardChoiceCommand)
                {
                    IsCancel = true,
                    ChoiceKey = new OptionPrompt("Cancel")
                };
                choiceModel.MultiChoiceCommands.Add(command);
            }

            choiceModel.Verifier = verifier;
            choiceModel.TimeOutSeconds = timeOutSeconds;
            CardChoiceModel = choiceModel;
        }

        private bool cardChoiceQuestionShown;

        public bool IsCardChoiceQuestionShown
        {
            get
            {
                return cardChoiceQuestionShown;
            }
            set
            {
                if (cardChoiceQuestionShown == value) return;
                cardChoiceQuestionShown = value;
                OnPropertyChanged("IsCardChoiceQuestionShown");
            }
        }

        private bool multiChoiceQuestionShown;

        public bool IsMultiChoiceQuestionShown
        {
            get
            {
                return multiChoiceQuestionShown;
            }
            set
            {
                if (multiChoiceQuestionShown == value) return;
                multiChoiceQuestionShown = value;
                OnPropertyChanged("IsMultiChoiceQuestionShown");
            }
        }

        public CardChoiceRearrangeCallback CurrentCardChoiceRearrangeCallback
        {
            get;
            private set;
        }

        public void AnswerWuGuChoice(Card card)
        {
            if (GameModel.WuGuModel.IsEnabled)
            {
                CardChoiceAnsweredEvent(new List<List<Card>>() { new List<Card>() { card } });
            }
            GameModel.WuGuModel.IsEnabled = false;
        }

        public void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks,
                                     List<string> resultDeckNames,
                                     List<int> resultDeckMaximums,
                                     ICardChoiceVerifier verifier,
                                     int timeOutSeconds,
                                     AdditionalCardChoiceOptions options,
                                     CardChoiceRearrangeCallback callback)
        {
            if (ViewModelBase.IsDetached) return;
            if (this != GameModel.MainPlayerModel && (verifier.Helper == null || !verifier.Helper.ShowToAll))
            {
                TimeOutSeconds = timeOutSeconds;
                CardChoiceAnsweredEvent(null);
                return;
            }
            Trace.Assert(resultDeckMaximums.Count == resultDeckNames.Count);

            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                lock (verifierLock)
                {
                    GameModel.CurrentActivePlayer = this;
                    if (!IsPlayable)
                    {
                        Trace.Assert(currentUsageVerifier == null);
                        TimeOutSeconds = timeOutSeconds;
                        CardChoiceAnsweredEvent(null);
                        prompt.ResourceKey = prompt.ResourceKey + Prompt.NonPlaybleAppendix;
                        prompt.Values.Insert(0, Player);
                    }
                    if (options != null && options.IsWuGu)
                    {
                        Trace.Assert(GameModel.WuGuModel != null);
                        TimeOutSeconds = timeOutSeconds;
                        GameModel.WuGuModel.IsEnabled = IsPlayable;
                        GameModel.WuGuModel.Prompt = PromptFormatter.Format(prompt);
                    }
                    else
                    {
                        _currentChoiceOptions = options;
                        _ConstructCardChoiceModel(sourceDecks, resultDeckNames, resultDeckMaximums, options, verifier, timeOutSeconds, callback);
                        CardChoiceModel.Prompt = PromptFormatter.Format(prompt);
                        if (!IsPlayable)
                        {
                            CardChoiceModel.DisplayOnly = true;
                            prompt.Values.Insert(0, Player);
                            CurrentCardChoiceRearrangeCallback = null;
                        }
                        else
                        {
                            CurrentCardChoiceRearrangeCallback = callback;
                        }
                        IsCardChoiceQuestionShown = true;
                    }
                }
            });
        }

        public Prompt CurrentPrompt
        {
            get;
            set;
        }

        private List<OptionPrompt> _currentMultiChoices;

        public void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> choices, int timeOutSeconds)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameModel.CurrentActivePlayer = this;
                CurrentPrompt = prompt;
                CurrentPromptString = PromptFormatter.Format(prompt);
                _currentMultiChoices = choices;

                if (!IsPlayable)
                {
                    Trace.Assert(currentUsageVerifier == null);
                    TimeOutSeconds = timeOutSeconds;
                    MultipleChoiceAnsweredEvent(0);
                    return;
                }

                if (prompt.ResourceKey == Prompt.MultipleChoicePromptPrefix + Prompt.SkillUseYewNoPrompt)
                {
                    Trace.Assert(prompt.Values.Count != 0);
                    var targetSkill = prompt.Values[0] as TriggerSkill;
                    Trace.Assert(targetSkill != null);
                    foreach (var skill in SkillCommands)
                    {
                        if (skill.Skill == targetSkill)
                        {
                            skill.IsHighlighted = true;
                            if (skill.IsSelected)
                            {
                                int answer = choices.IndexOf(Prompt.YesChoice);
                                Trace.Assert(answer != -1);
                                MultipleChoiceAnsweredEvent(answer);
                                return;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < choices.Count; i++)
                {
                    MultiChoiceCommand command = new MultiChoiceCommand(ExecuteMultiChoiceCommand)
                    {
                        CanExecuteStatus = true,
                        ChoiceKey = choices[i],
                        ChoiceIndex = i
                    };
                    if (choices[i] == Prompt.YesChoice)
                    {
                        SubmitAnswerCommand = command;
                    }
                    else if (choices[i] == Prompt.NoChoice)
                    {
                        CancelAnswerCommand = command;
                    }
                    else
                    {
                        MultiChoiceCommands.Add(command);
                    }
                }
                lock (verifierLock)
                {
                    IsMultiChoiceQuestionShown = true;
                    TimeOutSeconds = timeOutSeconds;
                }
            });
        }

        void triggerSkill_OnSelectedChanged(object sender, EventArgs e)
        {
            lock (verifierLock)
            {
                var command = (sender as SkillCommand);
                if (command.IsAutoInvokeSkill &&
                    command.IsSelected &&
                    IsMultiChoiceQuestionShown &&
                    CurrentPrompt.ResourceKey == Prompt.MultipleChoicePromptPrefix + Prompt.SkillUseYewNoPrompt &&
                    CurrentPrompt.Values[0] == command.Skill)
                {
                    int answer = _currentMultiChoices.IndexOf(Prompt.YesChoice);
                    Trace.Assert(answer != -1);
                    MultipleChoiceAnsweredEvent(answer);
                }
            }
        }

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;

        public void Freeze()
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                lock (verifierLock)
                {
                    _ResetAll();

                    foreach (var skillCommand in SkillCommands)
                    {
                        if (skillCommand is GuHuoSkillCommand)
                        {
                            (skillCommand as GuHuoSkillCommand).GuHuoTypes.Clear();
                        }
                        skillCommand.IsHighlighted = false;
                    }

                    if (currentUsageVerifier != null)
                    {
                        currentUsageVerifier = null;
                    }
                    else if (IsCardChoiceQuestionShown)
                    {
                        _AbortCardChoice();
                    }
                    else if (IsMultiChoiceQuestionShown)
                    {
                        _AbortMultipleChoice();
                    }
                }
            });
        }
        #endregion

        #region Private Members
        private object verifierLock;
        #endregion

        #region Cheats
        public bool CheatGetCard(Card card)
        {
            lock (verifierLock)
            {
                if (currentUsageVerifier == null)
                {
                    return false;
                }
                _ResetAll();
                if (currentUsageVerifier != null)
                {
                    currentUsageVerifier = null;
                    CardUsageAnsweredEvent(new CheatSkill() { CheatType = CheatType.Card, CardId = card.Id }, new List<Card>(), new List<Player>());
                }
            }
            return true;
        }

        public bool CheatGetSkill(string skillName)
        {
            lock (verifierLock)
            {
                if (currentUsageVerifier == null)
                {
                    return false;
                }
                _ResetAll();
                if (currentUsageVerifier != null)
                {
                    currentUsageVerifier = null;
                    CardUsageAnsweredEvent(new CheatSkill() { CheatType = CheatType.Skill, SkillName = skillName }, new List<Card>(), new List<Player>());
                }
            }
            return true;
        }
        #endregion

        internal void AnswerEmptyMultichoiceQuestion()
        {
            if (IsPlayable)
            {
                ExecuteMultiChoiceCommand(0);
            }
        }

        internal HeroViewModel GetHeroModel(Hero hero)
        {
            if (Hero == hero) return Hero1Model;
            else if (Hero2 == hero) return Hero2Model;
            else return null;
        }
    }
}
