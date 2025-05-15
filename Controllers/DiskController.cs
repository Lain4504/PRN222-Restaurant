using Microsoft.AspNetCore.Mvc;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiskController : ControllerBase
{
    private readonly IDiskService _diskService;

    public DiskController(IDiskService diskService)
    {
        _diskService = diskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Disk>>> GetDisks()
    {
        var disks = await _diskService.GetAllDisksAsync();
        return Ok(disks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Disk>> GetDisk(int id)
    {
        var disk = await _diskService.GetDiskByIdAsync(id);
        if (disk == null) return NotFound();
        return Ok(disk);
    }

    [HttpPost]
    public async Task<ActionResult<Disk>> CreateDisk(Disk disk)
    {
        var createdDisk = await _diskService.CreateDiskAsync(disk);
        return CreatedAtAction(nameof(GetDisk), new { id = createdDisk.Id }, createdDisk);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDisk(int id, Disk disk)
    {
        if (id != disk.Id) return BadRequest();
        var updatedDisk = await _diskService.UpdateDiskAsync(disk);
        if (updatedDisk == null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDisk(int id)
    {
        var result = await _diskService.DeleteDiskAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
} 