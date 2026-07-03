using JetFlight.Shared.Constants;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models.Avatars;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using Microsoft.Extensions.Options;

namespace JetFlight.Service.Services
{
    public interface IAvatarService
    {
        string GetAvatarPath(AvatarKey key);
        List<AvatarKey> GetAvatarKeysPerBranch(Branches branchId);
        string GetDefaultAvatar(Branches branchId);
        public AvatarDTO GetDto(string path, Branches branchId);
        List<AvatarDTO> GetAvatars(Branches branchId);
    }

    public class AvatarService : IAvatarService
    {
        public AvatarService()
        {
        }

        public List<AvatarKey> GetAvatarKeysPerBranch(Branches branchId)
        {
            return Enum.GetValues<AvatarKey>()
                .Where(x => IsAvatarForBranch(x, branchId))
                .ToList();
        }
        private static bool IsAvatarForBranch(AvatarKey avatarKey, Branches branchId)
        {
            var attribute = avatarKey.GetCustomAttribute<AvatarFilterAttribute>();
            return attribute == null || attribute.BranchId == branchId;
        }

        public List<AvatarDTO> GetAvatars(Branches branchId)
        {
            var keys = GetAvatarKeysPerBranch(branchId);
            var avatars = keys.Select(x => new AvatarDTO
            {
                Key = x,
                Path = GetAvatarPath(x),
                Type = Enum.Parse<AvatarType>(x.GetEnumMemberValue()),
            }).ToList();

            return avatars;
        }

        public string GetAvatarPath(AvatarKey key)
            => new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/Avatars/{key}.png"
            }.ToString();

        public AvatarDTO GetDto(string path, Branches branchId)
        {
            if (path == string.Empty)
            {
                path = GetDefaultAvatar(branchId);
            }

            var avatarKey = GetAvatarKey(path);
            return new AvatarDTO
            {
                Key = avatarKey,
                Path = path,
                Type = Enum.Parse<AvatarType>(avatarKey.GetEnumMemberValue()),
            };
        }

        private AvatarKey GetAvatarKey(string path)
        {
            var file = Path.GetFileName(path);
            var key = file.Replace(".png", string.Empty);

            return Enum.Parse<AvatarKey>(key);
        }

        public string GetDefaultAvatar(Branches branchId)
        {
            var avatarKey = branchId == Branches.BirdJet
                ? AvatarKey.BirdJet1
                : AvatarKey.CatJet1;

            return GetAvatarPath(avatarKey);
        }
    }
}
