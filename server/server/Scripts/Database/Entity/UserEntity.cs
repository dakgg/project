using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string PublicKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PrivateKey { get; set; } = string.Empty;

    public DateTime CreatedTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginTimeUtc { get; set; } = DateTime.UtcNow;
}
