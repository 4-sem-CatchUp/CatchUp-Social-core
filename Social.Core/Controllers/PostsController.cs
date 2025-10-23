using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social.Core;
using Social.Core.Ports.Incomming;

namespace Social.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostUseCases _postUseCases;

        public PostsController(IPostUseCases postUseCases)
        {
            _postUseCases = postUseCases;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        //{
        //    var postId = await _postUseCases.CreatePostAsync(dto.AuthorId, dto.Content, dto.Image);
        //    return CreatedAtAction(nameof(GetPostById), new { id = postId }, null);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            // Dette kunne kalde et IPostQueryUseCase, hvis du har et query-lag
            return Ok(new { Message = $"Dummy: Would return post {id}" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto dto)
        {
            await _postUseCases.UpdatePostAsync(id, dto.NewTitle, dto.NewContent);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            await _postUseCases.DeletePost(id);
            return NoContent();
        }

        [HttpPost("{id}/vote")]
        public async Task<IActionResult> VotePost(Guid id, [FromBody] VoteDto dto)
        {
            await _postUseCases.VotePost(id, dto.UpVote, dto.UserId);
            return Ok();
        }

        [HttpGet("{id}/vote/{userId}")]
        public async Task<IActionResult> GetUserVote(Guid id, Guid userId)
        {
            var vote = await _postUseCases.GetUserPostVote(id, userId);
            return Ok(vote);
        }
    }

    public class CreatePostDto
    {
        public Guid AuthorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public List<Image>? Images { get; set; }
    }

    public class UpdatePostDto
    {
        public string? NewTitle { get; set; }
        public string? NewContent { get; set; }
    }

    public class VoteDto
    {
        public Guid UserId { get; set; }
        public bool UpVote { get; set; }
    }
}
