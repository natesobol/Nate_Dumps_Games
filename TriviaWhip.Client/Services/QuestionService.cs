using System.Net.Http.Json;
using System.Text.Json;
using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class QuestionService
{
    private readonly HttpClient _httpClient;
    private List<Question>? _cache;

    public QuestionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Question>> GetAllAsync()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var questions = await _httpClient.GetFromJsonAsync<List<Question>>("data/questions.json", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _cache = questions ?? new List<Question>();
        return _cache;
    }

    public async Task<IReadOnlyList<Question>> GetFilteredAsync(Settings settings)
    {
        var all = await GetAllAsync();
        if (!settings.CategoriesSelected.Any() && !settings.SubCategoriesSelected.Any())
        {
            return all;
        }

        return all.Where(q =>
            (!settings.CategoriesSelected.Any() || settings.CategoriesSelected.Contains(q.Category)) &&
            (!settings.SubCategoriesSelected.Any() || settings.SubCategoriesSelected.Contains(q.SubCategory)))
            .ToList();
    }

    public static List<Question> Shuffle(IEnumerable<Question> questions)
    {
        var list = questions.ToList();
        var rng = new Random();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var swapIndex = rng.Next(i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
        return list;
    }
}
