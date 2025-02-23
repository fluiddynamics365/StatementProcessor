using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StatementProcessor.Model;
using StatementProcessor.OutputConnector;
using StatementProcessor.StatementConnector;

namespace StatementProcessor.Tests
{
    [TestFixture]
    public class StatementProcessorServiceTests
    {
        private Mock<IOutputConnector> _mockOutputConnector;
        private Mock<IBankStatementFactory> _mockStatementFactory;
        private Mock<ILogger<StatementProcessorService>> _mockLogger;
        private StatementProcessorService _correctlyConfiguredService;
        private IOptions<AppSettings> _settings;

        [SetUp]
        public void SetUp()
        {
            _mockOutputConnector = new Mock<IOutputConnector>();
            _mockStatementFactory = new Mock<IBankStatementFactory>();
            _mockLogger = new Mock<ILogger<StatementProcessorService>>();
            _settings = Options.Create(new AppSettings()
            {
                InputDirectory = "test-path"
            });
           
            _correctlyConfiguredService = new StatementProcessorService(
                _settings,
                _mockLogger.Object,
                _mockOutputConnector.Object,
                _mockStatementFactory.Object
            );
        }

        [Test]
        public void Run_ShouldThrowException_WhenInputDirectoryIsNull()
        {
            var removedValueSettings = Options.Create(new AppSettings());

            var service = new StatementProcessorService(
                removedValueSettings,
                _mockLogger.Object,
                _mockOutputConnector.Object,
                _mockStatementFactory.Object
            );

            Assert.Throws<ArgumentNullException>(() => service.Run());

            _mockStatementFactory.Verify(sf => sf.GetTransactions(), Times.Never);
            _mockOutputConnector.Verify(oc => oc.AddTransactions(It.IsAny<IEnumerable<Transaction>>()), Times.Never);
        }

        [Test]
        public void Run_ShouldProcess_WhenInputDirectoryIsValid()
        {
            var transactions = new List<Transaction>
            {
                new()
                {
                    TxDate = DateTime.Parse("2024-01-01"),
                    Amount = 200,
                    Description = "Description 1",
                    Source = "Credit card 1"
                },
                new()
                {
                    TxDate = DateTime.Parse("2024-02-01"),
                    Amount = 100,
                    Description = "Description 2",
                    Source = "Credit card 2"
                }
            };
            
            
            _mockStatementFactory.Setup(sf => sf.GetTransactions()).Returns(transactions);
                
            _correctlyConfiguredService.Run();

            _mockStatementFactory.Verify(sf => sf.GetTransactions(), Times.Once);
            _mockOutputConnector.Verify(oc => oc.AddTransactions(It.Is<IEnumerable<Transaction>>(t => t.First().TxDate == DateTime.Parse("2024-01-01"))), Times.Once);
        }

        [Test]
        public void Run_ShouldNotSendToOutputConnector_WhenZeroRecordsReturned()
        {
            _mockStatementFactory.Setup(sf => sf.GetTransactions()).Returns(new List<Transaction>());
                
            _correctlyConfiguredService.Run();

            _mockStatementFactory.Verify(sf => sf.GetTransactions(), Times.Once);
            _mockOutputConnector.Verify(oc => oc.AddTransactions(It.IsAny<IEnumerable<Transaction>>()), Times.Never);
        }
    }
}
