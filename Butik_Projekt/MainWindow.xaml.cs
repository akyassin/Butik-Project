using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Butik_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class BasketItem
    {
        public string Name;
        public int TotalAmount;
        public int TotalPrice;
        public bool Checked;
    }
    public class BuyedProduct
    {
        public string Name;
        public int Amount;
        public int Price;
        public int CurrentStock;
    }
    public class DiscountCopounes
    {
        public string Code;
        public double Percentage;
    }
    public partial class MainWindow : Window
    {
        public Grid grid = new Grid();

        public WrapPanel productWrapPanel;
        public StackPanel varukorgWrapPanel;

        public Button finish;
        public Label totalPrice;
        public Label currentStock;
        public TextBox discountText;

        public double total;
        public string savedBasket = @"C:\Windows\Temp\SavedBasket.csv";

        public string[] products;
        public string[] discounted;
        public string[] readItemsFromBasket;

        public List<string> savedProducts = new List<string>();
        public List<BasketItem> basketItems = new List<BasketItem>();
        public List<BuyedProduct> buyedProducts = new List<BuyedProduct>();
        public List<DiscountCopounes> discountCopounes = new List<DiscountCopounes>();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }
        public void Start()
        {
            Title = "AM Frukt Affär";
            Height = 600;
            Width = 1310;

            ScrollViewer scroll = (ScrollViewer)Content;
            scroll.Content = grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            ReadDiscountCopounes();

            ImageBrush myBrush = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("Bilder/Background.jpg"));
            myBrush.ImageSource = image.Source;

            StackPanel productStackPanel = new StackPanel
            {
                Background = myBrush
            };
            grid.Children.Add(productStackPanel);
            Grid.SetColumn(productStackPanel, 0);

            productWrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };
            productStackPanel.Children.Add(productWrapPanel);


            varukorgWrapPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = Brushes.LightBlue,
                Width = 365,
                Margin = new Thickness(10, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            grid.Children.Add(varukorgWrapPanel);
            Grid.SetColumn(varukorgWrapPanel, 1);

            KassaTableTitle();

            GetProducts();
        }

        public void GetProducts()
        {
            ReadSavedItems();

            products = File.ReadAllLines("Produkter.csv");
            foreach (string product in products)
            {
                string[] splittedPruduct = product.Split(',');

                StackPanel productStackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Width = 155,
                    Height = 250,
                    Background = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                string path = ("Bilder/" + splittedPruduct[0]);

                ImageSource source = new BitmapImage(new Uri(path, UriKind.Relative));
                Image image = new Image
                {
                    Source = source,
                    Width = 100,
                    Height = 100,
                    Stretch = Stretch.Fill,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                Label productName = new Label
                {
                    Content = splittedPruduct[1],
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 0, 0, 0)
                };

                Label productDescription = new Label
                {
                    Content = splittedPruduct[2],
                    Padding = new Thickness(10, 0, 0, 0)
                };

                Label productPrice = new Label
                {
                    Content = "Price: " + splittedPruduct[3] + "/kg",
                    Padding = new Thickness(10, 5, 0, 0),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                };

                currentStock = new Label
                {
                    Content = "Available: " + splittedPruduct[4] + " kg",
                    Padding = new Thickness(10, 5, 0, 0)
                };

                StackPanel amountAndBuy = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10)
                };

                Button buyProduct = new Button()
                {
                    Content = "Buy " + splittedPruduct[1],
                    Width = 100,
                    Height = 20,
                    Margin = new Thickness(5)
                };

                TextBox amount = new TextBox()
                {
                    Margin = new Thickness(2),
                    TextAlignment = TextAlignment.Center,
                    Width = 25,
                    Height = 20,
                    MaxLength = 3,
                };

                buyProduct.Tag = amount;

                BuyedProduct buyedProduct = new BuyedProduct()
                {
                    Name = splittedPruduct[1],
                    Amount = 0,
                    Price = int.Parse(splittedPruduct[3]),
                    CurrentStock = int.Parse(splittedPruduct[4])
                };
                buyedProducts.Add(buyedProduct);

                amountAndBuy.Children.Add(amount);
                amountAndBuy.Children.Add(buyProduct);

                productStackPanel.Children.Add(image);
                productStackPanel.Children.Add(productName);
                productStackPanel.Children.Add(productDescription);
                productStackPanel.Children.Add(productPrice);
                productStackPanel.Children.Add(currentStock);
                productStackPanel.Children.Add(amountAndBuy);

                productWrapPanel.Children.Add(productStackPanel);

                buyProduct.Click += GetProduct_Click;
            }
        }

        private void GetProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            string name = button.Content.ToString().Remove(0, 4);
            BuyedProduct product = buyedProducts.Where(a => a.Name == name).Select(a => a).FirstOrDefault();

            TextBox amountTextBox = (TextBox)button.Tag;

            if (int.TryParse(amountTextBox.Text, out int result))
            {
                if (result <= product.CurrentStock)
                {
                    int amount = int.Parse(amountTextBox.Text);

                    product.Amount = amount;

                    currentStock.Content = "Available: " + (product.CurrentStock - product.Amount) + " kg";
                    product.CurrentStock = product.CurrentStock - product.Amount;

                    SendToBasket(name, amount, amount * product.Price);
                    amountTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Tyvärr, vi har inga tillräkliga varor!");
                }
            }
            else
            {
                MessageBox.Show("Du kan bara skriva nummer!");
            }
        }

        public void KassaTableTitle()
        {
            string path = ("Bilder/Logo.png");

            ImageSource source = new BitmapImage(new Uri(path, UriKind.Relative));
            Image image = new Image
            {
                Source = source,
                Width = 250,
                Height = 125,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 10, 5, 0)
            };
            varukorgWrapPanel.Children.Add(image);

            Label kassan = new Label()
            {
                Content = "KASSAN: ",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 0, 5, 0),
                FontSize = 17,
            };
            varukorgWrapPanel.Children.Add(kassan);

            TableTitle();
        }

        public void TableTitle()
        {
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Width = 330,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 5, 0),
            };
            varukorgWrapPanel.Children.Add(panel);

            Label itemNumber = new Label()
            {
                Content = "No.",
                Padding = new Thickness(5),
                FontSize = 15,
                Width = 40,
                FontWeight = FontWeights.Bold
            };
            panel.Children.Add(itemNumber);

            Label itemName = new Label()
            {
                Content = "Product",
                Padding = new Thickness(5),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 100
            };
            panel.Children.Add(itemName);

            Label itemAmount = new Label()
            {
                Content = "Amount",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(itemAmount);

            Label itemPrice = new Label()
            {
                Content = "Price",
                Padding = new Thickness(5),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 70,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(itemPrice);
        }

        public void SendToBasket(object nameContent, object amountContent, object priceContent)
        {
            BasketItem basketItem = new BasketItem()
            {
                Name = nameContent.ToString(),
                TotalAmount = Convert.ToInt32(amountContent),
                TotalPrice = Convert.ToInt32(priceContent),
                Checked = false
            };

            if (basketItems.Exists(a => a.Name == nameContent.ToString()))
            {
                BasketItem existProduct = basketItems.Where(a => a.Name == nameContent.ToString()).Select(a => a).FirstOrDefault();
                existProduct.TotalAmount += Convert.ToInt32(amountContent);
                existProduct.TotalPrice += Convert.ToInt32(priceContent);
            }
            else
            {
                basketItems.Add(basketItem);
            }

            ShowBasketItems();
        }

        public void ShowBasketItems()
        {
            int counter = 1;

            varukorgWrapPanel.Children.Clear();
            KassaTableTitle();

            foreach (var item in basketItems)
            {
                StackPanel panel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Width = 330,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                varukorgWrapPanel.Children.Add(panel);

                Label itemNumber = new Label()
                {
                    Content = counter,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 40
                };
                panel.Children.Add(itemNumber);

                Label itemName = new Label()
                {
                    Content = item.Name,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 100
                };
                panel.Children.Add(itemName);


                Label itemAmount = new Label()
                {
                    Content = item.TotalAmount,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 90,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };
                panel.Children.Add(itemAmount);

                Label itemPrice = new Label()
                {
                    Content = item.TotalPrice + " kr",
                    FontSize = 15,
                    Width = 70,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                panel.Children.Add(itemPrice);

                CheckBox checkBox = new CheckBox()
                {
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(checkBox);

                checkBox.Tag = item;

                checkBox.Checked += CheckBox_Checked;
                checkBox.Unchecked += CheckBox_Unchecked;

                counter++;
            }
            if (basketItems.Count != 0)
            {
                CreateButtons();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            BasketItem item = (BasketItem)box.Tag;
            BasketItem existProduct = basketItems.Where(a => a.Name == item.Name.ToString()).Select(a => a).FirstOrDefault();

            existProduct.Checked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            BasketItem item = (BasketItem)box.Tag;
            BasketItem existProduct = basketItems.Where(a => a.Name == item.Name.ToString()).Select(a => a).FirstOrDefault();

            existProduct.Checked = false;
        }

        public void CreateButtons()
        {
            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            varukorgWrapPanel.Children.Add(buttons);

            Button save = new Button()
            {
                Content = "Spara",
                Width = 70,
                Height = 30,
                Margin = new Thickness(5),
                FontSize = 15,
            };
            buttons.Children.Add(save);
            save.Click += Save_Click;

            Button delete = new Button()
            {
                Content = "Ta Bort",
                Width = 70,
                Height = 30,
                Margin = new Thickness(5),
                FontSize = 15,
            };
            buttons.Children.Add(delete);
            delete.Click += Delete_Click;

            Button pay = new Button()
            {
                Content = "Betala",
                Width = 70,
                Height = 30,
                Margin = new Thickness(5),
                FontSize = 15,
            };
            buttons.Children.Add(pay);
            pay.Click += Pay_Click;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            foreach (var item in basketItems)
            {
                string name = item.Name;
                string totalAmount = item.TotalAmount.ToString();
                string totalPrice = item.TotalPrice.ToString();

                savedProducts.Add(name + "," + totalAmount + "," + totalPrice);
            }

            File.WriteAllLines(savedBasket, savedProducts);
            MessageBox.Show("Din varukorg har sparat!");

            varukorgWrapPanel.Children.Clear();
            KassaTableTitle();
            ShowBasketItems();
            savedProducts.Clear();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            PaymentDetails();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (basketItems.All(a => a.Checked == false))
            {
                MessageBox.Show("Du måste välj en produkt!");
            }
            else
            {
                foreach (var item in basketItems.ToList())
                {
                    if (item.Checked == true)
                    {
                        basketItems.Remove(item);
                    }
                }

                File.Delete(savedBasket);
                ShowBasketItems();
            }
        }

        public void PaymentDetails()
        {
            int counter = 1;

            varukorgWrapPanel.Children.Clear();

            Label receiptTitle = new Label()
            {
                Content = "Din Faktura:",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10)
            };
            varukorgWrapPanel.Children.Add(receiptTitle);

            TableTitle();

            foreach (var item in basketItems)
            {
                StackPanel panel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Width = 330,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                varukorgWrapPanel.Children.Add(panel);

                Label itemNumber = new Label()
                {
                    Content = counter,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 40
                };
                panel.Children.Add(itemNumber);

                Label itemName = new Label()
                {
                    Content = item.Name,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 100
                };
                panel.Children.Add(itemName);

                Label itemAmount = new Label()
                {
                    Content = item.TotalAmount,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 90,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };
                panel.Children.Add(itemAmount);

                Label itemPrice = new Label()
                {
                    Content = item.TotalPrice + " kr",
                    FontSize = 15,
                    Width = 70,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                panel.Children.Add(itemPrice);

                counter++;
            }

            total = basketItems.Select(a => a.TotalPrice).Sum();

            totalPrice = new Label()
            {
                Content = "Din totala belopp är: " + total + " kr",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10)
            };

            Label discount = new Label
            {
                Content = "Skriv in din rabattkod:",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10,10,10,0)
            };
            varukorgWrapPanel.Children.Add(discount);

            StackPanel discountStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10,0,0,0)
            };
            varukorgWrapPanel.Children.Add(discountStack);

            discountText = new TextBox
            {
                Margin = new Thickness(2),
                TextAlignment = TextAlignment.Left,
                Width = 150,
                Height = 25,
                Padding = new Thickness(5)
            };

            Button discountButton = new Button
            {
                Content = "Lös kod",
                Width = 60,
                Height = 25,
                Margin = new Thickness(5)
            };

            discountStack.Children.Add(discountText);
            discountStack.Children.Add(discountButton);
            varukorgWrapPanel.Children.Add(totalPrice);

            discountButton.Click += DiscountButton_Click;

            finish = new Button()
            {
                Content = "Slutför",
                Width = 75,
                Height = 30,
                Margin = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            varukorgWrapPanel.Children.Add(finish);
            finish.Click += Finish_Click;
        }

        private void DiscountButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateDiscount(discountText.Text, totalPrice);
        }

        public void CalculateDiscount(string input, Label totalprice)
        {
            bool Valid = false;

            foreach (var code in discountCopounes)
            {
                if (input == code.Code)
                {
                    Valid = true;
                    discountCopounes.Remove(code);
                    break;
                }
            }

            discountText.Clear();

            if (Valid == true && total > 0)
            {
                totalprice.Content = Math.Round(total - (total * int.Parse(discounted[1]) / 100),2);
                totalprice.Content = "Din totala belopp är: " + totalprice.Content + " kr";
                total = total - (total * int.Parse(discounted[1]) / 100);

                varukorgWrapPanel.Children.Remove(totalPrice);
                varukorgWrapPanel.Children.Add(totalPrice);
                varukorgWrapPanel.Children.Remove(finish);
                varukorgWrapPanel.Children.Add(finish);
                Valid = false;

                MessageBox.Show("Din rabat kod är accepterat!");
            }
            else
            {
                MessageBox.Show("Ogiligt rabatt kod!");
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Grattis, dit köp har klart!");
            varukorgWrapPanel.Children.Clear();
            basketItems.Clear();
            File.Delete(savedBasket);
            KassaTableTitle();
        }

        public void ReadSavedItems()
        {
            if (File.Exists(savedBasket))
            {
                readItemsFromBasket = File.ReadAllLines(savedBasket);

                if (readItemsFromBasket.Length > 0)
                {
                    foreach (var item in readItemsFromBasket)
                    {
                        string[] temp = item.Split(',');

                        BasketItem basketItem = new BasketItem();

                        basketItem.Name = temp[0];
                        basketItem.TotalAmount = int.Parse(temp[1]);
                        basketItem.TotalPrice = int.Parse(temp[2]);
                        basketItem.Checked = false;
                        basketItems.Add(basketItem);
                    }
                    ShowBasketItems();
                }
            }
        }

        public void ReadDiscountCopounes()
        {
            string[] discount = File.ReadAllLines("Discount.csv");

            foreach (var item in discount)
            {
                discounted = item.Split(',');
                DiscountCopounes copoune = new DiscountCopounes()
                {
                    Code = discounted[0],
                    Percentage = double.Parse(discounted[1])
                };
                discountCopounes.Add(copoune);
            }
        }
    }
}
