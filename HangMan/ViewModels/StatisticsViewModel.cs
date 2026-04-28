using HangMan.Models;
using HangMan.Services;
using System.Collections.ObjectModel;

namespace HangMan.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        public ObservableCollection<StatisticsRow> Rows { get; set; } = new();

        public StatisticsViewModel()
        {
            StatisticsService service = new StatisticsService();
            var statistics = service.LoadStatistics();

            foreach (var user in statistics)
            {
                foreach (var category in user.Categories)
                {
                    Rows.Add(new StatisticsRow
                    {
                        Username = user.Username,
                        Category = category.Key,
                        GamesPlayed = category.Value.GamesPlayed,
                        GamesWon = category.Value.GamesWon
                    });
                }
            }
        }
    }
}