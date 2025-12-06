namespace Blogify.Application.DTOs.Comments;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = null!;
}
