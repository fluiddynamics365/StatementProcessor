using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatementProcessor.OutputConnector;
using StatementProcessor.StatementConnector;

namespace StatementProcessor;

public class StatementProcessorService(
    IConfiguration configuration,
    ILogger<StatementProcessorService> logger,
    IOutputConnector outputConnector,
    IBankStatementFactory statementFactory)
{

    public void Run()
    {
        logger.LogInformation(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        logger.LogInformation("Statement processor service started");
        logger.LogDebug("Getting InputDirectory from configuration");
        var inputFilePath = configuration.GetValue<string>("InputDirectory");
        logger.LogDebug("InputDirectory: {inputFilePath}", inputFilePath);
        if (inputFilePath == null)
        {
            logger.LogCritical("Input directory could not be found - update appsettings.json");
            throw new ArgumentNullException(nameof(inputFilePath));
        }
        
        logger.LogCritical("Getting transactions");
        var aggregatedTransactions = statementFactory.GetTransactions(inputFilePath);
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