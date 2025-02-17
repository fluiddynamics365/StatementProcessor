using StatementProcessor.Model;

namespace StatementProcessor.Tests
{
    [TestFixture]
    public class TransactionTests
    {
        [Test]
        public void GenerateId_DifferentValues_ProducesDistinctUniqueId()
        {
            // Arrange
            var tx1 = new Transaction
            {
                TxDate = new DateTime(2023, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            var tx2 = new Transaction
            {
                TxDate = new DateTime(2024, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            // Act
            tx1.GenerateId();
            tx2.GenerateId();

            // Assert
            Assert.That(tx2.UniqueId, Is.Not.EqualTo(tx1.UniqueId),
                "UniqueId should be different for different transactions.");
        }

        [Test]
        public void GenerateId_SameValues_ProducesSameUniqueId()
        {
            // Arrange
            var tx1 = new Transaction
            {
                TxDate = new DateTime(2024, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            var tx2 = new Transaction
            {
                TxDate = new DateTime(2024, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            // Act
            tx1.GenerateId();
            tx2.GenerateId();

            // Assert
            Assert.That(tx2.UniqueId, Is.EqualTo(tx1.UniqueId), "UniqueId should be the same for identical transactions.");
        }

        [Test]
        public void GenerateId_DifferentCounterValues_ProducesDifferentUniqueIds()
        {
            // Arrange
            var tx1 = new Transaction
            {
                TxDate = new DateTime(2024, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            var tx2 = new Transaction
            {
                TxDate = new DateTime(2024, 2, 9),
                Description = "Test Transaction",
                Amount = 100.50m,
                Memo = "Sample Memo",
                CardHolder = "John Doe",
                Source = "Bank A"
            };

            // Act
            tx1.GenerateId(1);
            tx2.GenerateId(2);

            // Assert
            Assert.That(tx2.UniqueId, Is.Not.EqualTo(tx1.UniqueId), "UniqueId should be different when counter values are different.");
        }
    }
}
