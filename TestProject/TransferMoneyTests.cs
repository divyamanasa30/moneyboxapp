using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App;
using System.Security.Principal;

namespace Testproject
{
    [TestFixture]
    public class TransferMoneyTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private TransferMoney _transferMoney;
        private Account _fromAccount;
        private Account _toAccount;
        private User _user;

        [SetUp]
        public void SetUp()
        {
            // Arrange
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _transferMoney = new TransferMoney(_mockAccountRepository.Object, _mockNotificationService.Object);

            _user = new User(Guid.NewGuid(), "John Doe", "johndoe@example.com");
            _fromAccount = new Account(Guid.NewGuid(), _user, 1000m, 0m, 0m); // Initial balance 1000m
            _toAccount = new Account(Guid.NewGuid(), _user, 500m, 0m, 0m);   // Initial balance 500m

            _mockAccountRepository.Setup(repo => repo.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
            _mockAccountRepository.Setup(repo => repo.GetAccountById(_toAccount.Id)).Returns(_toAccount);
        }

        [Test]
        public void Execute_ShouldTransferMoney_WhenTransferIsSuccessful()
        {
            // Arrange
            decimal amount = 100m;

            // Act
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, amount);

            // Assert
            Assert.AreEqual(900m, _fromAccount.Balance); // 1000 - 100 = 900
            Assert.AreEqual(600m, _toAccount.Balance);   // 500 + 100 = 600
            _mockAccountRepository.Verify(repo => repo.Update(_fromAccount), Times.Once);
            _mockAccountRepository.Verify(repo => repo.Update(_toAccount), Times.Once);
            _mockNotificationService.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Execute_ShouldThrowException_WhenInsufficientFunds()
        {
            // Arrange
            decimal amount = 2000m; // Greater than balance

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, amount));
            Assert.AreEqual("Insufficient funds to make withdrawal/transfer", ex.Message);
        }

        [Test]
        public void Execute_ShouldThrowException_WhenMinimumBalanceRequired()
        {
            // Arrange
            decimal amount = 950m; // Trying to leave less than 100m in the `from` account

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, amount));
            Assert.AreEqual("Cannot withdraw, minimum balance of 100 required.", ex.Message);
        }
    }
}
