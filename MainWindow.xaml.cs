using System;
using System.Windows;
using System.Windows.Controls;
using FBS1._1.Models;
using System.Linq;

namespace FBS1._1
{
    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private int _currentUserId;

        public MainWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
        }

        // Обработчик для кнопки "Login"
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Проверка логина и пароля
            if (_databaseManager.Login(username, password, out _currentUserId))
            {
                // Если авторизация успешна, скрываем панель логина
                AuthGrid.Visibility = Visibility.Collapsed;

                // Отображаем страницу с билетами
                TicketsPage.Visibility = Visibility.Visible;

                // Показываем кнопки меню
                ProfileButton.Visibility = Visibility.Visible;
                TicketsButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Visible;

                // Загружаем информацию о билетах или другой контент
                LoadTickets();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }

        // Метод для загрузки информации о билетах
        private void LoadTickets()
        {
            var tickets = _databaseManager.GetFlights(); // Загрузка списка билетов

            // Создаем коллекцию с отформатированными датами
            var formattedTickets = tickets.Select(ticket => new
            {
                ticket.Id,
                ticket.From,
                ticket.To,
                Date = ticket.Date.ToString("yyyy-MM-dd") // Форматируем только дату
            }).ToList();

            // Привязываем список рейсов с отформатированными датами к DataGrid
            TicketsDataGrid.ItemsSource = formattedTickets;

            foreach (var ticket in formattedTickets)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };

                var fromText = new TextBlock { Text = ticket.From, Width = 100 };
                var toText = new TextBlock { Text = ticket.To, Width = 100 };

                // Форматируем дату, чтобы отображалась только дата без времени
                var dateText = new TextBlock { Text = ticket.Date, Width = 100 };

                // Кнопка для покупки
                var buyButton = new Button
                {
                    Content = "Buy",
                    Width = 80,
                    Height = 25,
                    Margin = new Thickness(5),
                };

                // При нажатии на кнопку открываем окно выбора мест для данного рейса
                buyButton.Click += (s, e) =>
                {
                    // Создаем новое окно для выбора места, передаем ID рейса и ID пользователя
                    var seatSelectionWindow = new SeatSelectionWindow(ticket.Id, _currentUserId, _databaseManager);
                    seatSelectionWindow.ShowDialog();
                };

                row.Children.Add(fromText);
                row.Children.Add(toText);
                row.Children.Add(dateText);
                row.Children.Add(buyButton);
            }
        }





        // Обработчик для кнопки "Logout"
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем страницу с билетами
            TicketsPage.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Collapsed;
            // Показываем экран логина
            AuthGrid.Visibility = Visibility.Visible;

            // Очищаем введенные данные
            UsernameTextBox.Clear();
            PasswordBox.Clear();

            // Скрываем кнопки меню
            ProfileButton.Visibility = Visibility.Collapsed;
            TicketsButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Collapsed;
        }

        // Обработчик для кнопки "Profile"
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем информацию о пользователе
            var userInfo = _databaseManager.GetUserInfo(_currentUserId);
            NameTextBox.Text = userInfo.Name;
            SurnameTextBox.Text = userInfo.Surname;
            AgeTextBox.Text = userInfo.Age.ToString();

            // Показываем панель профиля справа
            ProfileGrid.Visibility = Visibility.Visible;
            TicketsPage.Visibility = Visibility.Collapsed; // Скрываем билеты при отображении профиля
        }

        // Обработчик для кнопки "Tickets"
        private void TicketsButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileGrid.Visibility = Visibility.Collapsed;
            TicketsPage.Visibility = Visibility.Visible;
        }

        // Обработчик для кнопки "Save Changes"
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            int age;

            if (int.TryParse(AgeTextBox.Text, out age))
            {
                // Проверяем, что возраст >= 14
                if (age >= 14 && age < 100)
                {
                    // Обновляем информацию о пользователе
                    bool success = _databaseManager.UpdateUserInfo(_currentUserId, name, surname, age);
                    if (success)
                    {
                        MessageBox.Show("Данные успешно сохранены!");
                        ProfileGrid.Visibility = Visibility.Collapsed;
                        TicketsPage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при сохранении данных.");
                    }
                }
                else
                {
                    MessageBox.Show("Возраст должен быть не меньше 14 лет.");
                }
            }
            else
            {
                MessageBox.Show("Введите корректный возраст.");
            }

        }

        // Обработчик для кнопки "Register"
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            AuthGrid.Visibility = Visibility.Collapsed;
            RegisterGrid.Visibility = Visibility.Visible;
        }

        // Обработчик для кнопки "Back to Login" на экране регистрации
        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Collapsed;
            AuthGrid.Visibility = Visibility.Visible;
        }

        // Обработчик для кнопки "Register" на экране регистрации
        private void RegisterActionButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsernameTextBox.Text;
            string password = RegisterPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            //string email

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Проверка на совпадение паролей
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            // Добавляем пользователя в базу данных
            bool isSuccess = _databaseManager.Register(username, password);
            if (isSuccess)
            {
                MessageBox.Show("Регистрация прошла успешно.");
                RegisterGrid.Visibility = Visibility.Collapsed;
                AuthGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Ошибка регистрации.");
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // Проверяем, что CommandParameter не равен null
                if (button.CommandParameter != null)
                {
                    int flightId;
                    if (int.TryParse(button.CommandParameter.ToString(), out flightId))
                    {
                        // Проверяем, что _currentUserId и _databaseManager инициализированы
                        if (_databaseManager != null && _currentUserId > 0)
                        {
                            var seatSelectionWindow = new SeatSelectionWindow(flightId, _currentUserId, _databaseManager);
                            seatSelectionWindow.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: Пользователь не авторизован.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: Неверный формат идентификатора рейса.");
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: CommandParameter не установлен.");
                }
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            AuthGrid.Visibility = Visibility.Collapsed;
            ChangePasswordGrid.Visibility = Visibility.Visible;
        }

        // Обработчик для кнопки "Back to Login" на экране смены пароля
        private void BackToLoginFromChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordGrid.Visibility = Visibility.Collapsed;
            AuthGrid.Visibility = Visibility.Visible;
        }

        // Обработчик для кнопки "Change Password" на экране смены пароля
        private void ChangePasswordActionButton_Click(object sender, RoutedEventArgs e)
        {
            string nickname = NicknameTextBox.Text;
            string newPassword = NewPasswordBox.Password;
            string confirmNewPassword = ConfirmNewPasswordBox.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNewPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Проверка на совпадение нового пароля и подтверждения
            if (newPassword != confirmNewPassword)
            {
                MessageBox.Show("Новый пароль и подтверждение пароля не совпадают.");
                return;
            }

            // Проверка существования пользователя по нику и сброс пароля
            bool success = _databaseManager.ChangePasswordByNickname(nickname, newPassword);
            if (success)
            {
                MessageBox.Show("Пароль успешно изменен.");
                ChangePasswordGrid.Visibility = Visibility.Collapsed;
                AuthGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Ошибка: Неверный ник или проблемы с изменением пароля.");
            }
        }

        private void BackToLoginFromChangePassword(object sender, RoutedEventArgs e)
        {
            ChangePasswordGrid.Visibility = Visibility.Collapsed;
            AuthGrid.Visibility = Visibility.Visible;
        }

    }
}