using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AnonymousWordBackend.Models;

[Table("users")]
public class User
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ReadOnly(true)]
    [Column("id", TypeName = "bigint")]
    public long Id { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("tid", TypeName = "bigint")]
    public required long TelegramId { get; set; }
    
    [Column("link", TypeName = "varchar(32)")]
    public string? Link { get; set; }
    
    [DefaultValue(null)]
    [Column("welcome", TypeName = "varchar(256)")]
    public string? WelcomeMessage { get; set; }
    
    [Required]
    [JsonIgnore]
    public ICollection<Message> ReceivedMessages = [];
    
    [Required]
    [JsonIgnore]        
    public ICollection<Message> SentMessages = [];
    
    [Required]
    [ReadOnly(true)]
    [Column("registered_at", TypeName = "bigint")]
    public required long RegisteredAt { get; set; }
}