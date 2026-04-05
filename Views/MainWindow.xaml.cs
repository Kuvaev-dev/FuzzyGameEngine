using FuzzyGameEngine.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FuzzyGameEngine.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel vm;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm = new MainViewModel();

            vm.ChartData.CollectionChanged += (_, _) => DrawCharts();
            vm.BiasHistory.CollectionChanged += (_, _) => DrawCharts();
            vm.PropertyChanged += Vm_PropertyChanged;
            Loaded += (_, _) => DrawCharts();

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsDarkTheme))
                    ApplyTheme();
            };

            ApplyTheme();
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MainViewModel.ChartData) or nameof(MainViewModel.BiasHistory))
                DrawCharts();
        }

        private void ApplyTheme()
        {
            bool dark = vm.IsDarkTheme;
            Resources["MainBackground"] = new SolidColorBrush(dark ? Color.FromRgb(15, 15, 26) : Color.FromRgb(245, 247, 250));
            Resources["MainForeground"] = new SolidColorBrush(dark ? Color.FromRgb(224, 224, 255) : Color.FromRgb(30, 35, 45));
            Resources["PanelBackground"] = new SolidColorBrush(dark ? Color.FromRgb(26, 26, 46) : Color.FromRgb(255, 255, 255));
            Resources["HistoryItemBackground"] = new SolidColorBrush(dark ? Color.FromRgb(37, 37, 58) : Color.FromRgb(248, 250, 252));
            Resources["DifficultyForeground"] = new SolidColorBrush(dark ? Color.FromRgb(0, 255, 187) : Color.FromRgb(0, 180, 130));
            Resources["ConclusionForeground"] = new SolidColorBrush(dark ? Color.FromRgb(255, 204, 51) : Color.FromRgb(200, 140, 0));
            Resources["AccentBrush"] = new SolidColorBrush(dark ? Color.FromRgb(0, 212, 255) : Color.FromRgb(0, 119, 204));
            Resources["LogBackground"] = new SolidColorBrush(dark ? Color.FromRgb(17, 34, 51) : Color.FromRgb(240, 245, 255));
            Resources["LogForeground"] = new SolidColorBrush(dark ? Color.FromRgb(176, 224, 255) : Color.FromRgb(30, 30, 40));
            Resources["ChartBackground"] = new SolidColorBrush(dark ? Color.FromRgb(17, 34, 51) : Color.FromRgb(240, 245, 255));
        }

        private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (vm.SelectedHistoryEntry != null)
                vm.LoadHistoryCommand.Execute(null);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawCharts();

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            if (e.AddedItems[0] is not TabItem tabItem) return;
            if (tabItem.Header.ToString() == "Графіки")
                DrawCharts();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        private void NumericTextBox_Paste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string)) ?? string.Empty;

            if (!pastedText.All(char.IsDigit))
            {
                e.CancelCommand();
            }
        }

        private void DrawCharts()
        {
            DrawLineChart(DifficultyCanvas, vm.ChartData, vm.IsDarkTheme ? Colors.Cyan : Colors.DodgerBlue, 0, 100, "Складність рівня");
            DrawLineChart(BiasCanvas, vm.BiasHistory, vm.IsDarkTheme ? Colors.Orange : Colors.DarkOrange, 0.7, 1.3, "Адаптивний bias");
        }

        private static void DrawLineChart(Canvas canvas, ObservableCollection<double> data, Color lineColor,
                                   double minVal, double maxVal, string title)
        {
            canvas.Children.Clear();

            if (data.Count < 2)
            {
                var tb = new TextBlock { Text = "Немає даних для графіка", Foreground = Brushes.Gray, FontSize = 14 };
                Canvas.SetLeft(tb, 20); Canvas.SetTop(tb, 80);
                canvas.Children.Add(tb);
                return;
            }

            double w = canvas.ActualWidth > 10 ? canvas.ActualWidth : 700;
            double h = canvas.ActualHeight > 10 ? canvas.ActualHeight : 200;
            double step = w / (data.Count - 1);

            // сітка
            for (int i = 1; i < 5; i++)
            {
                double y = h * i / 5;
                canvas.Children.Add(new Line { X1 = 0, X2 = w, Y1 = y, Y2 = y, Stroke = Brushes.Gray, StrokeThickness = 1, Opacity = 0.25 });
            }

            var poly = new Polyline { Stroke = new SolidColorBrush(lineColor), StrokeThickness = 5, StrokeLineJoin = PenLineJoin.Round };

            for (int i = 0; i < data.Count; i++)
            {
                double x = i * step;
                double norm = (data[i] - minVal) / (maxVal - minVal);
                double y = h * (1 - norm);

                poly.Points.Add(new Point(x, y));

                var dot = new Ellipse { Width = 10, Height = 10, Fill = new SolidColorBrush(lineColor) };
                Canvas.SetLeft(dot, x - 5);
                Canvas.SetTop(dot, y - 5);
                canvas.Children.Add(dot);
            }

            canvas.Children.Add(poly);

            // підписи по осі Y
            for (int i = 0; i <= 4; i++)
            {
                double val = maxVal - (maxVal - minVal) * i / 4;
                var tb = new TextBlock { Text = val.ToString("F1"), Foreground = Brushes.Gray, FontSize = 12, FontWeight = FontWeights.Medium };
                Canvas.SetLeft(tb, 10);
                Canvas.SetTop(tb, h * i / 4 - 8);
                canvas.Children.Add(tb);
            }

            // значення останньої точки
            double lastVal = data[^1];
            double lastX = (data.Count - 1) * step;
            double lastY = h * (1 - (lastVal - minVal) / (maxVal - minVal));

            var lastTb = new TextBlock
            {
                Text = lastVal.ToString("F2"),
                Foreground = new SolidColorBrush(lineColor),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };

            double labelX = lastX + 12;
            if (labelX + 70 > w) labelX = lastX - 70;

            Canvas.SetLeft(lastTb, labelX);
            Canvas.SetTop(lastTb, lastY - 26);
            canvas.Children.Add(lastTb);

            // заголовок графіка
            var titleTb = new TextBlock { Text = title, Foreground = new SolidColorBrush(lineColor), FontWeight = FontWeights.Bold, FontSize = 14 };
            Canvas.SetLeft(titleTb, w / 2 - 80);
            Canvas.SetTop(titleTb, -26);
            canvas.Children.Add(titleTb);
        }
    }
}