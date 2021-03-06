using System;
using Bibblan.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bibblan.Models;
using System.Linq;
using System.Collections.Generic; 


namespace BibblanTest
{    
    public class TestData
    {
        public void SetupTestDb() 
        {
            BiblioteketContext.testConnectionString = "Server = tcp:bladerunnerdb.database.windows.net,1433; Initial Catalog = Biblioteket_Kopiera; Persist Security Info = False; User ID = harrison; " +
            "Password = Blade1234; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30";

            DbInitialiser.InitialiseDB(); 
        }
    }

    [TestClass]
    public class BookTests
    {
        TestData td = new TestData();

        [TestMethod]
        public void AddBookTest()
        {
            //Arrange
            td.SetupTestDb();

            bool BookEquals(Book actual, Book other)   //Metod som kollar om b?ckerna ?r 'likadana', v?r tidigare override i Equals i Booksmodellen skapade nya problem med grundl?ggande funktioner 
            {
                if (actual.Title == other.Title && actual.Author == other.Author && actual.Description == other.Description && actual.Category == other.Category)
                    return true;

                else return false;
            }

            string title = "Flugornas Herre";
            string author = "William Golding";
            string description = "Efter en flygkrasch uppt?cker en grupp engelska skolpojkar(6 - 12 ?r) att de befinner sig p? en ?de ? i Stilla havet." +
                "Pojkarna har evakuerats fr?n England p? grund av ett k?rnvapenkrig.Ingen vuxen finns p? ?n och pojkarna organiserar sig s? gott de kan, glada ?ver sin nyvunna frihet.";
            string edition = "1959";
            string price = "10";
            string ddk = "800";
            string sab = "H";
            string publisher = "Faber and Faber";
            int isEbook = 0;

            Book actual = new Book() { Title = title, Author = author, Description = description, Edition = int.Parse(edition), Price = int.Parse(price), Ddk = int.Parse(ddk), Sab = sab, Publisher = publisher, Category = isEbook };

            //Act
            Book bookToCheck = BookService.AddBook(title, author, description, edition, price, ddk, sab, publisher, isEbook); //Checkar om vi f?tt in samma bok, b?de lokalt och i databasen
            Book bookToCheckFromDb = DbInitialiser.Db.Books.OrderBy(x => x.Isbn).Last();

            Book bookToFail = BookService.AddBook("Inte alls namnet p? Flugornas Herre", author, description, edition, price, ddk, sab, publisher, isEbook); //Checkar om vi f?tt in samma bok, b?de lokalt och i databasen
            Book bookToFailFromDb = DbInitialiser.Db.Books.OrderBy(x => x.Isbn).Last();

            //Assert
            Assert.IsTrue(BookEquals(actual, bookToCheck));
            Assert.IsTrue(BookEquals(actual, bookToCheckFromDb));
            Assert.IsFalse(BookEquals(actual, bookToFail));
            Assert.IsFalse(BookEquals(actual, bookToFailFromDb));

            //Cleanup
            DbInitialiser.Db.Books.Remove(bookToCheck); // Tar bort b?ckerna fr?n testdatabasen n?r testet ?r klart.
            DbInitialiser.Db.Books.Remove(bookToFail);
            DbInitialiser.Db.SaveChanges();
        }

