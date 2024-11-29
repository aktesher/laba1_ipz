using FBS1._1.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace FBS1._1
{
    public partial class SeatSelectionWindow : Window
    {
        private int _flightId;
        private int _userId;
        private DatabaseManager _databaseManager;

        public SeatSelectionWindow(int flightId, int userId, DatabaseManager databaseManager)
        {
            InitializeComponent();
            _flightId = flightId;
            _userId = userId;
            _databaseManager = databaseManager;

            LoadSeats();
        }

        private void LoadSeats()
        {
            var availableSeats = _databaseManager.GetAvailableSeats(_flightId);
            SeatsListBox.ItemsSource = availableSeats;
        }

        private void ConfirmSeatButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedSeat = SeatsListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedSeat))
            {
                bool success = _databaseManager.BookSeat(_flightId, selectedSeat);
                if (success)
                {
                    // Запрос на отправку билета на почту
                    var result = MessageBox.Show("Seat booked successfully! Would you like to receive your ticket by email?", "Email Confirmation", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Скрыть список мест и кнопку подтверждения
                        SeatsListBox.Visibility = Visibility.Collapsed;
                        ConfirmSeatButton.Visibility = Visibility.Collapsed;
                        Choose.Visibility = Visibility.Collapsed;

                        // Показываем секцию для ввода почты
                        EmailInputSection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.Close(); // Закрываем окно, если пользователь не хочет получить билет на почту
                    }
                }
                else
                {
                    MessageBox.Show("Failed to book the seat. Please try again.");
                }
            }
            else
            {
                MessageBox.Show("Please select a seat.");
            }
        }

        private void SendTicketButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            // Проверка корректности email
            if (!IsValidEmail(email))
            {
                MessageBox.Show("The email address entered is not valid. Please try again.");
                return;
            }

            MessageBox.Show("Ticket sent to your email!");
            this.Close(); // Закрываем окно после отправки билета
        }

        // Метод для проверки правильности email
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }
    }
}
