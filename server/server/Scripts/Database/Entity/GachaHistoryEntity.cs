using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("gacha_history")]
public class GachaHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Uid { get; set; }

    [Required]
    public int PoolIndex { get; set; }

    [Required]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(8)]
    public string Rarity { get; set; } = string.Empty;

    public int GoldReward { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
