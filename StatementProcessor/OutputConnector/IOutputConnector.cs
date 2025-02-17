using StatementProcessor.Model;

namespace StatementProcessor.OutputConnector;

public interface IOutputConnector
{
    void AddTransactions(IEnumerable<Transaction> transactions);
}