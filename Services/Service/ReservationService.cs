using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;

    public ReservationService(IReservationRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Reservation>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Reservation?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task AddAsync(Reservation reservation) => _repository.AddAsync(reservation);

    public Task UpdateAsync(Reservation reservation) => _repository.UpdateAsync(reservation);

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    public Task ConfirmAsync(int id) => _repository.ConfirmAsync(id);


    
}
