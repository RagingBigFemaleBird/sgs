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

namespace Sanguosha.UI.Controls
{
    public class NumRolesToComboBoxEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Role> roles = value as List<Role>;
            return (roles != null) && (roles.Count > 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayerViewModel : SelectableItem, IAsyncUiProxy
    {
        #region Constructors
        public PlayerViewModel()
        {
            IsSelectionMode = false;
            SkillCommands = new ObservableCollection<SkillCommand>();
            HeroSkillNames = new ObservableCollection<string>();
            heroNameChars = new ObservableCollection<string>();
            MultiChoiceCommands = new ObservableCollection<ICommand>();

            submitCardUsageCommand = new SimpleRelayCommand(SubmitCardUsageCommand);
            cancelCardUsageCommand = new SimpleRelayCommand(CancelCardUsageCommand);
            abortCardUsageCommand = new SimpleRelayCommand(AbortCardUsageCommand);

            SubmitAnswerCommand = DisabledCommand;
            CancelAnswerCommand = DisabledCommand;
            AbortAnswerCommand = DisabledCommand;

            _possibleRoles = new ObservableCollection<Role>();
            _UpdateCardUsageStatusHandler = (o, e) => { _UpdateCardUsageStatus(); };
            IsCardChoiceQuestionShown = false;

            CardChoiceModel = new CardChoiceViewModel();
            HandCards = new ObservableCollection<CardViewModel>();
            verifierLock = new object();
            _lastSelectedPlayers = new List<Player>();
        }

        public PlayerViewModel(Player player, GameViewModel game, bool isPlayable) : this()
        {
            Player = player;
            GameModel = game;
            IsPlayable = isPlayable;
        }
        #endregion

        #region Fields
        Player _player;

        public bool IsPlayable
        {
            get;
            set;
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
                    PropertyChangedEventHandler handler = _PropertyChanged;
                    _player.PropertyChanged -= handler;
                }
                _player = value;
                _PropertyChanged = _OnPlayerPropertyChanged;                
                _player.PropertyChanged += _PropertyChanged;
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
            else if (name == "CurrentPhase" && IsCurrentPlayer)
            {                
                OnPropertyChanged("CurrentPhase");
            }
        }

        private PropertyChangedEventHandler _PropertyChanged;

        private void _UpdateSkills()
        {
            SkillCommands.Clear();
            foreach (ISkill skill in _player.Skills)
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
                SkillCommands.Add(command);
            }            
        }

