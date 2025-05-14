using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages.Disks;

public class IndexModel : PageModel
{
    private readonly IDiskService _diskService;

    public IndexModel(IDiskService diskService)
    {
        _diskService = diskService;
    }

    public IList<Disk> Disks { get; set; } = default!;

    public async Task OnGetAsync()
    {
        var disks = await _diskService.GetAllDisksAsync();
        Disks = disks.ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _diskService.DeleteDiskAsync(id);
        return RedirectToPage();
    }
} 