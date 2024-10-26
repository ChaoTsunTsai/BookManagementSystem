using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookManagementSystem.Data;
using BookManagementSystem.Models;
using System.Linq;

namespace BookManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 檢查是否為管理員的方法
        private bool IsAdmin()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value == "True";
        }

        // 顯示新增書籍頁面
        [HttpGet]
        [Authorize] // 需要登入
        public IActionResult Create()
        {
            // 檢查是否為管理員
            if (IsAdmin())
            {
                return View();
            }

            return Forbid(); // 如果不是管理員，返回禁止訪問的響應
        }

        // 處理新增書籍請求
        [HttpPost]
        [Authorize]
        public IActionResult Create(Book book)
        {
            // 檢查是否為管理員
            if (IsAdmin())
            {
                if (ModelState.IsValid)
                {
                    _context.Books.Add(book);
                    _context.SaveChanges();
                    return RedirectToAction("ManageBooks");
                }
                return View(book);
            }

            return Forbid();
        }

        // 顯示編輯書籍頁面
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id)
        {
            // 檢查是否為管理員
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // 處理編輯書籍的請求
        [HttpPost]
        [Authorize]
        public IActionResult Edit(Book book)
        {
            // 檢查是否為管理員
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Update(book);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }
            return View(book);
        }

        // 刪除書籍
        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // 檢查是否為管理員
            if (!IsAdmin())
            {
                return Forbid();
            }

            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            _context.SaveChanges();
            return RedirectToAction("ManageBooks");
        }

        // 顯示所有書籍的管理頁面
        [Authorize]
        public IActionResult ManageBooks()
        {
            // 檢查是否為管理員
            if (!IsAdmin())
            {
                return Forbid();
            }

            // 取得所有書籍
            var books = _context.Books.ToList();

            // 傳遞書籍列表給視圖
            return View(books);
        }

        // 顯示借閱書籍頁面
        [Authorize]
        public IActionResult BorrowBook()
        {
            // 取得所有書籍
            var books = _context.Books.ToList();

            // 傳遞書籍列表給視圖
            return View(books);
        }

        // 處理借閱書籍請求
        [Authorize]
        [HttpPost]
        public IActionResult BorrowBook(List<int> bookIds)
        {
            if (bookIds != null && bookIds.Any())
            {
                foreach (var bookId in bookIds)
                {
                    var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
                    if (book != null && !book.IsBorrowed)
                    {
                        book.IsBorrowed = true;
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction("BorrowBook");
        }

        // 顯示歸還書籍頁面
        [Authorize]
        public IActionResult ReturnBook()
        {
            var borrowedBooks = _context.Books.Where(b => b.IsBorrowed).ToList(); // 僅顯示已借閱的書籍
            return View(borrowedBooks);
        }

        // 處理歸還書籍的請求
        [HttpPost]
        [Authorize]
        public IActionResult ReturnBooks(List<int> bookIds)
        {
            var booksToReturn = _context.Books.Where(b => bookIds.Contains(b.Id) && b.IsBorrowed).ToList();

            foreach (var book in booksToReturn)
            {
                book.IsBorrowed = false; // 將借閱狀態設為可用
            }

            _context.SaveChanges();
            return RedirectToAction("ReturnBook"); // 重新載入頁面，確認歸還狀態
        }
    }
}