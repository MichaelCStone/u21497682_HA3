using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using u21497682_HA3.Models;
using System.Data.Entity;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Hosting;

namespace u21497682_HA3.Controllers
{
    public class HomeController : Controller
    {
        private LibraryEntities db = new LibraryEntities();

        public async Task<ActionResult> Home(int studentPage = 1, int studentPageSize = 10, int bookPage = 1, int bookPageSize = 10)
        {
            var students = await db.students
                .OrderBy(s => s.studentId)
                .Skip((studentPage - 1) * studentPageSize)
                .Take(studentPageSize)
                .ToListAsync();

            var books = await db.books
                .OrderBy(b => b.bookId)
                .Skip((bookPage - 1) * bookPageSize)
                .Take(bookPageSize)
                .ToListAsync();

            ViewBag.StudentCurrentPage = studentPage;
            ViewBag.StudentPageSize = studentPageSize;
            ViewBag.StudentTotalPages = (int)Math.Ceiling((double)await db.students.CountAsync() / studentPageSize);

            ViewBag.BookCurrentPage = bookPage;
            ViewBag.BookPageSize = bookPageSize;
            ViewBag.BookTotalPages = (int)Math.Ceiling((double)await db.books.CountAsync() / bookPageSize);

            var viewModel = new Combined
            {
                Students = students,
                Books = books,
                Authors = await db.authors.ToListAsync(),
                Types = await db.types.ToListAsync(),
                Borrows = await db.borrows.ToListAsync(),
            };

            return View(viewModel);
        }

        public async Task<ActionResult> Maintain(int authorPage = 1, int authorPageSize = 10, int typePage = 1, int typePageSize = 10, int borrowPage = 1, int borrowPageSize = 10)
        {
            var authors = await db.authors
                .OrderBy(s => s.authorId)
                .Skip((authorPage - 1) * authorPageSize)
                .Take(authorPageSize)
                .ToListAsync();

            var types = await db.types
                .OrderBy(b => b.typeId)
                .Skip((typePage - 1) * typePageSize)
                .Take(typePageSize)
                .ToListAsync();

            var borrows = await db.borrows
                .OrderBy(b => b.borrowId)
                .Skip((borrowPage - 1) * borrowPageSize)
                .Take(borrowPageSize)
                .ToListAsync();

            ViewBag.AuthorCurrentPage = authorPage;
            ViewBag.AuthorPageSize = authorPageSize;
            ViewBag.AuthorTotalPages = (int)Math.Ceiling((double)await db.authors.CountAsync() / authorPageSize);

            ViewBag.TypeCurrentPage = typePage;
            ViewBag.TypePageSize = typePageSize;
            ViewBag.TypeTotalPages = (int)Math.Ceiling((double)await db.types.CountAsync() / typePageSize);

            ViewBag.BorrowCurrentPage = borrowPage;
            ViewBag.BorrowPageSize = borrowPageSize;
            ViewBag.BorrowTotalPages = (int)Math.Ceiling((double)await db.borrows.CountAsync() / borrowPageSize);

            var viewModel = new Combined
            {
                Students = await db.students.ToListAsync(),
                Books = await db.books.ToListAsync(),
                Authors = authors,
                Types = types,
                Borrows = borrows,
            };

            return View(viewModel);
        }

        public async Task<ActionResult> Report()
        {
            var borrowCounts = await db.borrows
                .GroupBy(b => new { b.bookId, b.books.name })
                .Select(g => new
                {
                    BookId = g.Key.bookId,
                    BookName = g.Key.name,
                    BorrowCount = g.Count()
                })
                .ToListAsync();

            var viewModel = new Combined
            {
                Students = await db.students.ToListAsync(),
                Books = await db.books.ToListAsync(),
                Authors = await db.authors.ToListAsync(),
                Types = await db.types.ToListAsync(),
                Borrows = await db.borrows.ToListAsync(),
            };

            ViewBag.BorrowCounts = borrowCounts;

            var reportDirectory = Server.MapPath("~/Reports");
            var files = Directory.GetFiles(reportDirectory)
                                 .Select(file => new FileInfo(file))
                                 .Select(fileInfo => new ReportFile
                                 {
                                     FileName = fileInfo.Name,
                                     FilePath = $"/Reports/{fileInfo.Name}",
                                     FileSize = fileInfo.Length
                                 })
                                 .ToList();

            ViewBag.SavedReports = files;

            var reportPath = HostingEnvironment.MapPath("~/Reports");
            var reportFiles = Directory.GetFiles(reportPath)
                .Select(Path.GetFileName)
                .ToList();

            ViewBag.ReportFiles = reportFiles;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SaveReport(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var path = Path.Combine(HostingEnvironment.MapPath("~/Reports"), Path.GetFileName(file.FileName));
                file.SaveAs(path);
            }   
            return RedirectToAction("Report");
        }

