using Library.Entities.Models;
using Library.Shared.DTOs.BorrowRecord;

namespace Library.Services.Interfaces
{
    public interface IBorrowService
    {
        IQueryable<BorrowListDto> GetBorrowDetailsQuery();
        Task<bool> HasAvailableCopyAsync(int bookId);
        IQueryable<InventoryRecord> GetAvailableCopiesQuery(int bookId);
        Task<BorrowResponseDto> BorrowBookAsync(RequestBorrowDto dto, int userId);
        Task<bool> ReturnBookAsync(int borrowRecordId, int currentUserId);
        IQueryable<BorrowRecord> GetOverdueRecordsQuery();
        bool IsBorrowOverdue(BorrowRecord record);
        int CalculateOverdueDays(BorrowRecord record);

    }
}
