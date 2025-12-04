namespace Blogify.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;

    public ICollection<PostTag> Posts { get; set; } = new List<PostTag>();
}
