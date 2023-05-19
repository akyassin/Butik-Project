using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Butik_Projekt.Models;

namespace Butik_Projekt
{
    public partial class MainWindow : Window
    {
        private Grid grid = new Grid();
        private WrapPanel productWrapPanel;
        private WrapPanel varukorgWrapPanel;
        private StackPanel finishPanel = new StackPanel();

        //private Label totalPrice;
        private Label currentStock;
        private TextBox discountText;

        private List<Coupon> _coupons;
        private List<Product> _products;
        private List<BasketProduct> _basketProducts;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGlobalVariables();

            Start();
        }

        private void InitializeGlobalVariables()
        {
            _products = ReadProducts();
            _coupons = ReadDiscountCopounes();
            _basketProducts = ReadSavedProducts();
        }

        public void Start()
        {
            ConfigureWindowSettings();
            ConfigureScrolledMainGrid();
            ConfigureProductsWrapPanel();
            ConfigureVarukorgWrapPanel();

            ShowBasketItems();

            ShowProducts();
        }

        private void ConfigureScrolledMainGrid()
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            ScrollViewer scroll = (ScrollViewer)Content;
            scroll.Content = grid;
        }

        private void ConfigureVarukorgWrapPanel()
        {
            var varukorgStackPanel = new StackPanel
            {
                Background = Brushes.LightBlue,
            };

            varukorgWrapPanel = new WrapPanel
            {
                Orientation = Orientation.Vertical,
                Width = 350,
                Margin = new Thickness(10, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            varukorgStackPanel.Children.Add(varukorgWrapPanel);

            grid.Children.Add(varukorgStackPanel);
            Grid.SetColumn(varukorgStackPanel, 1);

            CreateKassaTableTitle();
        }

        private void ConfigureProductsWrapPanel()
        {
            var productStackPanel = new StackPanel
            {
                Background = GetBackgroundImageBrush()
            };

            productWrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };

            productStackPanel.Children.Add(productWrapPanel);

            grid.Children.Add(productStackPanel);
            Grid.SetColumn(productStackPanel, 0);
        }

        private void ConfigureWindowSettings()
        {
            Title = AppSetting.Title;
            Height = AppSetting.Height;
            Width = AppSetting.Width;
        }

        private static ImageBrush GetBackgroundImageBrush()
        {
            var uri = new Uri(AppSetting.BackgroundImagePath);
            return new ImageBrush(new BitmapImage(uri));
        }

        public void ShowProducts()
        {
            foreach (var product in _products)
            {
                var productStackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    Width = 155,
                    Height = 250,
                    Background = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var path = ("Images/" + product.ImagePath);

                var image = new Image
                {
                    Width = 100,
                    Height = 100,
                    Stretch = Stretch.Fill,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = new BitmapImage(new Uri(path, UriKind.Relative))
                };

                Label productName = new Label
                {
                    Content = product.Name,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 0, 0, 0)
                };

                Label productDescription = new Label
                {
                    Content = product.Description,
                    Padding = new Thickness(10, 0, 0, 0)
                };

                Label productPrice = new Label
                {
                    Content = "Price: " + product.Price + "/kg",
                    Padding = new Thickness(10, 5, 0, 0),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                };

                currentStock = new Label
                {
                    Content = "Available: " + product.CurrentStock + " kg",
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
                    Content = "Buy " + product.Name,
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

                buyProduct.Tag = new BoughtProduct
                {
                    ProductName= product.Name,
                    TextBox = amount
                };

                Product buyedProduct = new Product()
                {
                    Name = product.Name,
                    Price = product.Price,
                    CurrentStock = product.CurrentStock
                };

                //_products.Add(buyedProduct);

                amountAndBuy.Children.Add(amount);
                amountAndBuy.Children.Add(buyProduct);

                productStackPanel.Children.Add(image);
                productStackPanel.Children.Add(productName);
                productStackPanel.Children.Add(productDescription);
                productStackPanel.Children.Add(productPrice);
                productStackPanel.Children.Add(currentStock);
                productStackPanel.Children.Add(amountAndBuy);

                productWrapPanel.Children.Add(productStackPanel);

                buyProduct.Click += ShowProducts_Click;
            }
        }

        private void ShowProducts_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var boughtProduct = (BoughtProduct)button.Tag;
            
            var product = _products.FirstOrDefault(a => a.Name == boughtProduct.ProductName);

            if (int.TryParse(boughtProduct.TextBox.Text, out int amount))
            {
                if (amount <= product.CurrentStock)
                {
                    currentStock.Content = "Available: " + (product.CurrentStock - amount) + " kg";
                    _products.FirstOrDefault(a => a.Name == boughtProduct.ProductName).CurrentStock -= amount;

                    SendToBasket(boughtProduct.ProductName, amount, amount * product.Price);
                    boughtProduct.TextBox.Clear();
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

        public void CreateKassaTableTitle()
        {
            string path = ("Images/Logo.png");

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
            BasketProduct basketItem = new BasketProduct()
            {
                Name = nameContent.ToString(),
                TotalAmount = Convert.ToInt32(amountContent),
                TotalPrice = Convert.ToInt32(priceContent),
                Checked = false
            };

            if (_basketProducts.Exists(a => a.Name == nameContent.ToString()))
            {
                BasketProduct existProduct = _basketProducts.Where(a => a.Name == nameContent.ToString()).Select(a => a).FirstOrDefault();
                existProduct.TotalAmount += Convert.ToInt32(amountContent);
                existProduct.TotalPrice += Convert.ToInt32(priceContent);
            }
            else
            {
                _basketProducts.Add(basketItem);
            }

            ShowBasketItems();
        }

        public void ShowBasketItems()
        {
            int counter = 1;

            varukorgWrapPanel.Children.Clear();
            CreateKassaTableTitle();

            foreach (var item in _basketProducts)
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
            if (_basketProducts.Count != 0)
            {
                Line line = new Line
                {
                    X1 = 20,
                    Y1 = 20,
                    X2 = 350,
                    Y2 = 20,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                };
                varukorgWrapPanel.Children.Add(line);

                Label totalPrice = new Label()
                {
                    Padding = new Thickness(20, 10, 0, 20),
                    Content = "Totala belopp är: " + _basketProducts.Sum(a => a.TotalPrice) + " kr",
                    FontSize = 15,
                    Width = 250,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                varukorgWrapPanel.Children.Add(totalPrice);

                CreateButtons();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            BasketProduct item = (BasketProduct)box.Tag;
            BasketProduct existProduct = _basketProducts.Where(a => a.Name == item.Name.ToString()).Select(a => a).FirstOrDefault();

            existProduct.Checked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;

            BasketProduct item = (BasketProduct)box.Tag;
            BasketProduct existProduct = _basketProducts.Where(a => a.Name == item.Name.ToString()).Select(a => a).FirstOrDefault();

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
            var productsToSave = _basketProducts.Select(p => $"{p.Name},{p.TotalAmount},{p.TotalPrice}");

            File.WriteAllLines(AppSetting.SavedBasketPath, productsToSave);
            MessageBox.Show("Din varukorg har sparat!");

            varukorgWrapPanel.Children.Clear();
            CreateKassaTableTitle();
            ShowBasketItems();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            PaymentDetails();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_basketProducts.All(a => a.Checked == false))
            {
                MessageBox.Show("Du måste välj en produkt!");
            }
            else
            {
                foreach (var item in _basketProducts.ToList())
                {
                    if (item.Checked == true)
                    {
                        _basketProducts.Remove(item);
                    }
                }

                File.Delete(AppSetting.SavedBasketPath);
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

            foreach (var item in _basketProducts)
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

            var total = _basketProducts.Sum(a => a.TotalPrice);

            Label totalPrice = new Label()
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
                Margin = new Thickness(10, 10, 10, 0)
            };
            varukorgWrapPanel.Children.Add(discount);

            StackPanel discountStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
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
            discountButton.Click += DiscountButton_Click;

            CreateFinishPanel();
        }

        private void CreateFinishPanel()
        {
            finishPanel.Orientation = Orientation.Horizontal;
            finishPanel.HorizontalAlignment = HorizontalAlignment.Center;

            var finish = new Button()
            {
                Content = "Slutför",
                Width = 75,
                Height = 30,
                Margin = new Thickness(15, 15, 5, 15),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            finishPanel.Children.Add(finish);
            finish.Click += Finish_Click;

            var back = new Button()
            {
                Content = "Tillbaka",
                Width = 75,
                Height = 30,
                Margin = new Thickness(5, 15, 15, 15),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            finishPanel.Children.Add(back);
            back.Click += Back_Click;

            varukorgWrapPanel.Children.Add(finishPanel);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ShowBasketItems();
        }

        private void DiscountButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateDiscount(discountText.Text);
        }

        public void CalculateDiscount(string input)
        {
            //string discounted = null;
            //bool Valid = ValidateInput(input); 

            //discountText.Clear();

            //double total = _basketProducts.Sum(a => a.TotalPrice);
            //if (Valid == true && total > 0)
            //{
            //    totalprice.Content = Math.Round(total - (total * int.Parse(discounted) / 100), 2);
            //    totalprice.Content = "Din totala belopp är: " + totalprice.Content + " kr";
            //    total -= (total * int.Parse(discounted) / 100);

            //    varukorgWrapPanel.Children.Remove(finishPanel);
            //    finishPanel.Children.Clear();
            //    CreateFinishPanel();
            //    Valid = false;

            //    MessageBox.Show("Din rabat kod är accepterat!");
            //}
            //else
            //{
            //    MessageBox.Show("Ogiligt rabatt kod!");
            //}
        }

        private bool ValidateInput(string input)
        {
            var coupon = _coupons.FirstOrDefault(c => input == c.Code);

            if (coupon != null)
            {
                _coupons.Remove(coupon);
                return true;
            }

            return false;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Grattis, dit köp har klart!");
            varukorgWrapPanel.Children.Clear();
            _basketProducts.Clear();
            File.Delete(AppSetting.SavedBasketPath);
            CreateKassaTableTitle();
        }

        private List<BasketProduct> ReadSavedProducts()
        {
            var basketProducts = new List<BasketProduct>();

            if (File.Exists(AppSetting.SavedBasketPath))
            {
                basketProducts.AddRange(File.ReadAllLines(AppSetting.SavedBasketPath)
                .Select(p =>
                {
                    var productFields = p.Split(',');
                    return new BasketProduct
                    {
                        Name = productFields[0],
                        TotalAmount = int.Parse(productFields[1]),
                        TotalPrice = int.Parse(productFields[2]),
                        Checked = false
                    };
                }));
            }

            return basketProducts;
        }

        private List<Product> ReadProducts()
        {
            var products = new List<Product>();

            products.AddRange(File.ReadAllLines(AppSetting.ProductsFilePath)
            .Select(p =>
            {
                var productFields = p.Split(',');
                return new Product()
                {
                    ImagePath = productFields[0],
                    Name = productFields[1],
                    Description = productFields[2],
                    Price = int.Parse(productFields[3]),
                    CurrentStock = int.Parse(productFields[4])
                };
            }));

            return products;
        }

        private List<Coupon> ReadDiscountCopounes()
        {
            var coupons = new List<Coupon>();

            coupons.AddRange(File.ReadAllLines(AppSetting.DiscountFilePath)
            .Select(c =>
            {
                var discounted = c.Split(',');
                return new Coupon()
                {
                    Code = discounted[0],
                    Percentage = double.Parse(discounted[1])
                };
            }));

            return coupons;
        }


    }
}
