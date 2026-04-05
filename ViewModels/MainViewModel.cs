using FuzzyGameEngine.Core;
using FuzzyGameEngine.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FuzzyGameEngine.ViewModels
{
    public class MainViewModel : BaseViewModel, IDataErrorInfo
    {
        private const int MaxChartPoints = 30;
        private const int MaxHistoryEntries = 50;
        private const int MaxLogEntries = 20;

        private readonly FuzzyEngine engine;
        private readonly AdaptiveController adaptive;
        private readonly DispatcherTimer timer;

        public ObservableCollection<double> ChartData { get; } = new();
        public ObservableCollection<double> BiasHistory { get; } = new();
        public ObservableCollection<HistoryEntry> History { get; } = new();
        public ObservableCollection<string> LogEntries { get; } = new();

        public ICommand CalculateCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand ClearValuesCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadFileCommand { get; }
        public ICommand LoadHistoryCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        // Поля
        private double time; public double Time { get => time; set => Set(ref time, value); }
        private double enemies; public double Enemies { get => enemies; set => Set(ref enemies, value); }
        private double health; public double Health { get => health; set => Set(ref health, value); }
        private double accuracy; public double Accuracy { get => accuracy; set => Set(ref accuracy, value); }
        private double damage; public double Damage { get => damage; set => Set(ref damage, value); }
        private double playerSkill; public double PlayerSkill { get => playerSkill; set => Set(ref playerSkill, value); }
        private double stressLevel; public double StressLevel { get => stressLevel; set => Set(ref stressLevel, value); }
        private double progress; public double Progress { get => progress; set => Set(ref progress, value); }
        private double reactionTime; public double ReactionTime { get => reactionTime; set => Set(ref reactionTime, value); }

        private double difficultyValue; public double DifficultyValue { get => difficultyValue; set => Set(ref difficultyValue, value); }
        private double bias; public double Bias { get => bias; set => Set(ref bias, value); }

        private bool isAutoMode;
        public bool IsAutoMode
        {
            get => isAutoMode;
            set
            {
                Set(ref isAutoMode, value);
                RaisePropertyChanged(nameof(IsManualMode));
                if (value) timer.Start(); else timer.Stop();
            }
        }

        private string detailedConclusion = string.Empty;
        public string DetailedConclusion
        {
            get => detailedConclusion;
            set => Set(ref detailedConclusion, value);
        }

        public string ShortConclusion
        {
            get
            {
                if (DifficultyValue == 0)
                    return string.Empty;

                if (DifficultyValue < 40) return "Легкий";
                if (DifficultyValue < 70) return "Оптимально";
                return "Занадто складно";
            }
        }

        private HistoryEntry? selectedHistoryEntry;
        public HistoryEntry? SelectedHistoryEntry
        {
            get => selectedHistoryEntry;
            set => Set(ref selectedHistoryEntry, value);
        }

        public bool IsManualMode => !isAutoMode;

        private bool isDarkTheme = true;
        public bool IsDarkTheme
        {
            get => isDarkTheme;
            set => Set(ref isDarkTheme, value);
        }

        public string Error => null;

        public string this[string columnName] => columnName switch
        {
            nameof(Time) => Time < 0 || Time > 300 ? "Час повинен бути від 0 до 300 секунд" : null,
            nameof(Enemies) => Enemies < 0 || Enemies > 50 ? "Кількість ворогів — від 0 до 50" : null,
            nameof(Health) => Health < 0 || Health > 100 ? "Здоров'я — від 0 до 100%" : null,
            nameof(Accuracy) => Accuracy < 0 || Accuracy > 100 ? "Точність — від 0 до 100%" : null,
            nameof(Damage) => Damage < 0 || Damage > 100 ? "Пошкодження — від 0 до 100%" : null,
            nameof(PlayerSkill) => PlayerSkill < 0 || PlayerSkill > 100 ? "Навичка — від 0 до 100%" : null,
            nameof(StressLevel) => StressLevel < 0 || StressLevel > 100 ? "Стрес — від 0 до 100%" : null,
            nameof(Progress) => Progress < 0 || Progress > 100 ? "Прогрес — від 0 до 100%" : null,
            nameof(ReactionTime) => ReactionTime < 0 || ReactionTime > 1000 ? "Час реакції — від 0 до 1000 мс" : null,
            _ => null
        };

        public MainViewModel()
        {
            adaptive = new AdaptiveController();
            engine = EngineFactory.Create(adaptive);

            CalculateCommand = new RelayCommand(_ => Calc());
            SimulateCommand = new RelayCommand(_ => Simulate());
            ClearHistoryCommand = new RelayCommand(_ => ClearHistory());
            ClearValuesCommand = new RelayCommand(_ => ClearValues());
            SaveCommand = new RelayCommand(_ => Save());
            LoadFileCommand = new RelayCommand(_ => LoadFromFile());
            LoadHistoryCommand = new RelayCommand(_ => LoadSelectedHistory());
            ToggleThemeCommand = new RelayCommand(_ => IsDarkTheme = !IsDarkTheme);

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (_, _) => Simulate();

            // Початкові значення
            Time = 120;
            Enemies = 8;
            Health = 70;
            Accuracy = 65;
            Damage = 20;
            PlayerSkill = 60;
            StressLevel = 40;
            Progress = 50;
            ReactionTime = 350;

            Calc();
        }

        private void Calc()
        {
            try
            {
                var state = new GameState
                {
                    Time = Time,
                    Enemies = Enemies,
                    Health = Health,
                    Accuracy = Accuracy,
                    DamageTaken = Damage,
                    PlayerSkill = PlayerSkill,
                    StressLevel = StressLevel,
                    Progress = Progress,
                    ReactionTime = ReactionTime
                };

                var (res, explain, fullLog) = engine.Evaluate(state);

                DifficultyValue = res;
                Bias = adaptive.GetBias();

                DetailedConclusion = res switch
                {
                    < 30 => "Занадто легко — рівень дуже простий, гравець проходить його без зусиль і може швидко втратити інтерес",
                    < 70 => "Оптимально — ідеальний баланс складності. Гравець відчуває виклик, але не розчаровується",
                    _ => "Занадто складно — рівень надто важкий, гравець може швидко втратити мотивацію та кинути гру"
                };

                ChartData.Add(res);
                BiasHistory.Add(Bias);
                if (ChartData.Count > MaxChartPoints)
                {
                    ChartData.RemoveAt(0);
                    BiasHistory.RemoveAt(0);
                }

                adaptive.Learn(res);

                var entry = new HistoryEntry
                {
                    DifficultyValue = res,
                    Bias = Bias,
                    Time = Time,
                    Enemies = Enemies,
                    Health = Health,
                    Accuracy = Accuracy,
                    Damage = Damage,
                    PlayerSkill = PlayerSkill,
                    StressLevel = StressLevel,
                    Progress = Progress,
                    ReactionTime = ReactionTime,
                    Explain = explain
                };

                History.Insert(0, entry);
                if (History.Count > MaxHistoryEntries)
                    History.RemoveAt(History.Count - 1);

                LogEntries.Insert(0, fullLog);
                if (LogEntries.Count > MaxLogEntries)
                    LogEntries.RemoveAt(LogEntries.Count - 1);

                RaisePropertyChanged(nameof(ChartData));
                RaisePropertyChanged(nameof(BiasHistory));
                RaisePropertyChanged(nameof(ShortConclusion));
                RaisePropertyChanged(nameof(DetailedConclusion));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка розрахунку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Simulate()
        {
            var r = new Random();
            Time = r.Next(20, 200);
            Enemies = r.Next(1, 20);
            Health = r.Next(20, 100);
            Accuracy = r.Next(40, 100);
            Damage = r.Next(0, 100);
            PlayerSkill = r.Next(20, 100);
            StressLevel = r.Next(0, 100);
            Progress = r.Next(0, 100);
            ReactionTime = r.Next(100, 800);

            Calc();
        }

        private void LoadSelectedHistory()
        {
            if (SelectedHistoryEntry == null) return;

            Time = SelectedHistoryEntry.Time;
            Enemies = SelectedHistoryEntry.Enemies;
            Health = SelectedHistoryEntry.Health;
            Accuracy = SelectedHistoryEntry.Accuracy;
            Damage = SelectedHistoryEntry.Damage;
            PlayerSkill = SelectedHistoryEntry.PlayerSkill;
            StressLevel = SelectedHistoryEntry.StressLevel;
            Progress = SelectedHistoryEntry.Progress;
            ReactionTime = SelectedHistoryEntry.ReactionTime;

            DifficultyValue = SelectedHistoryEntry.DifficultyValue;
            Bias = SelectedHistoryEntry.Bias;

            RaisePropertyChanged(nameof(ShortConclusion));
            RaisePropertyChanged(nameof(DetailedConclusion));
        }

        private void ClearHistory()
        {
            History.Clear();
            LogEntries.Clear();
            ChartData.Clear();
            BiasHistory.Clear();
        }

        private void ClearValues()
        {
            Time = 0;
            Enemies = 0;
            Health = 0;
            Accuracy = 0;
            Damage = 0;
            PlayerSkill = 0;
            StressLevel = 0;
            Progress = 0;
            ReactionTime = 0;
            DifficultyValue = 0;
            Bias = 1.0;

            DetailedConclusion = string.Empty;

            RaisePropertyChanged(nameof(ShortConclusion));
            RaisePropertyChanged(nameof(DetailedConclusion));
        }

        private void Save()
        {
            var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "fuzzy_results.csv" };
            if (dialog.ShowDialog() != true) return;

            try
            {
                using var sw = new StreamWriter(dialog.FileName);
                sw.WriteLine("Timestamp,Difficulty,Bias,Time,Enemies,Health,Accuracy,Damage,PlayerSkill,StressLevel,Progress,ReactionTime,Conclusion");
                foreach (var e in History)
                {
                    sw.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0:yyyy-MM-dd HH:mm:ss},{1:F2},{2:F3},{3:F1},{4:F1},{5:F1},{6:F1},{7:F1},{8:F1},{9:F1},{10:F1},{11:F1},{12}",
                        e.Timestamp, e.DifficultyValue, e.Bias, e.Time, e.Enemies, e.Health,
                        e.Accuracy, e.Damage, e.PlayerSkill, e.StressLevel, e.Progress, e.ReactionTime,
                        e.DifficultyValue < 40 ? "Легкий" : e.DifficultyValue < 70 ? "Оптимально" : "Занадто складно"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
        }

        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
            if (dialog.ShowDialog() != true) return;

            try
            {
                History.Clear();
                using var sr = new StreamReader(dialog.FileName);
                sr.ReadLine(); // header

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 13) continue;

                    var entry = new HistoryEntry
                    {
                        Timestamp = DateTime.Parse(parts[0]),
                        DifficultyValue = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        Bias = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        Time = double.Parse(parts[3], CultureInfo.InvariantCulture),
                        Enemies = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        Health = double.Parse(parts[5], CultureInfo.InvariantCulture),
                        Accuracy = double.Parse(parts[6], CultureInfo.InvariantCulture),
                        Damage = double.Parse(parts[7], CultureInfo.InvariantCulture),
                        PlayerSkill = double.Parse(parts[8], CultureInfo.InvariantCulture),
                        StressLevel = double.Parse(parts[9], CultureInfo.InvariantCulture),
                        Progress = double.Parse(parts[10], CultureInfo.InvariantCulture),
                        ReactionTime = double.Parse(parts[11], CultureInfo.InvariantCulture),
                        Explain = ""
                    };

                    History.Add(entry);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }
    }
}