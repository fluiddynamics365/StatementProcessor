using StatementProcessor.Model;

namespace StatementProcessor.StatementConnector;

public interface IBankStatementFactory
{
    public IList<Transaction> GetTransactions();
    void UpdateAndArchive();
}