using FuzzyGameEngine.Core;
using FuzzyGameEngine.ViewModels.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FuzzyGameEngine.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private FuzzyEngine engine;
        private AdaptiveController adaptive;

        public double Time { get; set; }
        public double Enemies { get; set; }
        public double Health { get; set; }
        public double Accuracy { get; set; }
        public double Damage { get; set; }

        public string Result { get; set; }
        public string Explanation { get; set; }

        public double DifficultyValue { get; set; }
        public string DifficultyLevel { get; set; }

        public ObservableCollection<string> History { get; set; } = new();

        public ICommand CalculateCommand { get; }

        public MainViewModel()
        {
            adaptive = new AdaptiveController();
            engine = EngineFactory.Create(adaptive);

            CalculateCommand = new RelayCommand(Calc);

            Time = 50;
            Enemies = 5;
            Health = 80;
            Accuracy = 70;
            Damage = 20;
        }

        private void Calc(object obj)
        {
            var state = new GameState
            {
                Time = Time,
                Enemies = Enemies,
                Health = Health,
                Accuracy = Accuracy,
                DamageTaken = Damage
            };

            var (res, exp) = engine.Evaluate(state);

            DifficultyValue = res;

            if (res < 40) DifficultyLevel = "🟢 Easy";
            else if (res < 70) DifficultyLevel = "🟡 Medium";
            else DifficultyLevel = "🔴 Hard";

            Result = $"Difficulty: {res:F2}";
            Explanation = exp;

            History.Insert(0, $"{DateTime.Now:T} → {DifficultyLevel}");

            adaptive.Learn(res);

            OnPropertyChanged(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
