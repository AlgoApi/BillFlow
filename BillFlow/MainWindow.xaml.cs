using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BillFlow
{
    public partial class MainWindow : Window
    {
        DatabaseManager db = new DatabaseManager();
        List<dynamic> currentCart = new List<dynamic>();
        decimal currentTotalSum = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Session.IsAdmin)
            {
                tabAdmin.Visibility = Visibility.Visible;
                btnDeletePayment.Visibility = Visibility.Visible;
                btnDeleteClient.Visibility = Visibility.Visible;
                btnDeleteService.Visibility = Visibility.Visible;
            }
            LoadData(Session.IsAdmin);
        }

        private void LoadData(bool is_admin)
        {
            var clients = db.GetClients();
            var services = db.GetServices();
            var users = db.GetUsers();
            var payments = db.GetPayments();

            if (is_admin)
            {
                var usersView = from u in users
                                select new
                                {
                                    Id = u.Id,
                                    Username = u.username,
                                    User_role = u.user_role
                                };
                dgUsers.ItemsSource = usersView.ToList();
            }

            cmbClients.ItemsSource = clients;
            cmbServices.ItemsSource = services;

            var paymentsView = from p in payments
                               join c in clients on p.client_id equals c.Id
                               join u in users on p.user_id equals u.Id
                               select new
                               {
                                   Id = p.Id,
                                   Date = p.payment_date.ToString("dd.MM.yyyy HH:mm"),
                                   ClientName = c.full_name.Trim(),
                                   UserName = u.username.Trim(),
                                   TotalSum = p.total_sum
                               };

            var clientsView = from c in clients
                              select new 
                              {
                                  Id = c.Id,
                                  Full_name = c.full_name,
                                  Сontact_info = c.contact_info,
                              };

            var servicesView = from s in services
                               select new
                               {
                                   Id = s.Id,
                                   Service_name = s.service_name,
                                   Current_price = s.current_price
                               };

            dgPayments.ItemsSource = paymentsView.ToList();
            dgClients.ItemsSource = clientsView.ToList();
            dgServices.ItemsSource = servicesView.ToList();
        }


        private void BtnAddServiceToCart_Click(object sender, RoutedEventArgs e)
        {
            if (cmbServices.SelectedItem == null || !int.TryParse(txtAmount.Text, out int amount)) return;

            var selectedService = (maindata.servicesRow)((System.Data.DataRowView)cmbServices.SelectedItem).Row;
            decimal price = selectedService.current_price;

            var cartItem = new
            {
                ServiceId = selectedService.Id,
                ServiceName = selectedService.service_name.Trim(),
                PriceAtSale = price,
                Amount = amount,
                DisplayText = $"{selectedService.service_name.Trim()} x{amount} — {price * amount} руб."
            };

            currentCart.Add(cartItem);
            lstCart.Items.Add(cartItem);
            currentTotalSum += (price * amount);
            txtTotal.Text = $"Итого: {currentTotalSum} руб.";
        }

        private void BtnSavePayment_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClients.SelectedItem == null || currentCart.Count == 0)
            {
                MessageBox.Show("Выберите клиента и добавьте минимум одну услугу!");
                return;
            }

            int clientId = (int)cmbClients.SelectedValue;

            try
            {
                db.CreateFullPayment(DateTime.Now, clientId, Session.CurrentUserId, currentTotalSum, currentCart);

                MessageBox.Show("Платеж успешно проведен!");

                currentCart.Clear();
                lstCart.Items.Clear();
                currentTotalSum = 0;
                txtTotal.Text = "Итого: 0 руб.";

                LoadData(Session.IsAdmin);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgClients.SelectedItem as dynamic;
            if (selectedItem != null)
            {
                int id = selectedItem.Id;
                if (MessageBox.Show("Удалить клиента?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        db.DeleteClient(id);
                        LoadData(Session.IsAdmin);
                    }
                    catch
                    {
                        MessageBox.Show("Нельзя удалить клиента, у которого есть платежи!");
                    }
                }
            }
        }
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string role = ((ComboBoxItem)cmbRoles.SelectedItem).Content.ToString();
            byte[] hash = Session.HashPassword(newUserPass.Password);
            db.AddUser(newUserName.Text, role, hash);
            MessageBox.Show("Пользователь добавлен");
            LoadData(Session.IsAdmin);
        }

        // --- КЛИЕНТЫ ---
        private void BtnAddClient_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newClientName.Text)) return;

            db.AddClient(newClientName.Text, newClientContact.Text);
            newClientName.Clear();
            newClientContact.Clear();
            LoadData(Session.IsAdmin); // Обновляем таблицу
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newSvcName.Text) || !decimal.TryParse(newSvcPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректное название и цену.");
                return;
            }

            db.AddService(newSvcName.Text, price);
            newSvcName.Clear();
            newSvcPrice.Clear();
            LoadData(Session.IsAdmin);
        }

        private void BtnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgServices.SelectedItem as dynamic;
            if (selectedItem != null)
            {
                int id = selectedItem.Id;
                if (MessageBox.Show("Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.DeleteService(id);
                    LoadData(Session.IsAdmin);
                }
            }
        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgPayments.SelectedItem as dynamic;
            if (selectedItem != null)
            {
                int id = selectedItem.Id;
                if (MessageBox.Show($"Удалить платеж №{id}?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.DeletePayment(id);
                    LoadData(Session.IsAdmin);
                }
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgUsers.SelectedItem as dynamic;
            if (selectedItem != null)
            {
                int id = selectedItem.Id;
                if (MessageBox.Show("Удалить пользователя?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.DeleteUser(id);
                    LoadData(Session.IsAdmin);
                }
            }
        }
    }
}