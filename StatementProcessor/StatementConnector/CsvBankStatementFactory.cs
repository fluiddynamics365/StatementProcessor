using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatementProcessor.Model;

namespace StatementProcessor.StatementConnector;

public class CSVBankStatementFactory(IFileSystem fileSystem, IConfiguration configuration, ILogger<StatementProcessorService> logger) : IBankStatementFactory
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public IList<Transaction> GetTransactions(string inputFilePath)
    {
        var statementFilePaths = GetInputFiles(inputFilePath);
        var aggregatedTransactions = new List<Transaction>();
        
        foreach (var statementFilePath in statementFilePaths)
        {
            var transactions = GetTransactionsFromCsvFile(statementFilePath);
            
            logger.LogDebug($"Transactions for {statementFilePath}", statementFilePath);
            foreach (var transaction in transactions)
            {
                logger.LogDebug(transaction.ToString());
            }

            aggregatedTransactions.AddRange(transactions);
        }
        
        return aggregatedTransactions;
    }
    
    
    public void UpdateAndArchive(IList<BankStatement> bankStatements)
    {
        throw new NotImplementedException();
    }

    private List<Transaction> GetTransactionsFromCsvFile(string statementFilePath)
    {
        logger.LogDebug("Identifying file type...");
        var csvString = _fileSystem.File.ReadAllText(statementFilePath);

        using var csv = new CsvReader(new StringReader(csvString), CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        if (csv.HeaderRecord == null) throw new Exception($"No statement type found for file {statementFilePath}");
        var headers = csv.HeaderRecord.ToList();

        var statementTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BankStatement)) && !t.IsAbstract);

        foreach (var type in statementTypes)
        {
            var instance = (BankStatement)Activator.CreateInstance(type);
            if (!instance.IsMatch(headers)) continue;
            //              Logger.LogInformation($"Identified file type as {type.Name}");
            instance.LoadTransactions(csv.GetRecords<dynamic>());
            return instance.GetTransactions();
        }

        throw new Exception($"No statement type found for file {statementFilePath}");
    }


    private static List<string> GetInputFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var files = new List<string>(Directory.GetFiles(directory, "*.csv"));
        //Logger.LogInformation($"Found {files.Count} files in input folder.");

        return files;
    }

    private void MoveFileToProcessedFolder(string statementPath, string processedDirectory)
    {
        if (!Directory.Exists(processedDirectory))
        {
            Directory.CreateDirectory(processedDirectory);
        }
        
        File.Move(statementPath, Path.Combine(processedDirectory, Path.GetFileName(statementPath)));
    }
}