using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Salt { get; set; } = string.Empty;

    public DateTime CreatedTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginTimeUtc { get; set; } = DateTime.UtcNow;
}