        private void _UpdateHeroInfo()
        {
            HeroSkillNames.Clear();
            heroNameChars.Clear();

            if (Hero != null)
            {
                string name = Application.Current.TryFindResource(string.Format("Hero.{0}.Name", Hero.Name)) as string;
                if (name != null)
                {
                    foreach (var heroChar in name)
                    {
                        heroNameChars.Add(heroChar.ToString());
                    }
                }
                foreach (var skill in Hero.Skills)
                {
                    HeroSkillNames.Add(skill.GetType().Name);
                }
            } 
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
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateHeroInfo();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)_UpdateHeroInfo);
                    OnPropertyChanged("Hero");
                    OnPropertyChanged("HeroName");                    
                }
            }
            else if (name == "Skills")
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateSkills();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate(){ _UpdateSkills(); });
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
                return _player.Hero;
            }
        }

        public Hero Hero2
        {
            get
            {
                return _player.Hero2;
            }
        }

        public int Id
        {
            get
            {
                return _player.Id;
            }
        }

        public bool IsFemale
        {
            get
            {
                return _player.IsFemale;
            }
        }

        public bool IsMale
        {
            get
            {
                return _player.IsMale;
            }
        }

        public bool IsIronShackled { get { return _player.IsIronShackled; } }

        public bool IsImprisoned { get { return _player.IsImprisoned; } }

        public bool IsTargeted { get { return _player.IsTargeted; } }

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
                return _player.IsDead;
            }
        }

        #endregion
        
        #region Derived Player Properties

        public ObservableCollection<CardViewModel> HandCards
        {
            get;
            set;
        }


        public ObservableCollection<SkillCommand> SkillCommands
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the skill names of the primary hero.
        /// </summary>
        /// <remarks>For displaying tooltip purposes.</remarks>
        public ObservableCollection<string> HeroSkillNames
        {
            get;
            private set;
        }

        public string HeroName
        {
            get
            {
                if (_player == null || _player.Hero == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (_player.Hero.Name);
                }
            }
        }

        ObservableCollection<string> heroNameChars;

        public ObservableCollection<string> HeroNameChars
        {
            get
            {
                return heroNameChars;
            }
        }

        private static List<Role> roleGameRoles = new List<Role>() { Role.Loyalist, Role.Defector, Role.Rebel };

        ObservableCollection<Role> _possibleRoles;

        private void _UpdatePossibleRolesInternal()
        {
            _possibleRoles.Clear();
            _possibleRoles.Add(Role.Unknown);
            if (GameModel != null)
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
            List<Card> cards = _GetSelectedHandCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipSkill;
            SkillCommand skillCommand = _GetSelectedSkillCommand(out isEquipSkill);
            
            foreach (var equipCommand in EquipCommands)
            {
                if (!isEquipSkill && equipCommand.IsSelected)
                {
                    cards.Add(equipCommand.Card);
                }
            }

            if (skillCommand != null)
            {
                skill = skillCommand.Skill;
            }


            // Card usage question
            lock (verifierLock)
            {
                _ResetAll();
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
        
        #endregion

        #region Card Choice Questions
        private CardChoiceViewModel _cardChoiceModel;

        public CardChoiceViewModel CardChoiceModel
        {
            get { return _cardChoiceModel; }
            set { _cardChoiceModel = value; }
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

        private string _prompt;
        public string CurrentPrompt
        {
            get
            {
                return _prompt;
            }
            set
            {
                if (_prompt == value) return;
                _prompt = value;
                OnPropertyChanged("CurrentPrompt");
            }
        }

        #endregion

        #region IASyncUiProxy Helpers
        ICardUsageVerifier currentUsageVerifier;
        SkillCommand _GetSelectedSkillCommand(out bool isEquipSkill)
        {
            foreach (var skillCommand in SkillCommands)
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

        private List<Card> _GetSelectedHandCards()
        {
            List<Card> cards = new List<Card>();
            foreach (var card in HandCards)
            {
                if (card.IsSelected)
                {
                    Trace.Assert(card.Card != null);
                    cards.Add(card.Card);
                }
            }
            return cards;
        }

        private List<Player> _lastSelectedPlayers;

        private List<Player> _GetSelectedPlayers()
        {
            foreach (var playerModel in _game.PlayerModels)
            {
                if (playerModel.IsSelected && !_lastSelectedPlayers.Contains(playerModel.Player))
                {
                    _lastSelectedPlayers.Add(playerModel.Player);
                }
                else if (!playerModel.IsSelected && _lastSelectedPlayers.Contains(playerModel.Player))
                {
                    _lastSelectedPlayers.Remove(playerModel.Player);
                }
            }
            return new List<Player>(_lastSelectedPlayers);
        }

        private void _ResetSkillsAndCards()
        {
            foreach (var equipCommand in EquipCommands)
            {
                equipCommand.OnSelectedChanged -= _UpdateCardUsageStatusHandler;
                equipCommand.IsSelectionMode = false;
            }

            foreach (var skillCommand in SkillCommands)
            {
                skillCommand.IsSelected = false;
                skillCommand.IsEnabled = false;
            }

            foreach (CardViewModel card in HandCards)
            {
                card.OnSelectedChanged -= _UpdateCardUsageStatusHandler;
                card.IsSelectionMode = false;
            }

            foreach (var playerModel in _game.PlayerModels)
            {
                playerModel.OnSelectedChanged -= _UpdateCardUsageStatusHandler;
                playerModel.IsSelectionMode = false;
            }
            _lastSelectedPlayers.Clear();
            SubmitAnswerCommand = DisabledCommand;
            CancelAnswerCommand = DisabledCommand;
            AbortAnswerCommand = DisabledCommand; 
        }

        private void _ResetAll()
        {
            MultiChoiceCommands.Clear();
            _ResetSkillsAndCards();     
            CurrentPrompt = string.Empty;
            TimeOutSeconds = 0;
        }

        private void _UpdateCardUsageStatus()
        {
            List<Card> cards = _GetSelectedHandCards();
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

                var sc = new List<SkillCommand>(SkillCommands);
                
                // Handle skill down            
                foreach (var skillCommand in sc)
                {
                    // Handle kurou and luanwu
                    if (skillCommand.Skill != null && skillCommand.IsSelected)
                    {
                        var activeSkill = skillCommand.Skill as ActiveSkill;
                        if (activeSkill != null && activeSkill.UiHelper.HasNoConfirmation)
                        {
                            SubmitAnswerCommand.Execute(null);
                            return;
                        }
                    }
                
                    skillCommand.IsEnabled = (currentUsageVerifier.Verify(HostPlayer, skillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);

                    GuHuoSkillCommand cmdGuhuo = skillCommand as GuHuoSkillCommand;
                    if (cmdGuhuo != null)
                    {
                        if (skillCommand.IsEnabled)
                        {
                            if (cmdGuhuo.GuHuoTypes.Count == 0 && cmdGuhuo.GuHuoChoice == null)
                            {
                                var trySkill = Activator.CreateInstance(cmdGuhuo.Skill.GetType()) as IAdditionalTypedSkill;

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
                        else if (skillCommand.IsSelected && !skillCommand.IsSelected)
                        {
                            cmdGuhuo.GuHuoChoice = null;
                        }
                    }
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

                if (skill == null)
                {
                    foreach (var equipCommand in EquipCommands)
                    {
                        if (equipCommand.SkillCommand.Skill != null && equipCommand.SkillCommand.Skill is CardTransformSkill)
                        {
                            equipCommand.IsEnabled = (currentUsageVerifier.Verify(HostPlayer, equipCommand.SkillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
                        }
                        else
                        {
                            equipCommand.IsEnabled = false;
                        }
                    }
                }
                if (!isEquipCommand)
                {
                    foreach (var equipCommand in EquipCommands)
                    {
                        if (equipCommand.IsSelected)
                            cards.Add(equipCommand.Card);
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

                    foreach (var card in HandCards)
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
                        if (equipCommand.IsSelected) continue;

                        attempt.Add(equipCommand.Card);
                        bool disabled = (currentUsageVerifier.Verify(HostPlayer, skill, attempt, players) == VerifierResult.Fail);
                        //we do not have a skill and yet the equip is enabled!
                        //therefore it must be due to it being a valid command.
                        // kindly override in this case (this test does not count)
                        // otherwise we take this result as the result
                        if (!(skill == null && equipCommand.IsEnabled && equipCommand.SkillCommand.Skill != null))
                        {
                            equipCommand.IsEnabled = !disabled;
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
                    if (playerModel.IsSelected)
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
                    attempt2.Remove(playerModel.Player);
                    i++;

                }
                i = 0;

                bool allowSelection = (cards.Count != 0 || validCount != 0 || skill != null);
                foreach (var playerModel in _game.PlayerModels)
                {
                    if (playerModel.IsSelected)
                    {
                        i++;
                        continue;
                    }
                    playerModel.IsSelectionMode = allowSelection;
                    if (allowSelection)
                    {
                        playerModel.IsEnabled = enabledMap[i];
                    }
                    i++;
                }
            }
        }

        private void _AbortCardChoice()
        {
            lock(verifierLock)
            {
                CardChoiceModel.TimeOutSeconds = 0;
                IsCardChoiceQuestionShown = false;
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
                CurrentPrompt = string.Empty;
            }
        }

        private EventHandler _UpdateCardUsageStatusHandler;
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
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (!IsPlayable)
                {
                    Trace.Assert(currentUsageVerifier == null);
                    TimeOutSeconds = timeOutSeconds;
                    CardUsageAnsweredEvent(null, null, null);
                    return;
                }

                lock (verifierLock)
                {
                    TimeOutSeconds = timeOutSeconds;
                    currentUsageVerifier = verifier;                    
                    Trace.Assert(currentUsageVerifier != null);
                    Game.CurrentGame.CurrentActingPlayer = HostPlayer;
                    CurrentPrompt = PromptFormatter.Format(prompt);
                }
                foreach (var equipCommand in EquipCommands)
                {
                    equipCommand.OnSelectedChanged += _UpdateCardUsageStatusHandler;
                    equipCommand.IsSelectionMode = true;
                }

                foreach (var card in HandCards)
                {
                    card.IsSelectionMode = true;
                    card.OnSelectedChanged += _UpdateCardUsageStatusHandler;
                }

                foreach (var playerModel in _game.PlayerModels)
                {
                    playerModel.IsSelectionMode = true;
                    playerModel.OnSelectedChanged += _UpdateCardUsageStatusHandler;
                }

                foreach (var skillCommand in SkillCommands)
                {
                    if (skillCommand is GuHuoSkillCommand)
                    {
                        (skillCommand as GuHuoSkillCommand).GuHuoChoice = null;
                    }
                    skillCommand.OnSelectedChanged += _UpdateCardUsageStatusHandler;
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
        
        private void _ConstructCardChoiceModel(List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, List<bool> rearrangeable, int timeOutSeconds, CardChoiceRearrangeCallback callback)
        {
            if (resultDeckMaximums.Sum() != 1)
            {
                throw new NotImplementedException();
            }

            CardChoiceModel.CardStacks.Clear();
            
            int numLines = sourceDecks.Count;

            foreach (var deck in sourceDecks)
            {
                if (Game.CurrentGame.Decks[deck].Count == 0)
                {
                    continue;
                }
                CardChoiceLineViewModel line = new CardChoiceLineViewModel();
                line.DeckName = deck.DeckType.Name;
                int i = 0;
                int numCards = Game.CurrentGame.Decks[deck].Count;
                int maxColumns = Math.Max(numCards / 2 + 1, 5);
                bool firstRow = true;
                foreach (var card in Game.CurrentGame.Decks[deck])
                {
                    if (numLines == 1 && i >= maxColumns && firstRow)
                    {
                        Trace.Assert(CardChoiceModel.CardStacks.Count == 0);                        
                        CardChoiceModel.CardStacks.Add(line);                        
                        line = new CardChoiceLineViewModel();
                        line.DeckName = deck.DeckType.Name;
                        firstRow = false;
                    }
                    CardViewModel model = new CardViewModel() 
                    {
                        Card = card,
                        IsSelectionMode = true,
                        IsEnabled = true 
                    };
                    model.OnSelectedChanged += cardChoice_OnSelectedChanged;
                    line.Cards.Add(model);
                    i++;
                }
                CardChoiceModel.CardStacks.Add(line);
            }

            CardChoiceModel.TimeOutSeconds = timeOutSeconds;
        }

        private void cardChoice_OnSelectedChanged(object sender, EventArgs e)
        {
            CardViewModel model = sender as CardViewModel;
            Trace.Assert(model != null);
            if (!model.IsSelected) return;
            CardChoiceAnsweredEvent(new List<List<Card>>() { new List<Card>() { model.Card } });
            CardChoiceModel.TimeOutSeconds = 0;
            IsCardChoiceQuestionShown = false;
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

        public void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks,
                                     List<string> resultDeckNames,
                                     List<int> resultDeckMaximums, 
                                     ICardChoiceVerifier verifier,
                                     int timeOutSeconds,
                                     List<bool> rearrangeable,
                                     CardChoiceRearrangeCallback callback)
        {
            Trace.Assert(resultDeckMaximums.Count == resultDeckNames.Count);
            Trace.Assert(rearrangeable.Count == resultDeckMaximums.Count);           

            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (!IsPlayable)
                {
                    Trace.Assert(currentUsageVerifier == null);
                    TimeOutSeconds = timeOutSeconds;
                    CardChoiceAnsweredEvent(null);
                    return;
                }

                lock (verifierLock)
                {
                    CardChoiceModel.Prompt = PromptFormatter.Format(prompt);
                    _ConstructCardChoiceModel(sourceDecks, resultDeckNames, resultDeckMaximums, rearrangeable, timeOutSeconds, callback);
                    IsCardChoiceQuestionShown = true;
                }
            });
        }

        public void AskForMultipleChoice(Prompt prompt, List<string> choices, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (!IsPlayable)
                {
                    Trace.Assert(currentUsageVerifier == null);
                    TimeOutSeconds = timeOutSeconds;
                    MultipleChoiceAnsweredEvent(0);
                    return;
                }

                CurrentPrompt = PromptFormatter.Format(prompt);
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

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;

        public void Freeze()
        {
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
    }
}
