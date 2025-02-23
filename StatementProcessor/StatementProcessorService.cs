using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StatementProcessor.OutputConnector;
using StatementProcessor.StatementConnector;

namespace StatementProcessor;

public class StatementProcessorService(
    IOptions<AppSettings> settings,
    ILogger<StatementProcessorService> logger,
    IOutputConnector outputConnector,
    IBankStatementFactory statementFactory)
{

    public void Run()
    {
        logger.LogInformation(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        logger.LogInformation("Statement processor service started");
        logger.LogDebug("Getting InputDirectory from configuration");
        var inputDirectory =  settings.Value.InputDirectory;
        logger.LogDebug("InputDirectory: {inputFilePath}", inputDirectory);
        if (inputDirectory == null)
        {
            logger.LogCritical("Input directory could not be found - update appsettings.json");
            throw new ArgumentNullException(nameof(inputDirectory));
        }
        
        logger.LogCritical("Getting transactions");
        var aggregatedTransactions = statementFactory.GetTransactions(inputDirectory);
        logger.LogDebug("Total {transactionCount} transactions found", aggregatedTransactions.Count);
        if (aggregatedTransactions.Any())
        {
            outputConnector.AddTransactions(aggregatedTransactions.OrderBy(t => t.TxDate));
        }
        else
        {
            logger.LogWarning("No transactions found");
        }
        
        statementFactory.UpdateAndArchive();
    }
}