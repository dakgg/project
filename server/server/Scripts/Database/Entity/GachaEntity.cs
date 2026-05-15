using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("gacha_states")]
public class GachaEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Uid { get; set; }

    [Required]
    public int Index { get; set; }

    public int PityCount { get; set; }

    public DateTime UpdatedTimeUtc { get; set; } = DateTime.UtcNow;
}