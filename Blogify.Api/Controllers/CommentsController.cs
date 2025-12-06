using Blogify.Application.DTOs.Comments;
using Blogify.Domain.Entities;
using Blogify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blogify.Api.Controllers;

[ApiController]
[Route("api/posts/{postId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly BlogifyDbContext _db;

    public CommentsController(BlogifyDbContext db)
    {
        _db = db;
    }

    // CREATE COMMENT
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
            return NotFound("Post not found");

        var comment = new Comment
        {
            Content = request.Content,
            AuthorId = userId,
            PostId = postId
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var response = new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            AuthorId = userId,
            AuthorUsername = User.Identity!.Name!
        };

        return Ok(response);
    }

    // GET COMMENTS FOR POST
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(Guid postId)
    {
        var comments = await _db.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .ToListAsync();

        var response = comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            AuthorId = c.AuthorId,
            AuthorUsername = c.Author.Username
        }).ToList();

        return Ok(response);
    }

    // UPDATE COMMENT
    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);
        if (comment == null)
            return NotFound("Comment not found");

        if (comment.AuthorId != userId)
            return Forbid();

        comment.Content = request.Content;

        await _db.SaveChangesAsync();

        return Ok("Comment updated successfully");
    }

    // DELETE COMMENT
    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);
        if (comment == null)
            return NotFound("Comment not found");

        if (comment.AuthorId != userId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return Ok("Comment deleted successfully");
    }
}