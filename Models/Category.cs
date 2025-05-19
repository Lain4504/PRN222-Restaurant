namespace PRN222_Restaurant.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<MenuItem> MenuItems { get; set; }
}
