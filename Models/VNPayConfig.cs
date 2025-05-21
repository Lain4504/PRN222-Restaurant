using System;

namespace PRN222_Restaurant.Models
{
    public class VNPayConfig
    {
        public static string ConfigName => "Vnpay";
        public string Version { get; set; } = "2.1.0";
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string PaymentUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } = "other";
        public string Locale { get; set; } = "vn";
    }
} 