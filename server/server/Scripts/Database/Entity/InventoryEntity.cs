using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("inventories")]
public class InventoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Uid { get; set; }

    [Required]
    public int ItemId { get; set; }

    public int Count { get; set; } = 1;

    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
}
