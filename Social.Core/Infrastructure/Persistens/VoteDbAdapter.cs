using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class VoteDbAdapter : IVoteRepository
    {
        private readonly SocialDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="VoteDbAdapter"/> using the provided database context.
        /// </summary>
        /// <param name="context">The <see cref="SocialDbContext"/> used for data access operations.</param>
        public VoteDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists the provided domain Vote to the database.
        /// </summary>
        /// <param name="vote">The domain Vote to convert into the database representation and persist.</param>
        public async Task AddAsync(Vote vote)
        {
            // Ensure the user exists in the database
            var entity = vote.ToEntity();
            _context.Votes.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the vote with the specified identifier from the database if it exists.
        /// </summary>
        /// <param name="voteId">The identifier of the vote to delete. If no matching vote is found, no action is taken.</param>
        public async Task DeleteAsync(Guid voteId)
        {
            // Find the vote entity by its ID
            var entity = await _context.Votes.FindAsync(voteId);

            // If found, remove it
            if (entity != null)
            {
                _context.Votes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves the vote with the specified identifier.
        /// </summary>
        /// <returns>The domain Vote matching the given id, or null if no such vote exists.</returns>
        public async Task<Vote?> GetByIdAsync(Guid voteId)
        {
            // Find the vote entity by its ID
            var entity = await _context.Votes.FindAsync(voteId);
            return entity?.ToDomain();
        }

        /// <summary>
        /// Retrieves the vote cast by a specific user for a given target.
        /// </summary>
        /// <param name="targetId">Identifier of the target being voted on.</param>
        /// <param name="targetType">Type of the vote target (for example, post or comment).</param>
        /// <param name="userId">Identifier of the user who cast the vote.</param>
        /// <returns>The matching <see cref="Vote"/> if found, otherwise <c>null</c>.</returns>
        public async Task<Vote?> GetUserVoteAsync(
            Guid targetId,
            Core.VoteTargetType targetType,
            Guid userId
        )
        {
            // Find the vote entity by targetId, targetType, and userId
            var entity = await _context.Votes.FirstOrDefaultAsync(v =>
                v.TargetId == targetId
                && v.VoteTargetType == (Entities.VoteTargetType)targetType
                && v.UserId == userId
            );
            if (entity == null)
                return null;

            return entity?.ToDomain();
        }

        /// <summary>
        /// Retrieves all votes associated with the specified target identifier and target type.
        /// </summary>
        /// <param name="targetId">Identifier of the vote target.</param>
        /// <param name="targetType">Type of the vote target.</param>
        /// <returns>A read-only list of domain Vote objects that belong to the specified target; empty list if none exist.</returns>
        public async Task<IReadOnlyList<Vote>> GetVotesForTargetAsync(
            Guid targetId,
            Core.VoteTargetType targetType
        )
        {
            // Retrieve all votes for the specified targetId and targetType
            var entities = await _context
                .Votes.Where(v =>
                    v.TargetId == targetId
                    && v.VoteTargetType == (Entities.VoteTargetType)targetType
                )
                .ToListAsync();

            return entities.Select(e => e.ToDomain()).ToList();
        }

        /// <summary>
        /// Updates an existing persisted vote with values from the provided domain Vote; does nothing if no matching vote exists.
        /// </summary>
        /// <param name="vote">Domain vote whose Id identifies the record to update and whose properties provide the new values.</param>
        public async Task UpdateAsync(Vote vote)
        {
            // Find the existing vote entity by its ID
            var entity = await _context.Votes.FindAsync(vote.Id);
            if (entity == null)
                return;

            // Update the entity's properties
            entity.TargetId = vote.TargetId;
            entity.VoteTargetType = (Entities.VoteTargetType)vote.VoteTargetType;
            entity.UserId = vote.UserId;
            entity.Upvote = vote.Upvote;
            entity.Action = (Entities.VoteAction)vote.Action;

            // Save changes to the database
            _context.Votes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public static class VoteMapper
    {
        /// <summary>
        /// Maps a domain <see cref="Vote"/> to its persistence <see cref="VoteEntity"/> representation.
        /// </summary>
        /// <param name="vote">The domain vote to convert.</param>
        /// <returns>A <see cref="VoteEntity"/> with properties copied from the provided domain vote.</returns>
        public static VoteEntity ToEntity(this Vote vote)
        {
            return new VoteEntity
            {
                Id = vote.Id,
                TargetId = vote.TargetId,
                VoteTargetType = (Entities.VoteTargetType)vote.VoteTargetType,
                UserId = vote.UserId,
                Upvote = vote.Upvote,
                Action = (Entities.VoteAction)vote.Action,
            };
        }

        /// <summary>
        /// Creates a domain Vote from a VoteEntity by mapping entity properties to the domain model.
        /// </summary>
        /// <param name="entity">The persisted VoteEntity to convert.</param>
        /// <returns>A Vote domain object with properties copied from the entity.</returns>
        public static Vote ToDomain(this VoteEntity entity)
        {
            return new Vote
            {
                Id = entity.Id,
                TargetId = entity.TargetId,
                VoteTargetType = (Core.VoteTargetType)entity.VoteTargetType,
                UserId = entity.UserId,
                Upvote = entity.Upvote,
                Action = (Core.VoteAction)entity.Action,
            };
        }
    }
}