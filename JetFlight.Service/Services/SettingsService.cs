using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services;

public interface ISettingsService
{
    Task<Dictionary<SiteSettingsKeys, string>> GetSiteSettings(byte branchId);
    Task UpdateSiteSettingsAsync(Dictionary<SiteSettingsKeys, string> settings, byte branchId);
}

public class SettingsService(IDataUnitOfWork dataUnitOfWork) : ISettingsService
{
    private readonly IDataUnitOfWork _unitOfWork = dataUnitOfWork;

    public async Task<Dictionary<SiteSettingsKeys, string>> GetSiteSettings(byte branchId)
    {
        var settings = await _unitOfWork.SiteSettings.GetAllSettings(branchId);
        return settings;
    }

    public async Task UpdateSiteSettingsAsync(Dictionary<SiteSettingsKeys, string> settings, byte branchId)
    {
        var allowedKeys = Enum.GetNames<SiteSettingsKeys>();

        if (settings.Keys.Count != allowedKeys.Length)
        {
            throw new ArgumentException("Всі ключі мають бути присутніми");
        }

        var existingSettings = _unitOfWork.SiteSettings.GetAll().Where(x => x.BranchId == branchId).ToList();

        foreach (var s in existingSettings)
        {
            var valueToReplace = settings[s.Key];
            await ValidateAsync(s.Key, valueToReplace, branchId);
            s.Value = valueToReplace;
        }

        await _unitOfWork.Save(null);
    }

    private async Task ValidateAsync(SiteSettingsKeys key, string value, byte branchId)
    {
        switch (key)
        {
            case SiteSettingsKeys.fixed_post_id:
                {
                    if (value == string.Empty)
                    {
                        break;
                    }

                    if (int.TryParse(value, out var postId))
                    {
                        var post = await _unitOfWork.Posts.Find(x => x.Id == postId).FirstOrDefaultAsync();

                        if (post != null && post.Status.GetValueOrDefault() && post.OriginId == null && (post.BranchId == null || post.BranchId == branchId))
                        {
                            break;
                        }
                    }

                    throw new ArgumentException($"{nameof(SiteSettingsKeys.fixed_post_id)} не валідний id до статті.");
                }
            default: break;
        }
    }
}
