
using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;

namespace TestProject
{
    public class WithdrawMoneyTest

    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private WithdrawMoney _withdrawMoney;
        private Account _account;

        [SetUp]
        public void SetUp()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _withdrawMoney = new WithdrawMoney(_mockAccountRepository.Object, _mockNotificationService.Object);

            _account = new Account(Guid.NewGuid(), new User(Guid.NewGuid(),"", "test@example.com"), 1000m, 0m, 0m);

        }

        [Test]
        public void Execute_ValidWithdrawal_ShouldSucceed()
        {
            // Arrange
            decimal amount = 100m;
            _mockAccountRepository.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);

            // Act
            _withdrawMoney.Execute(_account.Id, amount);

            // Assert
            Assert.AreEqual(900m, _account.Balance);
            _mockAccountRepository.Verify(repo => repo.Update(_account), Times.Once);
        }

        [Test]
        public void Execute_WithdrawalCausingLowBalance_ShouldSendNotification()
        {
            // Arrange
            decimal amount = 900m;
            _mockAccountRepository.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);

            // Act
            _withdrawMoney.Execute(_account.Id, amount);

            // Assert
            _mockNotificationService.Verify(service => service.NotifyFundsLow(_account.User.Email), Times.Once);
        }

        [Test]
        public void Execute_WithdrawalFails_ShouldLogError()
        {
            // Arrange
            decimal amount = 500m;
            _mockAccountRepository.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);
            _mockAccountRepository.Setup(repo => repo.Update(It.IsAny<Account>())).Throws(new Exception("Database update failed"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _withdrawMoney.Execute(_account.Id, amount));
            Assert.That(ex.Message, Is.EqualTo("Database update failed"));
        }
    }
}