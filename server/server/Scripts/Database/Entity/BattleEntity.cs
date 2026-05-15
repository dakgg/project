using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("battle_records")]
public class BattleEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Uid { get; set; }

    [Required]
    public int StageId { get; set; }

    public bool IsWin { get; set; }

    public int RewardGold { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
