using System;
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

        try
        {
            var questions = await _httpClient.GetFromJsonAsync<List<Question>>("data/questions.json", new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _cache = questions ?? new List<Question>();
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Failed to fetch questions: {ex.Message}");
            _cache = new List<Question>();
        }
        catch (NotSupportedException ex)
        {
            Console.Error.WriteLine($"Questions payload type unsupported: {ex.Message}");
            _cache = new List<Question>();
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Failed to parse questions: {ex.Message}");
            _cache = new List<Question>();
        }

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
            IsCategoryAllowed(settings, q) &&
            IsSubCategoryAllowed(settings, q))
            .ToList();
    }

    private static bool IsCategoryAllowed(Settings settings, Question question)
    {
        var (categorySlug, subSlug) = CategoryCatalog.NormalizeQuestionCategory(question);
        return !settings.CategoriesSelected.Any()
               || settings.CategoriesSelected.Contains(categorySlug)
               || (!string.IsNullOrWhiteSpace(subSlug) && settings.CategoriesSelected.Contains(subSlug));
    }

    private static bool IsSubCategoryAllowed(Settings settings, Question question)
    {
        var (categorySlug, subSlug) = CategoryCatalog.NormalizeQuestionCategory(question);
        return !settings.SubCategoriesSelected.Any()
               || settings.SubCategoriesSelected.Contains(subSlug)
               || settings.SubCategoriesSelected.Contains(categorySlug)
               || settings.SubCategoriesSelected.Contains(CategoryCatalog.GetSubSlug(subSlug) ?? string.Empty);
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
