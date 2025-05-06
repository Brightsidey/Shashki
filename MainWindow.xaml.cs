using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CheckersGame
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 8;
        private const int CellSize = 60;
        private const int PieceRadius = 25;

        private Piece[,] board = new Piece[BoardSize, BoardSize];
        private Piece selectedPiece = null;
        private List<Point> possibleMoves = new List<Point>();
        private bool isWhiteTurn = true;
        private bool mustCapture = false;
        private Point? lastCapturePosition = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            DrawBoard();
            UpdateStatus();
        }

        private void InitializeBoard()
        {
            // Заполняем доску шашками
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if ((row + col) % 2 == 1)
                    {
                        if (row < 3)
                        {
                            board[row, col] = new Piece(false, false); 
                        }
                        else if (row > 4)
                        {
                            board[row, col] = new Piece(true, false);
                        }
                    }
                }
            }
        }

        private void DrawBoard()
        {
            BoardCanvas.Children.Clear();
            BoardCanvas.Width = BoardSize * CellSize;
            BoardCanvas.Height = BoardSize * CellSize;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    var cell = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = (row + col) % 2 == 0 ? Brushes.LightGray : Brushes.SaddleBrown
                    };

                    Canvas.SetLeft(cell, col * CellSize);
                    Canvas.SetTop(cell, row * CellSize);
                    BoardCanvas.Children.Add(cell);
                }
            }

            // Рисуем шашки
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] != null)
                    {
                        var piece = board[row, col];
                        var ellipse = new Ellipse
                        {
                            Width = PieceRadius * 2,
                            Height = PieceRadius * 2,
                            Fill = piece.IsWhite ? Brushes.White : Brushes.Black,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 1
                        };

                        if (piece.IsKing)
                        {
                            ellipse.Stroke = Brushes.Gold;
                            ellipse.StrokeThickness = 3;
                        }

                        Canvas.SetLeft(ellipse, col * CellSize + (CellSize - PieceRadius * 2) / 2);
                        Canvas.SetTop(ellipse, row * CellSize + (CellSize - PieceRadius * 2) / 2);
                        BoardCanvas.Children.Add(ellipse);
                    }
                }
            }

            // Подсвечиваем возможные ходы
            foreach (var move in possibleMoves)
            {
                var highlight = new Ellipse
                {
                    Width = PieceRadius,
                    Height = PieceRadius,
                    Fill = Brushes.LimeGreen,
                    Opacity = 0.5
                };

                Canvas.SetLeft(highlight, move.Y * CellSize + (CellSize - PieceRadius) / 2);
                Canvas.SetTop(highlight, move.X * CellSize + (CellSize - PieceRadius) / 2);
                BoardCanvas.Children.Add(highlight);
            }

            // Подсвечиваем выбранную шашку
            if (selectedPiece != null)
            {
                for (int row = 0; row < BoardSize; row++)
                {
                    for (int col = 0; col < BoardSize; col++)
                    {
                        if (board[row, col] == selectedPiece)
                        {
                            var highlight = new Rectangle
                            {
                                Width = CellSize,
                                Height = CellSize,
                                Stroke = Brushes.Yellow,
                                StrokeThickness = 3
                            };

                            Canvas.SetLeft(highlight, col * CellSize);
                            Canvas.SetTop(highlight, row * CellSize);
                            BoardCanvas.Children.Add(highlight);
                            break;
                        }
                    }
                }
            }
        }

        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(BoardCanvas);
            int row = (int)(point.Y / CellSize);
            int col = (int)(point.X / CellSize);

            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize) return;

            // Если есть обязательное взятие, проверяем, что ход именно им
            if (mustCapture && selectedPiece != null && possibleMoves.Contains(new Point(row, col)))
            {
                MovePiece(row, col);
                return;
            }

            // Если выбрана шашка текущего игрока
            if (board[row, col] != null && board[row, col].IsWhite == isWhiteTurn)
            {
                selectedPiece = board[row, col];
                possibleMoves = GetPossibleMoves(row, col);
                DrawBoard();
                return;
            }

            // Если выбрана пустая клетка и есть выбранная шашка
            if (board[row, col] == null && selectedPiece != null && possibleMoves.Contains(new Point(row, col)))
            {
                MovePiece(row, col);
            }
        }

        private void MovePiece(int toRow, int toCol)
        {
            int fromRow = -1, fromCol = -1;
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] == selectedPiece)
                    {
                        fromRow = row;
                        fromCol = col;
                        break;
                    }
                }
            }

            // Перемещаем шашку
            board[toRow, toCol] = selectedPiece;
            board[fromRow, fromCol] = null;

            // Для дамки обрабатываем все взятия по пути
            if (selectedPiece.IsKing)
            {
                int rowStep = toRow > fromRow ? 1 : -1;
                int colStep = toCol > fromCol ? 1 : -1;
                int steps = Math.Abs(toRow - fromRow);

                for (int i = 1; i < steps; i++)
                {
                    int currentRow = fromRow + i * rowStep;
                    int currentCol = fromCol + i * colStep;

                    if (board[currentRow, currentCol] != null)
                    {
                        board[currentRow, currentCol] = null;
                        lastCapturePosition = new Point(toRow, toCol);
                        break; // Дамка может бить только одну шашку за ход
                    }
                }
            }
            else // Для обычной шашки
            {
                bool isCapture = Math.Abs(toRow - fromRow) == 2;
                if (isCapture)
                {
                    int capturedRow = (fromRow + toRow) / 2;
                    int capturedCol = (fromCol + toCol) / 2;
                    board[capturedRow, capturedCol] = null;
                    lastCapturePosition = new Point(toRow, toCol);
                }
                else
                {
                    lastCapturePosition = null;
                }
            }

            // Проверяем превращение в дамку
            if ((selectedPiece.IsWhite && toRow == 0) || (!selectedPiece.IsWhite && toRow == BoardSize - 1))
            {
                selectedPiece.IsKing = true;
            }

            // Проверяем, есть ли продолжение взятия
            bool canContinueCapture = false;
            if (lastCapturePosition != null)
            {
                var nextCaptures = GetPossibleMoves(toRow, toCol);
                foreach (var move in nextCaptures)
                {
                    if (Math.Abs(move.X - toRow) >= 2 || selectedPiece.IsKing)
                    {
                        canContinueCapture = true;
                        break;
                    }
                }
            }

            if (canContinueCapture)
            {
                selectedPiece = board[toRow, toCol];
                possibleMoves = GetPossibleMoves(toRow, toCol);
                mustCapture = true;
            }
            else
            {
                isWhiteTurn = !isWhiteTurn;
                selectedPiece = null;
                possibleMoves.Clear();
                mustCapture = CheckMustCapture();
            }

            DrawBoard();
            UpdateStatus();
            CheckGameOver();
        }

        private List<Point> GetPossibleMoves(int row, int col, bool checkCapturesOnly = false)
        {
            var moves = new List<Point>();
            var piece = board[row, col];

            if (piece == null) return moves;

            // Для дамки
            if (piece.IsKing)
            {
                // Проверяем все 4 диагональных направления
                int[] rowDirections = { -1, -1, 1, 1 };
                int[] colDirections = { -1, 1, -1, 1 };

                for (int i = 0; i < 4; i++)
                {
                    int r = row + rowDirections[i];
                    int c = col + colDirections[i];
                    bool canJump = false;
                    Point? capturePos = null;

                    while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize)
                    {
                        if (board[r, c] == null)
                        {
                            if (!canJump)
                            {
                                if (!checkCapturesOnly)
                                    moves.Add(new Point(r, c));
                            }
                            else
                            {
                                moves.Add(new Point(r, c));
                            }
                        }
                        else
                        {
                            if (board[r, c].IsWhite == piece.IsWhite || canJump) break;

                            // Проверяем возможность взятия
                            int nextR = r + rowDirections[i];
                            int nextC = c + colDirections[i];

                            if (nextR >= 0 && nextR < BoardSize && nextC >= 0 && nextC < BoardSize &&
                                board[nextR, nextC] == null)
                            {
                                canJump = true;
                                capturePos = new Point(r, c);
                                r = nextR;
                                c = nextC;
                                moves.Add(new Point(r, c));
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        r += rowDirections[i];
                        c += colDirections[i];
                    }
                }
            }
            else // Для обычной шашки
            {
                int direction = piece.IsWhite ? -1 : 1;
                int[] captureDirections = { -1, 1 };

                // Простые ходы (если не проверяем только взятия)
                if (!checkCapturesOnly)
                {
                    int newRow = row + direction;
                    if (newRow >= 0 && newRow < BoardSize)
                    {
                        if (col - 1 >= 0 && board[newRow, col - 1] == null)
                        {
                            moves.Add(new Point(newRow, col - 1));
                        }
                        if (col + 1 < BoardSize && board[newRow, col + 1] == null)
                        {
                            moves.Add(new Point(newRow, col + 1));
                        }
                    }
                }

                // Проверяем взятия
                foreach (int colDir in captureDirections)
                {
                    int newRow = row + 2 * direction;
                    int newCol = col + 2 * colDir;

                    if (newRow >= 0 && newRow < BoardSize && newCol >= 0 && newCol < BoardSize)
                    {
                        int enemyRow = row + direction;
                        int enemyCol = col + colDir;

                        if (enemyRow >= 0 && enemyRow < BoardSize && enemyCol >= 0 && enemyCol < BoardSize &&
                            board[enemyRow, enemyCol] != null && board[enemyRow, enemyCol].IsWhite != piece.IsWhite &&
                            board[newRow, newCol] == null)
                        {
                            moves.Add(new Point(newRow, newCol));
                        }
                    }
                }
            }

            // Если проверяем только взятия, удаляем простые ходы
            if (checkCapturesOnly)
            {
                moves.RemoveAll(m => Math.Abs(m.X - row) < 2 && !piece.IsKing);
            }

            return moves;
        }

        private bool CheckMustCapture()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] != null && board[row, col].IsWhite == isWhiteTurn)
                    {
                        // Проверяем только взятия (параметр checkCapturesOnly = true)
                        var moves = GetPossibleMoves(row, col, true);
                        if (moves.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateStatus()
        {
            StatusText.Text = isWhiteTurn ? "Ход белых" : "Ход чёрных";
            if (mustCapture)
            {
                StatusText.Text += " (Обязательное взятие!)";
            }
        }

        private void CheckGameOver()
        {
            bool whiteHasPieces = false;
            bool blackHasPieces = false;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col] != null)
                    {
                        if (board[row, col].IsWhite)
                        {
                            whiteHasPieces = true;
                        }
                        else
                        {
                            blackHasPieces = true;
                        }
                    }
                }
            }

            if (!whiteHasPieces)
            {
                MessageBox.Show("Чёрные победили!", "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
            }
            else if (!blackHasPieces)
            {
                MessageBox.Show("Белые победили!", "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
            }
            else
            {
                // Проверяем, есть ли у текущего игрока возможные ходы
                bool hasValidMoves = false;
                for (int row = 0; row < BoardSize; row++)
                {
                    for (int col = 0; col < BoardSize; col++)
                    {
                        if (board[row, col] != null && board[row, col].IsWhite == isWhiteTurn)
                        {
                            var moves = GetPossibleMoves(row, col);
                            if (moves.Count > 0)
                            {
                                hasValidMoves = true;
                                break;
                            }
                        }
                    }
                    if (hasValidMoves) break;
                }

                if (!hasValidMoves)
                {
                    string winner = isWhiteTurn ? "Чёрные" : "Белые";
                    MessageBox.Show($"{winner} победили! У противника нет ходов.", "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetGame();
                }
            }
        }

        private void ResetGame()
        {
            board = new Piece[BoardSize, BoardSize];
            selectedPiece = null;
            possibleMoves.Clear();
            isWhiteTurn = true;
            mustCapture = false;
            lastCapturePosition = null;
            InitializeBoard();
            DrawBoard();
            UpdateStatus();
        }
    }

    public class Piece
    {
        public bool IsWhite { get; }
        public bool IsKing { get; set; }

        public Piece(bool isWhite, bool isKing)
        {
            IsWhite = isWhite;
            IsKing = isKing;
        }
    }
}
