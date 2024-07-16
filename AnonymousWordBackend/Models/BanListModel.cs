using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Models;

[Table("ban_list")]
public class BanListModel
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ReadOnly(true)]
    [Column("id", TypeName = "bigint")]
    public long Id { get; set; }

    [Required]
    [Column("is_banned", TypeName = "boolean")]
    [Comment("Is User banned.")]
    public bool IsBanned { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("message_id", TypeName = "bigint")]
    [Comment("Message ID from the issuer chat.")]
    public required long MessageId { get; set; }
    
    [Required]
    [ReadOnly(true)]
    [Column("issuer", TypeName = "bigint")]
    public required long IssuerId { get; set; }
    
    [Required]
    [JsonIgnore]
    public UserModel? Issuer;
    
    [Required]
    [ReadOnly(true)]
    [Column("banned", TypeName = "bigint")]
    public required long BannedId { get; set; }
    
    [Required]
    [JsonIgnore]
    public UserModel? Banned;
    
    [Required]
    [ReadOnly(true)]
    [Column("banned_on", TypeName = "bigint")]
    public required long BannedOn { get; set; }
}