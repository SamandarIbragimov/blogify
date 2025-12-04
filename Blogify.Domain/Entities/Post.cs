namespace Blogify.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relations
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}
