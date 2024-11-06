// public record Request
// {
//     public bool Stream { get; set; }
//     public List<Message> Messages { get; set; } = [];
// }
// public record Request
// {
//     public bool Stream { get; set; }
//     public List<Message> Messages { get; set; } = new List<Message>();
// }
public record Request
{
    public bool Stream { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();
}
