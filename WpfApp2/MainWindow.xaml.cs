using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TetrisWithTxt
{
    public partial class MainWindow : Window
    {
        private const int Rows = 20;
        private const int Columns = 10;
        private const int BlockSize = 30; // Размер блока

        private DispatcherTimer timer;
        private int[,] grid = new int[Rows, Columns];
        private List<Rectangle> currentShape = new List<Rectangle>();
        private Point currentPosition = new Point(Columns / 2, 0);
        private List<Point[]> shapes = new List<Point[]>
        {
            new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) }, // Квадрат так пишу
            new Point[] { new Point(-1, 0), new Point(0, 0), new Point(1, 0), new Point(2, 0) }, // I
            new Point[] { new Point(0, 0), new Point(-1, 0), new Point(0, 1), new Point(1, 1) }, // S
            new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(-1, 1) }, // Z
            new Point[] { new Point(0, 0), new Point(-1, 0), new Point(1, 0), new Point(0, 1) }, // T
            new Point[] { new Point(0, 0), new Point(-1, 0), new Point(1, 0), new Point(-1, 1) }, // L
            new Point[] { new Point(0, 0), new Point(-1, 0), new Point(1, 0), new Point(1, 1) }  // J
        };
        private int score = 0;
        private bool isGameOver = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            GameCanvas.Width = Columns * BlockSize;
            GameCanvas.Height = Rows * BlockSize;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += GameLoop;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            SpawnShape();
            timer.Start();
        }

        private void ResetGame()
        {
            Array.Clear(grid, 0, grid.Length);
            score = 0;
            isGameOver = false;
            ScoreLabel.Text = "Очки: 0";
            GameCanvas.Children.Clear();
            DrawGrid();
        }

        private void DrawGrid()
        {
            GameCanvas.Children.Clear();

            // Отрисовка существующих блоков
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (grid[i, j] == 1)
                    {
                        var rect = new Rectangle
                        {
                            Width = BlockSize,
                            Height = BlockSize,
                            Fill = Brushes.Blue,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(rect, j * BlockSize);
                        Canvas.SetTop(rect, i * BlockSize);
                        GameCanvas.Children.Add(rect);
                    }
                }
            }

            // Отрисовка текущей фигуры
            foreach (var rect in currentShape)
            {
                GameCanvas.Children.Add(rect);
            }
        }

        private void SpawnShape()
        {
            if (isGameOver) return;

            Random random = new Random();
            var shapeTemplate = shapes[random.Next(shapes.Count)];
            currentShape.Clear();
            currentPosition = new Point(Columns / 2, 0);

            foreach (var block in shapeTemplate)
            {
                double x = currentPosition.X + block.X;
                double y = currentPosition.Y + block.Y;

                if (y >= 0 && grid[(int)y, (int)x] == 1)
                {
                    EndGame();
                    return;
                }

                var rect = new Rectangle
                {
                    Width = BlockSize,
                    Height = BlockSize,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, x * BlockSize);
                Canvas.SetTop(rect, y * BlockSize);
                currentShape.Add(rect);
            }

            DrawGrid();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!MoveShape(0, 1))
            {
                PlaceShape();
                ClearRows();
                SpawnShape();
            }
        }

        private bool MoveShape(int offsetX, int offsetY)
        {
            if (CheckCollision(offsetX, offsetY)) return false;

            currentPosition.X += offsetX;
            currentPosition.Y += offsetY;

            foreach (var rect in currentShape)
            {
                double newX = Canvas.GetLeft(rect) + offsetX * BlockSize;
                double newY = Canvas.GetTop(rect) + offsetY * BlockSize;
                Canvas.SetLeft(rect, newX);
                Canvas.SetTop(rect, newY);
            }

            DrawGrid();
            return true;
        }

        private bool CheckCollision(int offsetX, int offsetY)
        {
            foreach (var rect in currentShape)
            {
                double x = Canvas.GetLeft(rect) + offsetX * BlockSize;
                double y = Canvas.GetTop(rect) + offsetY * BlockSize;

                int gridX = (int)(x / BlockSize);
                int gridY = (int)(y / BlockSize);

                if (gridX < 0 || gridX >= Columns || gridY >= Rows)
                    return true;

                if (gridY >= 0 && grid[gridY, gridX] == 1)
                    return true;
            }
            return false;
        }

        private void PlaceShape()
        {
            foreach (var rect in currentShape)
            {
                double x = Canvas.GetLeft(rect) / BlockSize;
                double y = Canvas.GetTop(rect) / BlockSize;

                if (y >= 0)
                {
                    grid[(int)y, (int)x] = 1;
                }
            }

            currentShape.Clear();
            DrawGrid();
        }

        private void ClearRows()
        {
            for (int i = Rows - 1; i >= 0; i--)
            {
                bool isFull = true;

                for (int j = 0; j < Columns; j++)
                {
                    if (grid[i, j] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    for (int k = i; k > 0; k--)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            grid[k, j] = grid[k - 1, j];
                        }
                    }

                    for (int j = 0; j < Columns; j++)
                    {
                        grid[0, j] = 0;
                    }

                    score += 100;
                    ScoreLabel.Text = $"Очки: {score}";
                    i++; // Повторить проверку для текущей строки после смещения
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            switch (e.Key)
            {
                case Key.Left:
                    MoveShape(-1, 0);
                    break;
                case Key.Right:
                    MoveShape(1, 0);
                    break;
                case Key.Down:
                    MoveShape(0, 1);
                    break;
                case Key.Space:
                    while (MoveShape(0, 1)) { }
                    break;
                case Key.Up:
                    RotateShape();
                    break;
            }
        }

        private void RotateShape()
        {
           
            List<Point> rotatedPoints = new List<Point>();
            foreach (var block in shapes[0]) // Используем текущую форму
            {
               
                double x = block.X;
                double y = block.Y;
                double rotatedX = -y;
                double rotatedY = x;
                rotatedPoints.Add(new Point(rotatedX, rotatedY));
            }

            // Проверка на столкновение после поворота
            for (int i = 0; i < rotatedPoints.Count; i++)
            {
                double x = currentPosition.X + rotatedPoints[i].X;
                double y = currentPosition.Y + rotatedPoints[i].Y;

                int gridX = (int)x;
                int gridY = (int)y;

                if (gridX < 0 || gridX >= Columns || gridY >= Rows)
                    return;

                if (gridY >= 0 && grid[gridY, gridX] == 1)
                    return;
            }

            // Применение поворота
            for (int i = 0; i < currentShape.Count; i++)
            {
                double newX = currentPosition.X + rotatedPoints[i].X;
                double newY = currentPosition.Y + rotatedPoints[i].Y;
                Canvas.SetLeft(currentShape[i], newX * BlockSize);
                Canvas.SetTop(currentShape[i], newY * BlockSize);
            }

            DrawGrid();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Управление:\n" +
                            "← - Влево\n" +
                            "→ - Вправо\n" +
                            "↓ - Быстрое падение\n" +
                            "Пробел - Мгновенное падение\n" +
                            "↑ - Поворот фигуры\n" +
                            "\nЦель: набрать как можно больше очков, заполняя горизонтальные линии.", "Помощь");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EndGame()
        {
            timer.Stop();
            isGameOver = true;
            MessageBox.Show($"Игра окончена!\nВаши очки: {score}", "Тетрис");

            // Запрос имени игрока
            string playerName = PromptForPlayerName();

            // Сохранение рекорда
            HighScore newScore = new HighScore(playerName, score, DateTime.Now);
            HighScoreManager.SaveHighScore(newScore);
        }

        private string PromptForPlayerName()
        {
           
            return Microsoft.VisualBasic.Interaction.InputBox("Введите ваше имя:", "Новый рекорд", "Игрок");
        }

        private void ShowHighScores_Click(object sender, RoutedEventArgs e)
        {
            List<HighScore> highScores = HighScoreManager.LoadHighScores();

            HighScoresWindow highScoresWindow = new HighScoresWindow(highScores);
            highScoresWindow.Owner = this;
            highScoresWindow.ShowDialog();
        }
    }
}
