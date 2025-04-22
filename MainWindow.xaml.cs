using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CheckersBoard
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupCheckers();
        }

        private void SetupCheckers()
        {
            int cellIndex = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = Pole.Children[cellIndex] as Rectangle;
                    cellIndex++;

                    if (cell.Fill != Brushes.Brown) continue;

                    if (row < 3)
                    {
                        AddCheckerToCell(cell, Brushes.Black);
                    }
                    else if (row > 4)
                    {
                        AddCheckerToCell(cell, Brushes.White);
                    }
                }
            }
        }

        private void AddCheckerToCell(Rectangle cell, SolidColorBrush color)
        {
            if (cell.Parent is not Grid cellContainer)
            {
                cellContainer = new Grid();
                var parentPanel = (Panel)cell.Parent;
                var cellIndex = parentPanel.Children.IndexOf(cell);

                parentPanel.Children.RemoveAt(cellIndex);
                parentPanel.Children.Insert(cellIndex, cellContainer);

                cellContainer.Children.Add(cell);
            }

            var checker = new Ellipse
            {
                Width = 60,
                Height = 60,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            cellContainer.Children.Add(checker);
        }
    }
}