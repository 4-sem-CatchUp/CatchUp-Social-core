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

        public VoteDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vote vote)
        {
            // Ensure the user exists in the database
            var entity = vote.ToEntity();
            _context.Votes.Add(entity);
            await _context.SaveChangesAsync();
        }

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

        public async Task<Vote?> GetByIdAsync(Guid voteId)
        {
            // Find the vote entity by its ID
            var entity = await _context.Votes.FindAsync(voteId);
            return entity?.ToDomain();
        }

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
        // Mapping methods from Vote to VoteEntity
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

        // Mapping method from VoteEntity to Vote
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
