using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class ProfileDbAdapter : IProfileRepository
    {
        private readonly SocialDbContext _context;

        public ProfileDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        public async Task AddFriendAsync(Guid profileId, Guid friendId)
        {
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            if (entity == null)
                return;

            var friendEntity = await _context.Profiles.FindAsync(friendId);
            if (friendEntity == null)
                return;
            if (!entity.Friends.Any(f => f.Id == friendId))
            {
                entity.Friends.Add(friendEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddProfileAsync(Profile profile)
        {
            var entity = profile.ToEntity();
            await _context.Profiles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(Guid profileId)
        {
            var entity = await _context.Profiles.FindAsync(profileId);
            if (entity != null)
            {
                _context.Profiles.Remove(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Profile with ID {profileId} not found.");
            }
        }

        public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            var entities = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .ToListAsync();
            var profiles = entities.Select(e => e.ToDomain());
            return profiles;
        }

        public async Task<Profile?> GetProfileByIdAsync(Guid profileId)
        {
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .FirstOrDefaultAsync(p => p.Id == profileId);
            return entity?.ToDomain();
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .FirstOrDefaultAsync(p => p.Id == profile.Id);

            if (entity == null)
                return;

            entity.Name = profile.Name;
            entity.Bio = profile.Bio;
            entity.DateOfSub = profile.DateOfSub;

            // Update Profile Picture
            if (profile.ProfilePic != null)
            {
                if (entity.ProfilePic == null)
                {
                    var newImage = new ImageEntity
                    {
                        FileName = profile.ProfilePic.FileName,
                        ContentType = profile.ProfilePic.ContentType,
                        Data = profile.ProfilePic.Data,
                    };
                    await _context.Images.AddAsync(newImage);
                    entity.ProfilePic = newImage;
                }
                else
                {
                    entity.ProfilePic.FileName = profile.ProfilePic.FileName;
                    entity.ProfilePic.ContentType = profile.ProfilePic.ContentType;
                    entity.ProfilePic.Data = profile.ProfilePic.Data;
                }
            }

            // Update Friends
            entity.Friends.Clear();
            foreach (var friendId in profile.Friends)
            {
                entity.Friends.Add(new ProfileEntity { Id = friendId });
            }

            _context.Profiles.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public static class ProfileMapper
    {
        public static ProfileEntity ToEntity(this Profile profile)
        {
            ImageEntity pic = null;
            if (profile.ProfilePic != null)
                pic = profile.ProfilePic.ToEntity();
            return new ProfileEntity
            {
                Id = profile.Id,
                Name = profile.Name,
                Bio = profile.Bio,
                DateOfSub = profile.DateOfSub,
                ProfilePic = pic,
                Friends = profile.Friends.Select(fid => new ProfileEntity { Id = fid }).ToList(),
            };
        }

        public static Profile ToDomain(this ProfileEntity entity)
        {
            return new Profile(
                entity.Id,
                entity.Name,
                entity.Bio,
                entity.DateOfSub,
                entity.Friends.Select(f => f.Id).ToList(),
                entity.ProfilePic != null ? entity.ProfilePic.ToDomain() : null
            );
        }
    }
}
