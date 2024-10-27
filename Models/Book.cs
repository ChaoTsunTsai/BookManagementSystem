using System.ComponentModel.DataAnnotations;

namespace BookManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Author { get; set; }

        public bool IsBorrowed { get; set; }

        // 記錄借閱者 ID
        public int? BorrowedByUserId { get; set; }

        // 導覽屬性以存取借閱者詳細信息
        public User BorrowedByUser { get; set; }
    }
}