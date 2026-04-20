using System;
using System.Collections.Generic;
using System.Text;
using BillFlow.maindataTableAdapters;

namespace BillFlow
{
    public class DatabaseManager
    {
        private clientsTableAdapter _clientsAdapter;
        private paymentsTableAdapter _paymentsAdapter;
        private servicesTableAdapter _servicesAdapter;
        private usersTableAdapter _usersAdapter;
        private payment_detailsTableAdapter _payment_detailsAdapter;

        public DatabaseManager()
        {
            _clientsAdapter = new clientsTableAdapter();
            _paymentsAdapter = new paymentsTableAdapter();
            _servicesAdapter = new servicesTableAdapter();
            _usersAdapter = new usersTableAdapter();
            _payment_detailsAdapter = new payment_detailsTableAdapter();
        }

        public void AddClient(string fullName, string contactInfo)
        {
            try
            {
                _clientsAdapter.Insert(fullName, contactInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления клиента {fullName}: {ex.Message}");
            }
        }
        public void AddPayment(DateTime paymentDate, int clientId, int userId, decimal totalSum)
        {
            try
            {
                _paymentsAdapter.Insert(paymentDate, clientId, userId, totalSum);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления платежа для {clientId}: {ex.Message}");
            }
        }

        public void AddUser(string username, string user_role, byte[] pwd_hash)
        {
            try
            {
                _usersAdapter.Insert(username, pwd_hash, user_role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления пользователя {username}: {ex.Message}");
            }
        }

        public void AddService(string service_name, decimal current_price)
        {
            try
            {
                _servicesAdapter.Insert(service_name, current_price);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления сервиса {service_name}: {ex.Message}");
            }
        }

        public void AddPayment_details(int payment_id, int service_id, int amount, decimal price_at_sale)
        {
            try
            {
                _payment_detailsAdapter.Insert(payment_id, service_id, amount, price_at_sale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления сервиса {service_id} к платежу {payment_id}: {ex.Message}");
            }
        }

        public maindata.usersDataTable GetUsers() => _usersAdapter.GetData();
        public maindata.clientsDataTable GetClients() => _clientsAdapter.GetData();
        public maindata.servicesDataTable GetServices() => _servicesAdapter.GetData();
        public maindata.paymentsDataTable GetPayments() => _paymentsAdapter.GetData();
        public maindata.payment_detailsDataTable GetPaymentDetails() => _payment_detailsAdapter.GetData();

        public bool Authenticate(string username, string password)
        {
            var users = GetUsers();
            byte[] inputHash = Session.HashPassword(password);

            var user = users.FirstOrDefault(u => u.username.Trim() == username && u.pwd_hash.SequenceEqual(inputHash));

            if (user != null)
            {
                Session.CurrentUserId = user.Id;
                Session.CurrentUserRole = user.user_role.Trim();
                return true;
            }
            return false;
        }

        public void CreateFullPayment(DateTime date, int clientId, int userId, decimal totalSum, List<dynamic> services)
        {
            _paymentsAdapter.Insert(date, clientId, userId, totalSum);

            int newPaymentId = (int)GetPayments().OrderByDescending(p => p.Id).First().Id;

            foreach (var s in services)
            {
                _payment_detailsAdapter.Insert(newPaymentId, s.ServiceId, s.Amount, s.PriceAtSale);
            }
        }

        public void DeleteClient(int id) { _clientsAdapter.Delete(id); }
        public void DeleteService(int id) { _servicesAdapter.Delete(id); }
        public void DeleteUser(int id) { _usersAdapter.Delete(id); }

        public void DeletePayment(int id)
        {
            _payment_detailsAdapter.DeleteByPaymentId(id);

            _paymentsAdapter.Delete(id);
        }

        public void EnsureAdminExists()
        {
            try
            {
                var users = GetUsers();

                bool adminExists = users.Any(u => u.user_role.Trim().ToLower() == "admin");

                if (!adminExists)
                {
                    byte[] defaultHash = Session.HashPassword("admin");
                    AddUser("admin", "admin", defaultHash);
                    System.Diagnostics.Debug.WriteLine("Дефолтный админ создан: admin/admin");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке админа: {ex.Message}");
            }
        }
    }
}
