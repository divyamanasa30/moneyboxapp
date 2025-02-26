using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        private const decimal LowFundsThreshold = 500m;
        private const decimal DailyWithdrawalLimit = 1000m;
        private const decimal MinimumBalance = 100m;

        public decimal DailyWithdrawn { get; private set; }
        public Guid Id { get; private set; }

        public User User { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }

        public Account(Guid id, User user, decimal balance, decimal withdrawn, decimal paidIn)
        {
            Id = id;
            User = user;
            Balance = balance;
            Withdrawn = withdrawn;
            PaidIn = paidIn;
        }
        public void Withdraw(decimal amount, INotificationService notificationService)
        {
            if (Balance - amount < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make withdrawal/transfer");
            }

            if (DailyWithdrawn + amount > DailyWithdrawalLimit)
            {
                throw new InvalidOperationException("Daily withdrawal limit exceeded");
            }

            if (Balance - amount < MinimumBalance)
            {
                throw new InvalidOperationException($"Cannot withdraw, minimum balance of {MinimumBalance} required.");
            }

            Balance -= amount;
            Withdrawn -= amount;
            DailyWithdrawn += amount;

            if (Balance < LowFundsThreshold)
            {
                notificationService.NotifyFundsLow(User.Email);
            }
        }
        public void Deposit(decimal amount, INotificationService notificationService)
        {
            var newPaidIn = PaidIn + amount;
            if (newPaidIn > PayInLimit)
            {
                throw new InvalidOperationException("Account pay-in limit reached");
            }

            Balance += amount;
            PaidIn += amount;

            if (PayInLimit - PaidIn < LowFundsThreshold)
            {
                notificationService.NotifyApproachingPayInLimit(User.Email);
            }
        }
    }
}
