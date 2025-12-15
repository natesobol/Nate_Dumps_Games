using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TriviaWhip.Shared.Models;

public record SubcategoryDefinition([property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug);

public record CategoryDefinition([property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("subcategories")] IReadOnlyList<SubcategoryDefinition> Subcategories);

public static class CategoryCatalog
{
    public static readonly IReadOnlyList<CategoryDefinition> Categories = new List<CategoryDefinition>
    {
        new("Science", "science", new List<SubcategoryDefinition>
        {
            new("Anatomy", "science__anatomy"),
            new("Biology", "science__biology"),
            new("Chemistry", "science__chemistry"),
            new("Physics", "science__physics")
        }),
        new("History", "history", new List<SubcategoryDefinition>
        {
            new("Ancient Egypt", "history__ancient-egypt"),
            new("Crime History", "history__crime-history"),
            new("Revolutionary War", "history__revolutionary-war"),
            new("Roman Empire", "history__roman-empire"),
            new("Us", "history__us"),
            new("World", "history__world"),
            new("World War 2", "history__world-war-2")
        }),
        new("Culture", "culture", new List<SubcategoryDefinition>
        {
            new("Fashion", "culture__fashion"),
            new("Movie", "culture__movie"),
            new("Nineties Culture", "culture__nineties-culture"),
            new("Reality Tv", "culture__reality-tv"),
            new("Tv", "culture__tv")
        }),
        new("Music", "music", new List<SubcategoryDefinition>
        {
            new("Beatles", "music__beatles"),
            new("Eighties", "music__eighties"),
            new("Nineties Music", "music__nineties-music"),
            new("Rock", "music__rock")
        }),
        new("Geography", "geography", new List<SubcategoryDefinition>
        {
            new("Us", "geography__us"),
            new("Us Geography", "geography__us-geography"),
            new("World", "geography__world"),
            new("World Geography", "geography__world-geography")
        }),
        new("Religion", "religion", new List<SubcategoryDefinition>
        {
            new("Bible", "religion__bible"),
            new("Islam", "religion__islam")
        }),
        new("Vehicle", "vehicle", new List<SubcategoryDefinition>
        {
            new("Car Mechanic", "vehicle__car-mechanic"),
            new("Cdl", "vehicle__cdl"),
            new("Sailing", "vehicle__sailing")
        }),
        new("Sports", "sports", new List<SubcategoryDefinition>
        {
            new("Baseball", "sports__baseball"),
            new("Basketball", "sports__basketball"),
            new("Golf", "sports__golf")
        }),
        new("Technology", "technology", new List<SubcategoryDefinition>
        {
            new("Windows Os", "technology__windows-os")
        }),
        new("Humanities", "humanities", new List<SubcategoryDefinition>
        {
            new("Literature", "humanities__literature"),
            new("Philosophy", "humanities__philosophy")
        }),
        new("Holidays", "holidays", new List<SubcategoryDefinition>
        {
            new("Christmas", "holidays__christmas")
        })
    };

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> SlugIndex = new Dictionary<string, IReadOnlyList<string>>
    {
        { "science", new List<string> { "science__anatomy", "science__biology", "science__chemistry", "science__physics" } },
        { "history", new List<string> { "history__ancient-egypt", "history__crime-history", "history__revolutionary-war", "history__roman-empire", "history__us", "history__world", "history__world-war-2" } },
        { "culture", new List<string> { "culture__fashion", "culture__movie", "culture__nineties-culture", "culture__reality-tv", "culture__tv" } },
        { "music", new List<string> { "music__beatles", "music__eighties", "music__nineties-music", "music__rock" } },
        { "geography", new List<string> { "geography__us", "geography__us-geography", "geography__world", "geography__world-geography" } },
        { "religion", new List<string> { "religion__bible", "religion__islam" } },
        { "vehicle", new List<string> { "vehicle__car-mechanic", "vehicle__cdl", "vehicle__sailing" } },
        { "sports", new List<string> { "sports__baseball", "sports__basketball", "sports__golf" } },
        { "technology", new List<string> { "technology__windows-os" } },
        { "humanities", new List<string> { "humanities__literature", "humanities__philosophy" } },
        { "holidays", new List<string> { "holidays__christmas" } }
    };

    public static string GetCategoryDisplay(string slug)
    {
        return Categories.FirstOrDefault(c => string.Equals(c.Slug, GetMainSlug(slug), StringComparison.OrdinalIgnoreCase))?.Name
               ?? ToTitle(slug);
    }

    public static string GetSubcategoryDisplay(string slug)
    {
        var match = Categories.SelectMany(c => c.Subcategories)
            .FirstOrDefault(s => string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            return match.Name;
        }

        var (_, subSlug) = SplitSlug(slug);
        return string.IsNullOrWhiteSpace(subSlug) ? ToTitle(slug) : ToTitle(subSlug);
    }

    public static (string CategorySlug, string SubcategorySlug) NormalizeQuestionCategory(Question question)
    {
        var categoryPart = question.Category;
        var (main, sub) = SplitCategory(categoryPart);
        if (!string.IsNullOrWhiteSpace(question.SubCategory))
        {
            sub = NormalizeSegment(question.SubCategory);
        }

        var categorySlug = NormalizeSegment(main);
        var subSlug = string.IsNullOrWhiteSpace(sub) ? string.Empty : $"{categorySlug}__{NormalizeSegment(sub)}";
        question.SubCategory = subSlug;
        return (categorySlug, subSlug);
    }

    public static string GetMainSlug(string value)
    {
        var (main, _) = SplitSlug(value);
        return main;
    }

    public static string? GetSubSlug(string value)
    {
        var (_, sub) = SplitSlug(value);
        return sub;
    }

    private static (string Main, string Sub) SplitCategory(string category)
    {
        var parts = (category ?? string.Empty).Split('_', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var main = parts.Length > 0 ? parts[0] : string.Empty;
        var sub = parts.Length > 1 ? parts[1] : string.Empty;
        return (main, sub);
    }

    private static (string Main, string Sub) SplitSlug(string slug)
    {
        var parts = (slug ?? string.Empty).Split("__", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var main = NormalizeSegment(parts.Length > 0 ? parts[0] : string.Empty);
        var sub = parts.Length > 1 ? NormalizeSegment(parts[1]) : string.Empty;
        return (main, sub);
    }

    private static string NormalizeSegment(string value)
    {
        var cleaned = value.Replace(" ", "-").Replace("_", "-").Trim();
        return cleaned.ToLowerInvariant();
    }

    private static string ToTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return string.Join(" ", value.Replace("_", " ").Replace("-", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}
