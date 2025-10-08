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
            var entity = vote.ToEntity();
            _context.Votes.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid voteId)
        {
            var entity = await _context.Votes.FindAsync(voteId);
            if (entity != null)
            {
                _context.Votes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Vote?> GetByIdAsync(Guid voteId)
        {
            var entity = await _context.Votes.FindAsync(voteId);
            return entity?.ToDomain();
        }

        public async Task<Vote?> GetUserVoteAsync(
            Guid targetId,
            Core.VoteTargetType targetType,
            Guid userId
        )
        {
            var entity = await _context.Votes.FirstOrDefaultAsync(v =>
                v.TargetId == targetId
                && v.VoteTargetType == (Entities.VoteTargetType)targetType
                && v.UserId == userId
            );
            return entity?.ToDomain();
        }

        public async Task<IReadOnlyList<Vote>> GetVotesForTargetAsync(
            Guid targetId,
            Core.VoteTargetType targetType
        )
        {
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
            var entity = await _context.Votes.FindAsync(vote.Id);
            if (entity == null)
                return;

            entity.TargetId = vote.TargetId;
            entity.VoteTargetType = (Entities.VoteTargetType)vote.VoteTargetType;
            entity.UserId = vote.UserId;
            entity.Upvote = vote.Upvote;
            entity.Action = (Entities.VoteAction)vote.Action;

            _context.Votes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public static class VoteMapper
    {
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
