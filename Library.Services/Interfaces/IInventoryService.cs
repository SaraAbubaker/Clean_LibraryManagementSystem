using Library.Entities.Models;

namespace Library.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> ReturnCopyAsync(int inventoryRecordId, int currentUserId);
        Task<InventoryRecord> CreateCopyAsync(int bookId, int createdByUserId);
        Task<bool> ArchiveCopyAsync(int inventoryRecordId, int performedByUserId);
        IQueryable<InventoryRecord> ListCopiesForBookQuery(int bookId);
        IQueryable<InventoryRecord> GetAvailableCopiesQuery(int bookId);
    }
}
