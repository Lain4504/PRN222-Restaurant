namespace PRN222_Restaurant.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash"; // Cash, CreditCard, QR, etc.
    public string Status { get; set; } = "Paid"; // Paid, Failed, Pending

    public Order Order { get; set; }
}
