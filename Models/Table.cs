namespace PRN222_Restaurant.Models;

public class Table
{
    public int Id { get; set; } 
    public int TableNumber { get; set; } 
    public int Capacity { get; set; } 
    public string Status { get; set; } = "Available"; 

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
