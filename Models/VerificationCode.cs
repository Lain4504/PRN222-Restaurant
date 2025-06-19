public class VerificationCode
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime ExpiredAt { get; set; }
}
