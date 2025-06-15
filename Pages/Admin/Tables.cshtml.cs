using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class TablesModel : PageModel
    {
        [TempData]
        public string? SuccessMessage { get; set; }

        public List<TableViewModel> Tables { get; set; } = new List<TableViewModel>();
        public int TotalTables { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalTables + PageSize - 1) / PageSize;
        public int FromRecord => ((CurrentPage - 1) * PageSize) + 1;
        public int ToRecord => System.Math.Min(CurrentPage * PageSize, TotalTables);

        private readonly ITableService _tableService;
        public TablesModel(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task OnGetAsync()
        {
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 10;

            var allTables = await _tableService.GetAllAsync();
            TotalTables = allTables.Count();

            Tables = allTables
                .OrderBy(t => t.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new TableViewModel
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    Status = t.Status
                }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int tableId)
        {
            await _tableService.DeleteAsync(tableId);
            SuccessMessage = $"Bàn số {tableId} đã được xóa thành công";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddAsync(TableViewModel newTable, int page = 1)
        {
            if (newTable == null || newTable.TableNumber <= 0)
            {
                SuccessMessage = "Số bàn không hợp lệ.";
                return RedirectToPage();
            }
            if (newTable.Capacity <= 0)
            {
                SuccessMessage = "Sức chứa không hợp lệ.";
                return RedirectToPage();
            }
            var allTables = await _tableService.GetAllAsync();
            if (allTables.Any(t => t.TableNumber == newTable.TableNumber))
            {
                SuccessMessage = $"Số bàn {newTable.TableNumber} đã tồn tại.";
                return RedirectToPage();
            }
            var table = new Table
            {
                Id = newTable.Id > 0 ? newTable.Id : 0,
                TableNumber = newTable.TableNumber,
                Capacity = newTable.Capacity,
                Status = "Available"
            };
            await _tableService.AddAsync(table);
            SuccessMessage = $"Đã thêm bàn số {newTable.TableNumber} thành công";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(TableViewModel editTable)
        {
            var table = await _tableService.GetByIdAsync(editTable.Id);
            if (table != null)
            {
                var allTables = await _tableService.GetAllAsync();
                if (editTable.TableNumber > 0 && allTables.Any(t => t.TableNumber == editTable.TableNumber && t.Id != editTable.Id))
                {
                    SuccessMessage = $"Số bàn {editTable.TableNumber} đã tồn tại.";
                    return RedirectToPage();
                }
                table.TableNumber = editTable.TableNumber;
                table.Capacity = editTable.Capacity;
            }
            else
            {
                SuccessMessage = "Không tìm thấy bàn để cập nhật.";
                return RedirectToPage();
            }
            await _tableService.UpdateAsync(table);
            SuccessMessage = $"Đã cập nhật bàn số {editTable.TableNumber} thành công";
            return RedirectToPage();
        }
    }

    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}