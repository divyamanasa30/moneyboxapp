    using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            try
            {
                from.Withdraw(amount, this.notificationService);
                this.accountRepository.Update(from);
                Console.WriteLine($"Withdrawal successful: Account {fromAccountId}, Amount: {amount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Withdrawal failed for Account {fromAccountId}: {ex.Message}");
                throw;
            }
        }
    }
}
