using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("games")]
public class GameEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Uid { get; set; }  

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public long Gold { get; set; } = 1000;
    public int Gems { get; set; } = 100;

    public DateTime CreatedTimeUtc { get; set; } = DateTime.UtcNow;
}
