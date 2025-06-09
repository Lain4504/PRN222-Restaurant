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
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalTables + PageSize - 1) / PageSize;

        private readonly ITableService _tableService;
        public TablesModel(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page < 1 ? 1 : page;
            var (tables, totalCount) = await _tableService.GetPagedAsync(CurrentPage, PageSize);
            TotalTables = totalCount;
            Tables = tables.Select(t => new TableViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Status = t.Status
            }).ToList();
            Console.WriteLine($"PageSize: {PageSize}, CurrentPage: {CurrentPage}");

        }

        public async Task<IActionResult> OnPostDeleteAsync(int tableId)
        {
            await _tableService.DeleteAsync(tableId);
            SuccessMessage = $"Bàn số {tableId} đã được xóa thành công";
            return RedirectToPage();
        }

        // Thêm mới bàn ăn (persist to DB)
        public async Task<IActionResult> OnPostAddAsync(TableViewModel newTable, int page = 1)
        {
            // Kiểm tra số bàn không được để trống hoặc <= 0
            if (newTable == null || newTable.TableNumber <= 0)
            {
                SuccessMessage = "Số bàn không hợp lệ.";
                return RedirectToPage();
            }
            // Kiểm tra sức chứa không được để trống hoặc <= 0
            if (newTable.Capacity <= 0)
            {
                SuccessMessage = "Sức chứa không hợp lệ.";
                return RedirectToPage();
            }
            // Kiểm tra số bàn không được trùng
            var allTables = await _tableService.GetAllAsync();
            if (allTables.Any(t => t.TableNumber == newTable.TableNumber))
            {
                SuccessMessage = $"Số bàn {newTable.TableNumber} đã tồn tại.";
                return RedirectToPage();
            }
           
            // Khi thêm mới, status luôn là "Available"
            var table = new Table
            {
                Id = newTable.Id > 0 ? newTable.Id : 0, // Nếu có nhập ID thủ công
                TableNumber = newTable.TableNumber,
                Capacity = newTable.Capacity,
                Status = "Available"
            };
            await _tableService.AddAsync(table);
            SuccessMessage = $"Đã thêm bàn số {newTable.TableNumber} thành công";
            return RedirectToPage();
        }

        // Sửa thông tin bàn ăn (mock)
        public async Task<IActionResult> OnPostEditAsync(TableViewModel editTable)
        {
            // Lấy bàn từ DB
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
                table.Status = editTable.Status;
                await _tableService.UpdateAsync(table);
                SuccessMessage = $"Đã cập nhật bàn số {editTable.TableNumber} thành công";
            }
            else
            {
                SuccessMessage = $"Không tìm thấy bàn để cập nhật.";
            }
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
