using System.Globalization;
using System.Linq;
using System.Text;

namespace TriviaWhip.Client.Shared;

public static class AssetCatalog
{
    private const string AssetBasePath = "/Files/";

    public static readonly string[] TitleBackgrounds =
    {
        "rome.png",
        "worldgeography.png",
        "worldhistory.png",
        "statue-of-liberty.png",
    };

    public static readonly string[] AvatarOptions =
    {
        "add-user.png",
        "bear.png",
        "pawn.png",
        "rocket.png",
        "groundhog.png",
        "horse.png",
        "ideas.png",
        "bigfoot.png",
        "butterfly.png",
        "rook.png"
    };

    public static readonly Dictionary<int, string> BuffIcons = new()
    {
        { 1, "coinico.png" },
        { 2, "statisticsico.png" },
        { 3, "saleico.png" },
        { 4, "heartico.png" }
    };

    public static readonly Dictionary<int, string> IdolIcons = new()
    {
        { 0, "idol.png" },
        { 1, "tier1.png" },
        { 2, "tier2.png" },
        { 3, "tier3.png" },
        { 4, "tier4.png" },
        { 5, "tier5.png" }
    };

    private static readonly Dictionary<string, string> MainCategoryIcons = new()
    {
        { "science", "science.png" },
        { "history", "history.png" },
        { "culture", "culture.png" },
        { "music", "music.png" },
        { "sports", "sports.png" },
        { "religion", "religion.png" },
        { "vehicle", "vehicles.png" },
        { "vehicles", "vehicles.png" },
        { "technology", "tech.png" },
        { "tech", "tech.png" },
        { "geography", "geo.png" },
        { "geo", "geo.png" },
        { "humanities", "history-book.png" },
        { "holidays", "holidays.png" }
    };

    private static readonly Dictionary<string, string> SubCategoryIcons = new()
    {
        { "anatomy", "anatomy.png" },
        { "ancientegypt", "pyramid.png" },
        { "baseball", "baseball.png" },
        { "basketball", "basketball.png" },
        { "beatles", "beatles.png" },
        { "bible", "bible.png" },
        { "biology", "biology.png" },
        { "chemistry", "chemistry.png" },
        { "christmas", "christmas.png" },
        { "crimehistory", "criminalhistory.png" },
        { "cdl", "cdl.png" },
        { "carmechanic", "mechanic.png" },
        { "eighties", "80smusic.png" },
        { "fashion", "fashion.png" },
        { "golf", "golf.png" },
        { "islam", "islam.png" },
        { "literature", "literature.png" },
        { "movie", "movies.png" },
        { "ninetiesmusic", "90smusic.png" },
        { "ninetiesculture", "90s.png" },
        { "philosophy", "philosophy.png" },
        { "physics", "physics.png" },
        { "realitytv", "realitytv.png" },
        { "revolutionarywar", "revwar.png" },
        { "rock", "rock.png" },
        { "romanempire", "rome.png" },
        { "sailing", "sailing.png" },
        { "television", "tv.png" },
        { "ushistory", "ushistory.png" },
        { "us", "usgeo.png" },
        { "usgeography", "usgeo.png" },
        { "windows", "windows.png" },
        { "windowsos", "windows.png" },
        { "world", "worldgeography.png" },
        { "worldhistory", "worldhistory.png" },
        { "worldgeography", "worldgeography.png" },
        { "worldwar2", "ww2.png" }
    };

    public static string GetAssetPath(string fileName) => $"{AssetBasePath}{Uri.EscapeDataString(fileName)}";

    public static string GetRandomTitleBackground(Random rng)
    {
        return GetAssetPath(TitleBackgrounds[rng.Next(TitleBackgrounds.Length)]);
    }

    public static string GetAvatar(int avatarIndex)
    {
        var file = avatarIndex >= 0 && avatarIndex < AvatarOptions.Length
            ? AvatarOptions[avatarIndex]
            : AvatarOptions.First();
        return GetAssetPath(file);
    }

    public static string GetBuffIcon(int buffId)
    {
        return GetAssetPath(BuffIcons.TryGetValue(buffId, out var file) ? file : "add buff.png");
    }

    public static string GetIdolIcon(int idolLevel)
    {
        return GetAssetPath(IdolIcons.TryGetValue(idolLevel, out var file) ? file : IdolIcons[0]);
    }

    public static string GetMainCategoryIcon(string name)
    {
        return GetAssetPath(LookupIcon(MainCategoryIcons, name) ?? "categoryview.png");
    }

    public static string GetSubCategoryIcon(string name)
    {
        return GetAssetPath(LookupIcon(SubCategoryIcons, name) ?? "categoryview.png");
    }

    private static string? LookupIcon(IDictionary<string, string> map, string key)
    {
        return map.TryGetValue(Normalize(key), out var file) ? file : null;
    }

    public static string Normalize(string value)
    {
        var sb = new StringBuilder();
        foreach (var ch in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }

    public static string ToTitle(string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Replace("_", " "));
    }
}
