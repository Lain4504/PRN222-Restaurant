namespace PRN222_Restaurant.Models;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TableId { get; set; }
    public DateTime ReservationTime { get; set; }
    public string Note { get; set; }

    public User User { get; set; }
    public Table Table { get; set; }
}
