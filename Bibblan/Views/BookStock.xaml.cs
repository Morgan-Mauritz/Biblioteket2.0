﻿using System;
using System.Collections.Generic;
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
using Bibblan.Models;
using Bibblan.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Bibblan.Views
{
    /// <summary>
    /// Interaction logic for BookStock.xaml
    /// </summary>
    public partial class BookStock : Page
    {
        List<Stock> dbVirtual = new List<Stock>();
        List<Stock> defaultStocks = new List<Stock>();
        Stock selectedStock = new Stock();
        public BookStock()
        {
            InitializeComponent();

            foreach (var item in DbInitialiser.Db.Stocks)
            {
                dbVirtual.Add(item);
            }
            defaultStocks = dbVirtual.Where(x => x.Isbn.ToString() == GlobalClass.chosenBook.Isbn.ToString()).ToList();

            searchBar.Text = "Sök";
        }

        private void removeBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (LVBookStock.SelectedItem != null)
            {
                if(commentComboBox.SelectedItem != null)
                {
                    ComboBoxItem commentSelected = (ComboBoxItem)commentComboBox.SelectedItem;

                    var dbStockItem = DbInitialiser.Db.Stocks.Where(x => x.StockId == selectedStock.StockId).SingleOrDefault();
                    dbStockItem.Comment = commentSelected.Content.ToString();
                    dbStockItem.Discarded = 1;
                    DbInitialiser.Db.SaveChanges();

                    MessageBox.Show("Bok utrangerad!");
                }
                else
                {
                    MessageBox.Show("Lägg till kommentar angående varför exemplaret utrangeras!");
                }
            }
            else
            {
                MessageBox.Show("Välj ett exemplar!");
            }
        }

        private void addBooksButton_Click(object sender, RoutedEventArgs e)
        {
            if(LVBookStock.SelectedItem != null && amountBox.Text != "")
            {
                if(Regex.IsMatch(amountBox.Text, @"^([0-9])$"))
                {
                    for(int i = 0; i < Convert.ToInt32(amountBox.Text); i++)
                    {
                        Stock dbInput = new Stock() { Isbn = selectedStock.Isbn, Condition = "Nyskick", Discarded = 0 };
                        DbInitialiser.Db.Stocks.Add(dbInput);
                        DbInitialiser.Db.SaveChanges();
                    }
                    MessageBox.Show($"{amountBox.Text} böcker tillagda!");
                }
                else
                {
                    MessageBox.Show("Vänligen sätt en mängd i siffror");
                }
            }
            else 
            {
                MessageBox.Show("Välj en bok!");
            }
        }

        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchBar.Text.Length >= 2 && searchBar.Text != "Sök")
            {
                LVBookStock.ClearValue(ItemsControl.ItemsSourceProperty);
                List<Stock>? showStocks = new List<Stock>();
                showStocks = defaultStocks.Where(x => x.Condition.ToLower().Contains(searchBar.Text.ToLower())).DefaultIfEmpty().ToList();
                
                LVBookStock.ItemsSource = showStocks;
            } else
            {
                LVBookStock.ClearValue(ItemsControl.ItemsSourceProperty);

                LVBookStock.ItemsSource = defaultStocks;
            }

            //if (searchBar.Text == null || searchBar.Text == "")
            //{
            //    LVBookStock.ClearValue(ItemsControl.ItemsSourceProperty);
                
            //    LVBookStock.ItemsSource = defaultStocks;
            //}
        }

        private void LVBookStock_Selected(object sender, RoutedEventArgs e)
        {
            if(LVBookStock.SelectedItem != null)
            {
                selectedStock = LVBookStock.SelectedItem as Stock;
                isbnBox.Foreground = Brushes.Black;
                isbnBox.Text = selectedStock.Isbn.ToString();
                stockIdBox.Foreground = Brushes.Black;
                stockIdBox.Text = selectedStock.StockId.ToString();
                if(selectedStock.Comment != null)
                {
                    commentComboBox.Text = selectedStock.Comment.ToString();
                }
                if(selectedStock.Condition != null)
                {
                    conditionComboBox.Text = selectedStock.Condition.ToString();
                }
            }
            else
            {
                isbnBox.Foreground = Brushes.LightGray;
                isbnBox.Text = "";
                stockIdBox.Foreground = Brushes.LightGray;
                stockIdBox.Text = "";
            }
        }

        private void conditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(selectedStock != null && conditionComboBox.SelectedItem != null)
            {
                var dbStockItem = DbInitialiser.Db.Stocks.Where(x => x.StockId == selectedStock.StockId).SingleOrDefault();
                ComboBoxItem comboBoxSelection = (ComboBoxItem)conditionComboBox.SelectedItem;
                dbStockItem.Condition = comboBoxSelection.Content.ToString();
                DbInitialiser.Db.SaveChanges();
            }
        }

        private void searchBar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBar.Foreground == Brushes.LightGray)
            {
                searchBar.Foreground = Brushes.Black;
                searchBar.Text = "";
            }
        }

        private void searchBar_LostFocus(object sender, RoutedEventArgs e)
        {
            if(searchBar.Text == "" || searchBar.Text == null)
            {
                searchBar.Foreground = Brushes.LightGray;
                searchBar.Text = "Sök";
            }
        }
    }
}