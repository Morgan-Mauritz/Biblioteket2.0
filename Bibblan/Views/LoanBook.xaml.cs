using Bibblan.Models;
using Bibblan.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bibblan.Views
{
    /// <summary>
    /// Interaction logic for LoanBook.xaml
    /// </summary>
    public partial class LoanBook : Page
    {
        BookStockLoan b = new BookStockLoan();  
        List<BookStockLoan> virtualBooksToLoan = new List<BookStockLoan>();
        public LoanBook()
        {            
            InitializeComponent();
            ClearAndRetrieveVirtualDb();
            Validation();

            LVLoanBook.ItemsSource = virtualBooksToLoan;

            descriptionBox.IsReadOnly = true;
        }
        public void Validation()
        {
            if (GlobalClass.loanPermission == 0 && GlobalClass.userPermission == 0) //Gömmer för ordinarie användare utan lånekort
            {
                loanButton.Visibility = Visibility.Collapsed; //Låna knappen göms
            }
        }
        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Searchfunction();
        }
        private void Searchfunction()
        {
            LVLoanBook.ClearValue(ItemsControl.ItemsSourceProperty);

            if(Int32.TryParse(searchBar.Text, out var _) == false)
            {
                List<BookStockLoan> bookList = virtualBooksToLoan.Where(x => x.Title.ToLower().Contains(searchBar.Text.ToLower())
                                                    || x.Author.ToLower().Contains(searchBar.Text.ToLower())).DefaultIfEmpty().ToList();
                LVLoanBook.ItemsSource = bookList;
                return;
            }
            else if (Int32.TryParse(searchBar.Text, out var _) == true) //kollar om userInput är en int eller ej
            {
                List<BookStockLoan> query;
                if (searchBar.Text.Length == 1)
                {
                    query = virtualBooksToLoan.Where(x => x.Category.ToString() == searchBar.Text).DefaultIfEmpty().ToList();
                    LVLoanBook.ItemsSource = query;
                    return;
                }
                else
                {
                    query = virtualBooksToLoan.Where(x => x.Isbn.ToString().Contains(searchBar.Text)).DefaultIfEmpty().ToList();
                    LVLoanBook.ItemsSource = query;
                    return;
                }
            }
        }
        private void LVLoanBook_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (LVLoanBook.SelectedItem != null)
            {
                b = LVLoanBook.SelectedItem as BookStockLoan;

                descriptionBox.Text = b.Description;

            }
        }
        private void loanButton_Click(object sender, RoutedEventArgs e)
        {
            if (LVLoanBook.SelectedItem == null)
            {
                MessageBox.Show("Du måste välja en bok först!", "Meddelande", MessageBoxButton.OK);
                return;
            }
            else
            if (GlobalClass.userPermission < 0) { MessageBox.Show("Du har inte behörighet att göra detta", "Meddelande", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
            

            User query = DbInitialiser.Db.Users.Where(x => x.UserId == GlobalClass.currentUserID).FirstOrDefault();
            if (query.HasLoanCard == 1)
            {
                Loanlog loanLog = new Loanlog();
                BookStockLoan b = LVLoanBook.SelectedItem as BookStockLoan;

                var bookToLoan = DbInitialiser.Db.Stocks.Where(x => x.Isbn == b.Isbn && x.Available != 0).FirstOrDefault();
                

                if (bookToLoan== null)
                {
                    MessageBox.Show("Boken du vill låna är inte tillgänglig för tillfället", "Meddelande", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Är det säkert att du vill låna den här boken?", "Meddelande", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            {
                                loanLog = BookService.AddLoanlog(bookToLoan.StockId, (int)GlobalClass.currentUserID, DateTime.Now.Date, DateTime.Now.AddMonths(1)); //Skapar upp ny loanlog och populerar den

                                bookToLoan.Available = 0; // sätter tillgänglighet på aktuell bok till 'ej tillänglig'

                                DbInitialiser.Db.Stocks.Update(bookToLoan);
                                DbInitialiser.Db.SaveChanges(); // sparar databasen

                                ClearAndRetrieveVirtualDb();
                                LVLoanBook.ClearValue(ItemsControl.ItemsSourceProperty);
                                LVLoanBook.ItemsSource = virtualBooksToLoan;
                                MessageBox.Show($"Du har nu lånat {b.Title}.\nDatum för återlämning är {loanLog.Returndate}", "Meddelande", MessageBoxButton.OK);
                            }
                            break;
                        case MessageBoxResult.No:
                        return;
                    }
                }
                return;
            }
            else
            {
                MessageBox.Show("Du har inga låneprivilegier på ditt lånekort för tillfället. Kontakta bibliotekarie.", "Meddelande", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


           
        }
        public void ClearAndRetrieveVirtualDb()
        {
            virtualBooksToLoan.Clear();

            string titleTemp = "";
            int? availableTemp = -1;
            int counter = 0;

            foreach (var item in DbInitialiser.Db.BookStockLoans)
            {
                if (item.Title == titleTemp && item.Available == availableTemp)
                {
                    continue;
                }
                if (item.Title == titleTemp && item.Available != 0)
                {
                    virtualBooksToLoan.Remove(virtualBooksToLoan[counter - 1]);
                    virtualBooksToLoan.Add(item);
                    titleTemp = item.Title;
                    availableTemp = item.Available;
                   
                    continue;
                }
                if (item.Title == titleTemp && availableTemp == 1)
                {
                    continue;
                }
                if(item.Available == null)
                {
                    continue; 
                }

                virtualBooksToLoan.Add(item);
                titleTemp = item.Title;
                availableTemp = item.Available;
                counter++;
            }
        }
    }
}
