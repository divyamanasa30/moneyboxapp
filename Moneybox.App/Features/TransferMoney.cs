using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);
            try {
                from.Withdraw(amount, this.notificationService);
                to.Deposit(amount, this.notificationService);
                this.accountRepository.Update(from);
                this.accountRepository.Update(to);
                Console.WriteLine($"Transfer successful from Account {fromAccountId} to {toAccountId}, Amount: {amount}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Transfer failed from Account {fromAccountId} to {toAccountId}: {ex.Message}");
                throw;
            }
        }
    }
}
