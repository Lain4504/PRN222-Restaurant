using Microsoft.AspNetCore.Http;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;

namespace PRN222_Restaurant.Services.Service
{
    public class ReservationSessionService : IReservationSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string RESERVATION_SESSION_KEY = "ReservationData";

        public ReservationSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SaveReservationData(ReservationSessionData data)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var json = JsonSerializer.Serialize(data);
                session.SetString(RESERVATION_SESSION_KEY, json);
            }
        }

        public ReservationSessionData? GetReservationData()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var json = session.GetString(RESERVATION_SESSION_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<ReservationSessionData>(json);
                }
            }
            return null;
        }

        public void ClearReservationData()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(RESERVATION_SESSION_KEY);
            }
        }
    }
}
