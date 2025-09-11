using Social.Core;

namespace Social.Core.Ports.Outgoing
{
    public interface IVoteRepository
    {
        Task<Vote?> GetByIdAsync(Guid voteId);
        Task AddAsync(Vote vote);
        Task UpdateAsync(Vote vote);
        Task DeleteAsync(Guid voteId);

        Task<Vote?> GetUserVoteAsync(Guid targetId, VoteTargetType targetType, Guid userId);
        Task<IReadOnlyList<Vote>> GetVotesForTargetAsync(Guid targetId, VoteTargetType targetType);
    }
}