        public FileResult DownloadReport(string filename)
        {
            var path = Path.Combine(Server.MapPath("~/Reports"), filename);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }

        [HttpPost]
        public ActionResult DeleteReport(string filename)
        {
            var path = Path.Combine(Server.MapPath("~/Reports"), filename);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            return RedirectToAction("Report");
        }



        //AUTHORS CONTROLLER METHODS
        // GET: authors
        public async Task<ActionResult> AuthorsIndex()
        {
            return View(await db.authors.ToListAsync());
        }

        // GET: authors/Details/5
        public async Task<ActionResult> AuthorsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            authors authors = await db.authors.FindAsync(id);
            if (authors == null)
            {
                return HttpNotFound();
            }
            return View(authors);
        }

        // GET: authors/Create
        public ActionResult AuthorsCreate()
        {
            return View();
        }

        // POST: authors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AuthorsCreate([Bind(Include = "authorId,name,surname")] authors authors)
        {
            if (ModelState.IsValid)
            {
                db.authors.Add(authors);
                await db.SaveChangesAsync();
                return RedirectToAction("AuthorsIndex");
            }

            return View(authors);
        }

        // GET: authors/Edit/5
        public async Task<ActionResult> AuthorsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            authors authors = await db.authors.FindAsync(id);
            if (authors == null)
            {
                return HttpNotFound();
            }
            return View(authors);
        }

        // POST: authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AuthorsEdit([Bind(Include = "authorId,name,surname")] authors authors)
        {
            if (ModelState.IsValid)
            {
                db.Entry(authors).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("AuthorsIndex");
            }
            return View(authors);
        }

        // GET: authors/Delete/5
        public async Task<ActionResult> AuthorsDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            authors authors = await db.authors.FindAsync(id);
            if (authors == null)
            {
                return HttpNotFound();
            }
            return View(authors);
        }

        // POST: authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AuthorsDeleteConfirmed(int id)
        {
            authors authors = await db.authors.FindAsync(id);
            db.authors.Remove(authors);
            await db.SaveChangesAsync();
            return RedirectToAction("AuthorsIndex");
        }






        //BOOKS CONTROLLER METHODS
        // GET: books
        public async Task<ActionResult> BooksIndex()
        {
            var books = db.books.Include(b => b.authors).Include(b => b.types);
            return View(await books.ToListAsync());
        }

        // GET: books/Details/5
        public async Task<ActionResult> BooksDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            books books = await db.books.FindAsync(id);
            if (books == null)
            {
                return HttpNotFound();
            }
            return View(books);
        }

        // GET: books/Create
        public ActionResult BooksCreate()
        {
            ViewBag.authorId = new SelectList(db.authors, "authorId", "name");
            ViewBag.typeId = new SelectList(db.types, "typeId", "name");
            return View();
        }

        // POST: books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BooksCreate([Bind(Include = "bookId,name,pagecount,point,authorId,typeId")] books books)
        {
            if (ModelState.IsValid)
            {
                db.books.Add(books);
                await db.SaveChangesAsync();
                return RedirectToAction("BooksIndex");
            }

            ViewBag.authorId = new SelectList(db.authors, "authorId", "name", books.authorId);
            ViewBag.typeId = new SelectList(db.types, "typeId", "name", books.typeId);
            return View(books);
        }

        // GET: books/Edit/5
        public async Task<ActionResult> BooksEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            books books = await db.books.FindAsync(id);
            if (books == null)
            {
                return HttpNotFound();
            }
            ViewBag.authorId = new SelectList(db.authors, "authorId", "name", books.authorId);
            ViewBag.typeId = new SelectList(db.types, "typeId", "name", books.typeId);
            return View(books);
        }

        // POST: books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BooksEdit([Bind(Include = "bookId,name,pagecount,point,authorId,typeId")] books books)
        {
            if (ModelState.IsValid)
            {
                db.Entry(books).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("BooksIndex");
            }
            ViewBag.authorId = new SelectList(db.authors, "authorId", "name", books.authorId);
            ViewBag.typeId = new SelectList(db.types, "typeId", "name", books.typeId);
            return View(books);
        }

        // GET: books/Delete/5
        public async Task<ActionResult> BooksDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            books books = await db.books.FindAsync(id);
            if (books == null)
            {
                return HttpNotFound();
            }
            return View(books);
        }

        // POST: books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BooksDeleteConfirmed(int id)
        {
            books books = await db.books.FindAsync(id);
            db.books.Remove(books);
            await db.SaveChangesAsync();
            return RedirectToAction("BooksIndex");
        }






        //BORROWS CONTROLLER METHODS
        // GET: borrows
        public async Task<ActionResult> BorrowsIndex()
        {
            var borrows = db.borrows.Include(b => b.books).Include(b => b.students);
            return View(await borrows.ToListAsync());
        }

        // GET: borrows/Details/5
        public async Task<ActionResult> BorrowsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            borrows borrows = await db.borrows.FindAsync(id);
            if (borrows == null)
            {
                return HttpNotFound();
            }
            return View(borrows);
        }

        // GET: borrows/Create
        public ActionResult BorrowsCreate()
        {
            ViewBag.bookId = new SelectList(db.books, "bookId", "name");
            ViewBag.studentId = new SelectList(db.students, "studentId", "name");
            return View();
        }

        // POST: borrows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BorrowsCreate([Bind(Include = "borrowId,studentId,bookId,takenDate,broughtDate")] borrows borrows)
        {
            if (ModelState.IsValid)
            {
                db.borrows.Add(borrows);
                await db.SaveChangesAsync();
                return RedirectToAction("BorrowsIndex");
            }

            ViewBag.bookId = new SelectList(db.books, "bookId", "name", borrows.bookId);
            ViewBag.studentId = new SelectList(db.students, "studentId", "name", borrows.studentId);
            return View(borrows);
        }

        // GET: borrows/Edit/5
        public async Task<ActionResult> BorrowsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            borrows borrows = await db.borrows.FindAsync(id);
            if (borrows == null)
            {
                return HttpNotFound();
            }
            ViewBag.bookId = new SelectList(db.books, "bookId", "name", borrows.bookId);
            ViewBag.studentId = new SelectList(db.students, "studentId", "name", borrows.studentId);
            return View(borrows);
        }

        // POST: borrows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BorrowsEdit([Bind(Include = "borrowId,studentId,bookId,takenDate,broughtDate")] borrows borrows)
        {
            if (ModelState.IsValid)
            {
                db.Entry(borrows).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("BorrowsIndex");
            }
            ViewBag.bookId = new SelectList(db.books, "bookId", "name", borrows.bookId);
            ViewBag.studentId = new SelectList(db.students, "studentId", "name", borrows.studentId);
            return View(borrows);
        }

        // GET: borrows/Delete/5
        public async Task<ActionResult> BorrowsDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            borrows borrows = await db.borrows.FindAsync(id);
            if (borrows == null)
            {
                return HttpNotFound();
            }
            return View(borrows);
        }

        // POST: borrows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BorrowsDeleteConfirmed(int id)
        {
            borrows borrows = await db.borrows.FindAsync(id);
            db.borrows.Remove(borrows);
            await db.SaveChangesAsync();
            return RedirectToAction("BorrowsIndex");
        }







        //STUDENTS CONTROLLER METHODS
        private const int PageSize = 10; // Set the number of students per page

        public async Task<ActionResult> StudentList(int page = 1)
        {
            // Fetch all students, books, authors, etc.
            var combinedModel = new Combined
            {
                Students = await db.students.ToListAsync(),
                Books = await db.books.ToListAsync(),
                Authors = await db.authors.ToListAsync(),
                Types = await db.types.ToListAsync(),
                Borrows = await db.borrows.ToListAsync(),
            };

            // Apply pagination
            var pagedStudents = combinedModel.Students
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Pass the paginated list and additional data to the model
            var pagedModel = new Combined
            {
                Students = pagedStudents,
                Books = combinedModel.Books,
                Authors = combinedModel.Authors,
                Types = combinedModel.Types,
                Borrows = combinedModel.Borrows
            };

            // Pass the current page and total pages to the view using ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)combinedModel.Students.Count() / PageSize);

            return View(pagedModel);
        }


        // GET: students
        public async Task<ActionResult> StudentsIndex()
        {
            return View(await db.students.ToListAsync());
        }

        // GET: students/Details/5
        public async Task<ActionResult> StudentsDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            students students = await db.students.FindAsync(id);
            if (students == null)
            {
                return HttpNotFound();
            }
            return View(students);
        }

        // GET: students/Create
        public ActionResult StudentsCreate()
        {
            return View();
        }

        // POST: students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentsCreate([Bind(Include = "studentId,name,surname,birthdate,gender,class,point")] students students)
        {
            if (ModelState.IsValid)
            {
                db.students.Add(students);
                await db.SaveChangesAsync();
                return RedirectToAction("StudentsIndex");
            }

            return View(students);
        }

        // GET: students/Edit/5
        public async Task<ActionResult> StudentsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            students students = await db.students.FindAsync(id);
            if (students == null)
            {
                return HttpNotFound();
            }
            return View(students);
        }

        // POST: students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentsEdit([Bind(Include = "studentId,name,surname,birthdate,gender,class,point")] students students)
        {
            if (ModelState.IsValid)
            {
                db.Entry(students).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("StudentsIndex");
            }
            return View(students);
        }

        // GET: students/Delete/5
        public async Task<ActionResult> StudentsDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            students students = await db.students.FindAsync(id);
            if (students == null)
            {
                return HttpNotFound();
            }
            return View(students);
        }

        // POST: students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentsDeleteConfirmed(int id)
        {
            students students = await db.students.FindAsync(id);
            db.students.Remove(students);
            await db.SaveChangesAsync();
            return RedirectToAction("StudentsIndex");
        }







        //TYPES CONTROLLER METHOD
        // GET: types
        public async Task<ActionResult> TypesIndex()
        {
            return View(await db.types.ToListAsync());
        }

        // GET: types/Details/5
        public async Task<ActionResult> TypesDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            types types = await db.types.FindAsync(id);
            if (types == null)
            {
                return HttpNotFound();
            }
            return View(types);
        }

        // GET: types/Create
        public ActionResult TypesCreate()
        {
            return View();
        }

        // POST: types/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TypesCreate([Bind(Include = "typeId,name")] types types)
        {
            if (ModelState.IsValid)
            {
                db.types.Add(types);
                await db.SaveChangesAsync();
                return RedirectToAction("TypesIndex");
            }

            return View(types);
        }

        // GET: types/Edit/5
        public async Task<ActionResult> TypesEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            types types = await db.types.FindAsync(id);
            if (types == null)
            {
                return HttpNotFound();
            }
            return View(types);
        }

        // POST: types/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TypesEdit([Bind(Include = "typeId,name")] types types)
        {
            if (ModelState.IsValid)
            {
                db.Entry(types).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("TypesIndex");
            }
            return View(types);
        }

        // GET: types/Delete/5
        public async Task<ActionResult> TypesDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            types types = await db.types.FindAsync(id);
            if (types == null)
            {
                return HttpNotFound();
            }
            return View(types);
        }

        // POST: types/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TypesDeleteConfirmed(int id)
        {
            types types = await db.types.FindAsync(id);
            db.types.Remove(types);
            await db.SaveChangesAsync();
            return RedirectToAction("TypesIndex");
        }
    }
}