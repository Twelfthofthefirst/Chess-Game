using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Chess_logic;

namespace Chess_UI
{
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> movecache = new Dictionary<Position, Move>();
        private GameState gameState;
        private Position selectedPos = null;
        private Dictionary<string, MediaPlayer> soundPlayers;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            InitializeSounds();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            
        }

        private void InitializeSounds()
        {
            soundPlayers = new Dictionary<string, MediaPlayer>
            {
                { "illegal", CreateMediaPlayer("Assets/illegal.mp3") },
                { "promote", CreateMediaPlayer("Assets/promote.mp3") },
                { "move-opponent", CreateMediaPlayer("Assets/move-opponent.mp3") },
                { "castle", CreateMediaPlayer("Assets/castle.mp3") },
                { "move-self", CreateMediaPlayer("Assets/move-self.mp3") },
                { "capture", CreateMediaPlayer("Assets/capture.mp3") },
                { "move-check", CreateMediaPlayer("Assets/move-check.mp3") },
                { "game-start", CreateMediaPlayer("Assets/game-start.mp3") },
                { "game-end", CreateMediaPlayer("Assets/game-end.mp3") }
            };
        }

        private MediaPlayer CreateMediaPlayer(string soundFilePath)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri($"pack://application:,,,/{soundFilePath}"));
            return mediaPlayer;
        }

        private void PlaySound(string soundKey)
        {
            if (soundPlayers.ContainsKey(soundKey))
            {
                soundPlayers[soundKey].Stop();  // Stop any currently playing sound
                soundPlayers[soundKey].Play();
            }
        }

        

        private void BtnSoundOptions_Click(object sender, RoutedEventArgs e)
        {
            SoundOptionsWindow soundOptionsWindow = new SoundOptionsWindow();
            soundOptionsWindow.ShowDialog();
        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();

            if (movecache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
            else
            {
                PlaySound("illegal");
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
                PlaySound("promote");
            };
        }

        private void HandleMove(Move move)
        {
            bool isCapture = gameState.Board[move.ToPos.Row, move.ToPos.Column] != null;

            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            if (move.Type == MoveType.CastleKs)
            {
                PlaySound("castle");
            }

            if (move.Type == MoveType.CastleQs)
            {
                PlaySound("castle");
            }

          else  if (isCapture)
            {
                PlaySound("capture");
            }
            else
            {
                PlaySound("move-self");
            }

            if (gameState.IsInCheck(gameState.CurrentPlayer.Opponent()))
            {
                PlaySound("move-check");
            }

            if (gameState.IsGameOver())
            {
                PlaySound("game-end");
                ShowGameOver();
            }
            else
            {
                PlaySound("move-opponent");
            }
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            movecache.Clear();

            foreach (Move move in moves)
            {
                movecache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);

            foreach (Position to in movecache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in movecache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if (player == Player.White)
            {
                Cursor = ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor = ChessCursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    ShowStartMenu();
                }
            };
        }

        private void ShowStartMenu()
        {
            StartMenuWindow startMenu = new StartMenuWindow();
            startMenu.Show();
            this.Close();
        }

        private void RestartGame()
        {
            PlaySound("game-start");
            selectedPos = null;
            HideHighlights();
            movecache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;

                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }
    }
}
