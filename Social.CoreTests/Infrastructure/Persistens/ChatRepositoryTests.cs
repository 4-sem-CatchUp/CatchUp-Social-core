using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Core;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class ChatRepositoryTests
    {
        private IChatRepository _repo;

        [SetUp]
        public void Setup()
        {
            //_repo = new InMemoryChatRepository(); // din egen test-impl.
        }

        //[Test]
        //public void CreateChat_ShouldStoreAndRetrieve()
        //{
        //    var chat = new Chat();
        //    var created = _repo.CreateChat(chat);

        //    var loaded = _repo.GetChat(chat.ChatId);

        //    Assert.That(loaded.ChatId, Is.EqualTo(chat.ChatId));
        //}
    }
}
