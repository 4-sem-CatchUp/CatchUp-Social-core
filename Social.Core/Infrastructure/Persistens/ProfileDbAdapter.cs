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

        /// <summary>
        /// Initializes a new instance of <see cref="ProfileDbAdapter"/> that uses the provided <see cref="SocialDbContext"/> for data operations.
        /// </summary>
        public ProfileDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds the profile identified by <paramref name="friendId"/> to the friends of the profile identified by <paramref name="profileId"/>.
        /// </summary>
        /// <param name="profileId">The ID of the profile to which a friend will be added.</param>
        /// <param name="friendId">The ID of the profile to add as a friend.</param>
        /// <remarks>
        /// If either profile does not exist or the friendship already exists, no changes are made. When added, the change is persisted to the database.
        /// </remarks>
        public async Task AddFriendAsync(Guid profileId, Guid friendId)
        {
            // Fetch the profile along with its friends
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            if (entity == null)
                return;

            // Fetch the friend profile
            var friendEntity = await _context.Profiles.FindAsync(friendId);

            if (friendEntity == null)
                return;

            // Avoid adding duplicate friends
            if (!entity.Friends.Any(f => f.Id == friendId))
            {
                entity.Friends.Add(friendEntity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds a new profile to the persistent store.
        /// </summary>
        /// <param name="profile">The domain Profile to persist.</param>
        /// <exception cref="DbUpdateException">Thrown when a profile with the same ID already exists.</exception>
        public async Task AddProfileAsync(Profile profile)
        {
            // Ensure the profile does not already exist
            var entity = profile.ToEntity();
            try
            {
                await _context.Profiles.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Profile with the same ID already exists.", ex);
            }
        }

        /// <summary>
        /// Deletes the profile with the specified ID from the database.
        /// </summary>
        /// <param name="profileId">The unique identifier of the profile to delete.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no profile with the given ID exists.</exception>
        public async Task DeleteProfileAsync(Guid profileId)
        {
            // Find the profile entity by its ID
            var entity = await _context.Profiles.FindAsync(profileId);

            // If found, remove it
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

        /// <summary>
        /// Retrieves all profiles from the database including each profile's friends and profile picture.
        /// </summary>
        /// <returns>An IEnumerable of domain Profile objects for all stored profiles, with friend IDs and profile picture included when present.</returns>
        public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            // Fetch all profiles along with their friends and profile pictures
            var entities = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .ToListAsync();

            // Map entities to domain models
            var profiles = entities.Select(e => e.ToDomain());

            return profiles;
        }

        /// <summary>
        /// Retrieves a profile by its identifier, eagerly including its friends and profile picture.
        /// </summary>
        /// <param name="profileId">The identifier of the profile to retrieve.</param>
        /// <returns>The Profile with the specified ID, or `null` if no matching profile exists.</returns>
        public async Task<Profile?> GetProfileByIdAsync(Guid profileId)
        {
            // Find the profile entity by its ID, including friends and profile picture
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            return entity?.ToDomain();
        }

        /// <summary>
        /// Persist changes from the given domain Profile to the corresponding database record if it exists.
        /// </summary>
        /// <param name="profile">Domain profile containing updated fields, optional ProfilePic, and the list of friend IDs to set.</param>
        public async Task UpdateProfileAsync(Profile profile)
        {
            // Fetch the existing profile entity
            var entity = await _context
                .Profiles.Include(p => p.Friends)
                .Include(p => p.ProfilePic)
                .FirstOrDefaultAsync(p => p.Id == profile.Id);

            if (entity == null)
                return;

            // Update basic fields
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
        /// <summary>
        /// Create a persistence representation of the given domain profile.
        /// </summary>
        /// <param name="profile">The domain profile to convert.</param>
        /// <returns>A ProfileEntity representing the profile; includes a converted ProfilePic if present and Friends mapped as entity references with only their Id set.</returns>
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

        /// <summary>
        /// Converts a persisted ProfileEntity into its domain Profile representation, including friend IDs and profile picture if present.
        /// </summary>
        /// <param name="entity">The persisted profile entity to convert.</param>
        /// <returns>The corresponding domain <see cref="Profile"/>.</returns>
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