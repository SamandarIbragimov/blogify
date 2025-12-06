using Blogify.Application.DTOs.Post;
using Blogify.Domain.Entities;
using Blogify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Blogify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly BlogifyDbContext _dbContext;

    public PostsController(BlogifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // CREATE POST
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        // Handle tags
        var tags = new List<Tag>();
        foreach (var tagName in request.Tags)
        {
            var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _dbContext.Tags.Add(tag);
            }
            tags.Add(tag);
        }

        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Author = user,
            Tags = tags.Select(t => new PostTag { Tag = t }).ToList()
        };

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();

        return Ok(new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author.Username,
            Tags = post.Tags.Select(t => t.Tag.Name).ToList()
        });
    }

    // GET ALL POSTS
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
            .ToListAsync();

        var response = posts.Select(p => new PostResponse
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            AuthorId = p.AuthorId,
            AuthorUsername = p.Author.Username,
            Tags = p.Tags.Select(t => t.Tag.Name).ToList()
        }).ToList();

        return Ok(response);
    }

    // GET SINGLE POST
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var p = await _dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound();

        var response = new PostResponse
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            AuthorId = p.AuthorId,
            AuthorUsername = p.Author.Username,
            Tags = p.Tags.Select(t => t.Tag.Name).ToList()
        };

        return Ok(response);
    }

    // UPDATE POST
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await _dbContext.Posts
            .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();
        if (post.AuthorId != userId) return Forbid();

        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAt = DateTime.UtcNow;

        // Update tags
        post.Tags.Clear();
        foreach (var tagName in request.Tags)
        {
            var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _dbContext.Tags.Add(tag);
            }
            post.Tags.Add(new PostTag { Post = post, Tag = tag });
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author.Username,
            Tags = post.Tags.Select(t => t.Tag.Name).ToList()
        });
    }

    // DELETE POST
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await _dbContext.Posts.FindAsync(id);

        if (post == null) return NotFound();
        if (post.AuthorId != userId) return Forbid();

        _dbContext.Posts.Remove(post);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Post deleted successfully" });
    }
}
