namespace Blogify.Application.DTOs.Post;

public class UpdatePostRequest
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<string> Tags { get; set; } = new List<string>();
}
