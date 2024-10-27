using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookManagementSystem.Data;
using BookManagementSystem.Models;
using System.Linq;
using System.Security.Claims;

namespace BookManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value == "True";
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            if (IsAdmin())
            {
                return View();
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(Book book)
        {
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

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id)
        {
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

        [HttpPost]
        [Authorize]
        public IActionResult Edit(Book book)
        {
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

        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
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

        [Authorize]
        public IActionResult ManageBooks()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }
            var books = _context.Books.ToList();
            return View(books);
        }

        [Authorize]
        public IActionResult BorrowBook()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        [Authorize]
        [HttpPost]
        public IActionResult BorrowBook(List<int> bookIds)
        {
            if (bookIds != null && bookIds.Any())
            {
                // 確認用戶已經登入並且擁有 NameIdentifier
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    // 若未找到 NameIdentifier，返回錯誤信息
                    return Unauthorized("User ID not found. Please make sure you are logged in.");
                }

                var userId = int.Parse(userIdClaim.Value);

                foreach (var bookId in bookIds)
                {
                    var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
                    if (book != null && !book.IsBorrowed)
                    {
                        book.IsBorrowed = true;
                        book.BorrowedByUserId = userId; // 記錄借閱者的 ID
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction("BorrowBook");
        }

        [Authorize]
        public IActionResult ReturnBook()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var borrowedBooks = _context.Books.Where(b => b.IsBorrowed && b.BorrowedByUserId == userId).ToList();
            return View(borrowedBooks);
        }

        [HttpPost]
        [Authorize]
        public IActionResult ReturnBooks(List<int> bookIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var booksToReturn = _context.Books
                .Where(b => bookIds.Contains(b.Id) && b.IsBorrowed && b.BorrowedByUserId == userId)
                .ToList();

            foreach (var book in booksToReturn)
            {
                book.IsBorrowed = false;
                book.BorrowedByUserId = null;
            }

            _context.SaveChanges();
            return RedirectToAction("ReturnBook");
        }
    }
}