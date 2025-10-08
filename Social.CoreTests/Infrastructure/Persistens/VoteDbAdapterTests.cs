using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Social.Core;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;

namespace SocialCoreTests.Infrastructure.Persistens
{
    public class VoteDbAdapterTests
    {
        private SocialDbContext _dbContext;
        private VoteDbAdapter _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unik DB pr. test
                .Options;

            _dbContext = new SocialDbContext(options);
            _dbContext.Database.EnsureCreated();

            _repository = new VoteDbAdapter(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddAsync_Should_Add_Vote()
        {
            var vote = new Vote
            {
                TargetId = Guid.NewGuid(),
                VoteTargetType = VoteTargetType.Post,
                UserId = Guid.NewGuid(),
                Upvote = true,
            };

            await _repository.AddAsync(vote);
            var fromDb = await _repository.GetByIdAsync(vote.Id);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(vote.Id, Is.EqualTo(fromDb!.Id));
            Assert.That(fromDb.Upvote, Is.True);
        }

        [Test]
        public async Task UpdateAsync_Should_Update_Existing_Vote()
        {
            var vote = new Vote
            {
                TargetId = Guid.NewGuid(),
                VoteTargetType = VoteTargetType.Comment,
                UserId = Guid.NewGuid(),
                Upvote = true,
            };

            await _repository.AddAsync(vote);

            vote.Upvote = false;
            await _repository.UpdateAsync(vote);

            var fromDb = await _repository.GetByIdAsync(vote.Id);
            Assert.That(fromDb!.Upvote, Is.False);
        }

        [Test]
        public async Task DeleteAsync_Should_Remove_Vote()
        {
            var vote = new Vote
            {
                TargetId = Guid.NewGuid(),
                VoteTargetType = VoteTargetType.Post,
                UserId = Guid.NewGuid(),
                Upvote = true,
            };

            await _repository.AddAsync(vote);
            await _repository.DeleteAsync(vote.Id);

            var fromDb = await _repository.GetByIdAsync(vote.Id);
            Assert.That(fromDb, Is.Null);
        }

        [Test]
        public async Task GetUserVoteAsync_Should_Return_UserVote()
        {
            var vote = new Vote
            {
                TargetId = Guid.NewGuid(),
                VoteTargetType = VoteTargetType.Post,
                UserId = Guid.NewGuid(),
                Upvote = true,
            };

            await _repository.AddAsync(vote);

            var fromDb = await _repository.GetUserVoteAsync(
                vote.TargetId,
                vote.VoteTargetType,
                vote.UserId
            );

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(vote.UserId, Is.EqualTo(fromDb!.UserId));
        }

        [Test]
        public async Task GetVotesForTargetAsync_Should_Return_All_Votes()
        {
            var targetId = Guid.NewGuid();

            var vote1 = new Vote
            {
                TargetId = targetId,
                VoteTargetType = VoteTargetType.Comment,
                UserId = Guid.NewGuid(),
                Upvote = true,
            };
            var vote2 = new Vote
            {
                TargetId = targetId,
                VoteTargetType = VoteTargetType.Comment,
                UserId = Guid.NewGuid(),
                Upvote = false,
            };

            await _repository.AddAsync(vote1);
            await _repository.AddAsync(vote2);

            var votes = await _repository.GetVotesForTargetAsync(targetId, VoteTargetType.Comment);

            Assert.That(votes.Count, Is.EqualTo(2));
        }
    }
}
