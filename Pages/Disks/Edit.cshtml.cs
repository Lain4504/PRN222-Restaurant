using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages.Disks;

public class EditModel : PageModel
{
    private readonly IDiskService _diskService;

    public EditModel(IDiskService diskService)
    {
        _diskService = diskService;
    }

    [BindProperty]
    public Disk Disk { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var disk = await _diskService.GetDiskByIdAsync(id.Value);
        if (disk == null)
        {
            return NotFound();
        }

        Disk = disk;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _diskService.UpdateDiskAsync(Disk.Id, Disk);
        if (result == null)
        {
            return NotFound();
        }

        return RedirectToPage("./Index");
    }
} 