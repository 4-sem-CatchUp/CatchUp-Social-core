using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using Social.Core;
using Social.Social.Infrastructure.Notification;
using System.Threading.Tasks;

[TestFixture]
public class ChatHubTests
{
    private Mock<HubCallerContext> _mockContext;
    private Mock<IClientProxy> _mockClients;
    private ChatHub _hub;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<HubCallerContext>();
        _mockClients = new Mock<IClientProxy>();
        _hub = new ChatHub();

        _hub.Context = _mockContext.Object;
        _hub.Clients = new Mock<IHubCallerClients>().Object;
    }

    [Test]
    public async Task NotifyMessageSent_ShouldCallClientsAllSendAsync()
    {
        var chat = new Chat();
        var message = new ChatMessage(chat.ChatId, Profile.CreateNewProfile("Alice"), "Hej");

        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        _hub.Clients = mockClients.Object;

        await _hub.NotifyMessageSent(message);

        mockClientProxy.Verify(c => c.SendAsync("ReceiveMessage", message, default), Times.Once);
    }
}

