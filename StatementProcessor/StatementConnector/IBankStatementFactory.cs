using StatementProcessor.Model;

namespace StatementProcessor.StatementConnector;

public interface IBankStatementFactory
{
    public IList<Transaction> GetTransactions(string inputFilePath);
    public void UpdateAndArchive(IList<BankStatement> bankStatements);
}