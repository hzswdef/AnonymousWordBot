using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Models;

[Table("messages")]
public class MessageModel
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ReadOnly(true)]
    [Column("id", TypeName = "bigint")]
    public long Id { get; set; }

    [Required]
    [ReadOnly(true)]
    [Column("author_chat_message_id", TypeName = "bigint")]
    [Comment("Message ID from the chat with its author.")]
    public required long AuthorChatMessageId { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("recipient_chat_message_id", TypeName = "bigint")]
    [Comment("Message ID from the chat with the recipient.")]
    public required long RecipientChatMessageId { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("storage_message_id", TypeName = "bigint")]
    [Comment("Message ID from storage channel.")]
    public required long StorageMessageId { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("author", TypeName = "bigint")]
    public required long AuthorId { get; set; }
    
    [Required]
    [JsonIgnore]
    public UserModel? Author;
    
    [Required]
    [ReadOnly(true)]
    [Column("recipient", TypeName = "bigint")]
    public required long RecipientId { get; set; }
    
    [Required]
    [JsonIgnore]
    public UserModel? Recipient;

    [DefaultValue(null)]
    [ReadOnly(true)]
    [Column("message", TypeName = "varchar(4096)")]
    public string? Body { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("authored_on", TypeName = "bigint")]
    public required long AuthoredOn { get; set; }
}