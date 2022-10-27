using _2048.Framework;
using _2048.Models;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace _2048.ViewModels
{
    public enum GameState
    {
        Ready,
        Playing,
        Finished
    }

    public class MainViewModel : ModelBase
    {
        private readonly GameCore<BlockInfo> gameCore;

        private int score = 0;

        /// <summary>
        /// 分数
        /// </summary>
        public int Score
        {
            get => score;
            set
            {
                score = value;

                var bestScore = Properties.Settings.Default.BestScore;

                if (bestScore < score)
                {
                    Properties.Settings.Default.BestScore = score;

                    Properties.Settings.Default.Save();
                }

                OnPropertyChanged(nameof(Score));
            }
        }

        private GameState state = GameState.Ready;

        /// <summary>
        /// 游戏状态
        /// </summary>
        public GameState State
        {
            get => state;
            set
            {
                state = value;

                OnPropertyChanged(nameof(state));
            }
        }

        /// <summary>
        /// 所有元素
        /// </summary>
        public ObservableCollection<BlockInfo> Blocks { get; } = new ObservableCollection<BlockInfo>();

        public ICommand MoveRightCommand { get; }

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveForwardCommand { get; }

        public ICommand MoveBackCommand { get; }

        public ICommand RestartGameCommand { get; }

        public ICommand ReturnStateCommand { get; }

        public string ArchivePath => $"{System.IO.Directory.GetCurrentDirectory()}\\archive.sav";

        public MainViewModel()
        {
            gameCore = new GameCore<BlockInfo>(4, 4, Blocks)
            {
                OnGameOver = (win) =>
                {
                    if (!win)
                    {
                        State = GameState.Finished;
                    }
                },
                SetScore = (score, append) =>
                 {
                     if (append)
                     {
                         Score += score;
                     }
                     else
                     {
                         Score = score;
                     }
                 },
                GetScore = () =>
                 {
                     return Score;
                 }
            };

            MoveForwardCommand = new DelegateCommand(OnMoveForward, CanMoveForward);
            MoveLeftCommand = new DelegateCommand(OnMoveLeft, CanMoveLeft);
            MoveRightCommand = new DelegateCommand(OnMoveRight, CanMoveRight);
            MoveBackCommand = new DelegateCommand(OnMoveBack, CanMoveBack);
            ReturnStateCommand = new DelegateCommand(OnReturnState, CanReturn);

            RestartGameCommand = new DelegateCommand(OnRestartGame, CanRestartGame);
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnRestartGame();

            if (Application.Current != null && !DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                gameCore.Load(ArchivePath);
            }
        }

        private bool CanReturn()
        {
            return State == GameState.Playing;
        }

        private void OnReturnState()
        {
            gameCore.Return();
        }

        private bool CanRestartGame()
        {
            return true;
        }

        private void OnRestartGame()
        {
            //重置分数
            Score = 0;

            gameCore.ResetBlocks();

            gameCore.Reflush(out _, 2);

            gameCore.ClearStates();

            //设置为游戏中状态
            State = GameState.Playing;
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            gameCore.Save(ArchivePath);
        }

        private bool CanMoveBack()
        {
            return State == GameState.Playing;
        }

        private void OnMoveBack()
        {
            gameCore.MoveBack();
        }

        private bool CanMoveRight()
        {
            return State == GameState.Playing;
        }

        private void OnMoveRight()
        {
            gameCore.MoveRight();
        }

        private bool CanMoveLeft()
        {
            return State == GameState.Playing;
        }

        private void OnMoveLeft()
        {
            gameCore.MoveLeft();
        }

        private bool CanMoveForward()
        {
            return State == GameState.Playing;
        }

        private void OnMoveForward()
        {
            gameCore.MoveForward();
        }
    }
}