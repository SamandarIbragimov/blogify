namespace Blogify.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
}
