using TriviaWhip.Shared.Models;

namespace TriviaWhip.Client.Services;

public class PurchaseService
{
    private readonly ProfileService _profileService;

    public PurchaseService(ProfileService profileService)
    {
        _profileService = profileService;
    }

    public bool TryPurchaseScheme(int schemeId, int cost)
    {
        if (_profileService.Current.OwnedSchemes.Contains(schemeId) || _profileService.Current.Coins < cost)
        {
            return false;
        }

        _profileService.Current.Coins -= cost;
        _profileService.Current.OwnedSchemes.Add(schemeId);
        return true;
    }

    public bool TryPurchaseAvatar(int avatarId, int cost)
    {
        if (_profileService.Current.OwnedAvatars.Contains(avatarId) || _profileService.Current.Coins < cost)
        {
            return false;
        }

        _profileService.Current.Coins -= cost;
        _profileService.Current.OwnedAvatars.Add(avatarId);
        return true;
    }

    public bool TryPurchaseBuff(int buffId, int cost)
    {
        if (_profileService.Current.OwnedBuffs.Contains(buffId) || _profileService.Current.Coins < cost)
        {
            return false;
        }

        _profileService.Current.Coins -= cost;
        _profileService.Current.OwnedBuffs.Add(buffId);
        _profileService.Current.Buffs = buffId switch
        {
            1 => new Buff { CoinMultiplier = 1.2, Id = buffId },
            2 => new Buff { CorrectMultiplier = 1.1, Id = buffId },
            3 => new Buff { SkipCostMultiplier = 0.5, Id = buffId },
            4 => new Buff { ExtraLife = true, Id = buffId },
            _ => _profileService.Current.Buffs
        };
        return true;
    }
}