        [TestMethod]
        public void AddStockBookTest()
        {
            //Arrange
            td.SetupTestDb();

            bool StockEquals(Book expected, Stock actual)   //Metod som kollar om b?ckerna ?r 'likadana', v?r tidigare override i Equals i Booksmodellen skapade nya problem med grundl?ggande funktioner 
            {
                if (expected.Isbn == actual.Isbn)
                    return true;

                else return false;
            }

            string title = "Flugornas Herre";
            string author = "William Golding";
            string description = "Efter en flygkrasch uppt?cker en grupp engelska skolpojkar(6 - 12 ?r) att de befinner sig p? en ?de ? i Stilla havet." +
                "Pojkarna har evakuerats fr?n England p? grund av ett k?rnvapenkrig.Ingen vuxen finns p? ?n och pojkarna organiserar sig s? gott de kan, glada ?ver sin nyvunna frihet.";
            string edition = "1959";
            string price = "10";
            string ddk = "800";
            string sab = "H";
            string publisher = "Faber and Faber";
            int isEbook = 0;
            int amount = 1;

            Book bookToAddToStock = BookService.AddBook(title, author, description, edition, price, ddk, sab, publisher, isEbook);

            //Act
            BookService.AddStockBook(bookToAddToStock, amount);
            Book isbnBook = DbInitialiser.Db.Books.Where(x => x.Title == bookToAddToStock.Title && x.Edition == bookToAddToStock.Edition && x.Category == bookToAddToStock.Category).FirstOrDefault();
            Stock expected = DbInitialiser.Db.Stocks.Where(x => x.Isbn == isbnBook.Isbn).FirstOrDefault();
            Stock expectedFalse = new Stock() { StockId = expected.StockId, Isbn = 7 }; 

            //Assert
            Assert.IsTrue(StockEquals(isbnBook, expected));
            Assert.IsFalse(StockEquals(isbnBook, expectedFalse)); 

            //Cleanup
            var stocksToRemove = DbInitialiser.Db.Stocks.Where(x => x.Isbn == isbnBook.Isbn).ToList();

            foreach (var item in stocksToRemove)
            {
                DbInitialiser.Db.Stocks.Remove(item);
            }

            DbInitialiser.Db.Books.Remove(isbnBook);
            DbInitialiser.Db.SaveChanges();

        }

        [TestMethod]
        public void AddLoanLogTest()
        {
            //Arrange
            td.SetupTestDb();

            bool LoanLogEquals(Loanlog expected, Loanlog actual)   //Metod som kollar om LoanLogsen ?r 'likadana', v?r tidigare override i Equals i Booksmodellen skapade nya problem med grundl?ggande funktioner 
            {
                if (expected.StockId == actual.StockId && expected.UserId == actual.UserId)
                    return true;

                else return false;
            }

            //Act
            string title = "Flugornas Herre";
            string author = "William Golding";
            string description = "Efter en flygkrasch uppt?cker en grupp engelska skolpojkar(6 - 12 ?r) att de befinner sig p? en ?de ? i Stilla havet." +
                "Pojkarna har evakuerats fr?n England p? grund av ett k?rnvapenkrig.Ingen vuxen finns p? ?n och pojkarna organiserar sig s? gott de kan, glada ?ver sin nyvunna frihet.";
            string edition = "1959";
            string price = "10";
            string ddk = "800";
            string sab = "H";
            string publisher = "Faber and Faber";
            int isEbook = 0;
            int amount = 1;
            int userId = 67; 

           
            Book bookToAddToStock = BookService.AddBook(title, author, description, edition, price, ddk, sab, publisher, isEbook);

            BookService.AddStockBook(bookToAddToStock, amount);

            Book isbnBook = DbInitialiser.Db.Books.Where(x => x.Title == bookToAddToStock.Title && x.Edition == bookToAddToStock.Edition && x.Category == bookToAddToStock.Category).FirstOrDefault();
            List<Stock> stockForBook = DbInitialiser.Db.Stocks.Where(x => x.Isbn == isbnBook.Isbn).ToList(); 

            Loanlog loanLog = new Loanlog() { StockId = stockForBook.First().StockId, UserId = userId, Loandate = DateTime.Now, Returndate = DateTime.Now.AddMonths(1) };
            Loanlog loanLogFalse = new Loanlog() { StockId = stockForBook.First().StockId, UserId = 17, Loandate = DateTime.Now, Returndate = DateTime.Now.AddMonths(1) };

            BookService.AddLoanlog(stockForBook.First().StockId, userId, DateTime.Now, DateTime.Now.AddMonths(1));

            var LoanlogToCheck = DbInitialiser.Db.Loanlogs.Where(x => x.UserId == userId).FirstOrDefault();

            //Assert
            Assert.IsTrue(LoanLogEquals(LoanlogToCheck, loanLog));
            Assert.IsFalse(LoanLogEquals(LoanlogToCheck, loanLogFalse)); 


            DbInitialiser.Db.Loanlogs.Remove(LoanlogToCheck);

            foreach (var item in stockForBook)
            {
                DbInitialiser.Db.Stocks.Remove(item);
            }
            DbInitialiser.Db.Books.Remove(isbnBook);
            DbInitialiser.Db.SaveChanges();
        }


    }

        






}
