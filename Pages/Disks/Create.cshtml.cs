using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages.Disks;

public class CreateModel : PageModel
{
    private readonly IDiskService _diskService;

    public CreateModel(IDiskService diskService)
    {
        _diskService = diskService;
    }

    [BindProperty]
    public Disk Disk { get; set; } = default!;

    public void OnGet()
    {
        Disk = new Disk { IsAvailable = true };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _diskService.CreateDiskAsync(Disk);
        return RedirectToPage("./Index");
    }
} 