using System;

namespace PRN222_Restaurant.Services.IService
{
    public class ReservationSessionData
    {
        public int TableId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string? Note { get; set; }
        public string? SelectedItems { get; set; }
    }

    public interface IReservationSessionService
    {
        void SaveReservationData(ReservationSessionData data);
        ReservationSessionData? GetReservationData();
        void ClearReservationData();
    }
}
