namespace AnonymousWordBackend.ControllerParams;

public class MessageFilters
{
    public long? Id { get; set; }
    public long? AuthorChatMessageId { get; set; }
    public long? RecipientChatMessageId { get; set; }
    public long? StorageMessageId { get; set; }
    public long? AuthorId { get; set; }
    public long? RecipientId { get; set; }
    public string? Body { get; set; }
    public long? AuthoredOn { get; set; }
}