using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Shared.DTOs.BorrowRecord
{
    public class BorrowResponseDto
    {
        public int BorrowRecordId { get; set; }
        public int InventoryRecordId { get; set; }
        public DateOnly BorrowDate { get; set; }
        public DateOnly DueDate { get; set; }
    }
}
