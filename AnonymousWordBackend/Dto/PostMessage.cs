namespace AnonymousWordBackend.Dto;

public class PostMessage
{
    public required long AuthorChatMessageId { get; set; }
    public required long RecipientChatMessageId { get; set; }
    public required long StorageMessageId { get; set; }
    public required long AuthorId { get; set; }
    public required long RecipientId { get; set; }
    public string? Body { get; set; }
}