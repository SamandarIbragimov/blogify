namespace Blogify.Application.DTOs.Post;

public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public string AuthorUsername { get; set; } = null!;
    public Guid AuthorId { get; set; }

    public List<string> Tags { get; set; } = new();
}
