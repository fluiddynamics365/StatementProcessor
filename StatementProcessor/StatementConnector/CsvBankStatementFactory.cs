using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StatementProcessor.Model;

namespace StatementProcessor.StatementConnector;

public class CSVBankStatementFactory(IFileSystem fileSystem, IOptions<AppSettings> settings, ILogger<StatementProcessorService> logger) : IBankStatementFactory
{
    private static List<string> _files = [];

    public IList<Transaction> GetTransactions(string inputFilePath)
    {
        var statementFilePaths = GetInputFiles(inputFilePath);
        var aggregatedTransactions = new List<Transaction>();
        
        foreach (var statementFilePath in statementFilePaths)
        {
            var transactions = GetTransactionsFromCsvFile(statementFilePath);
            
            logger.LogDebug("Transactions for {statementFilePath}", statementFilePath);
            foreach (var transaction in transactions)
            {
                logger.LogTrace(transaction.ToString());
            }

            aggregatedTransactions.AddRange(transactions);
        }
        
        return aggregatedTransactions;
    }
    
    public void UpdateAndArchive()
    {
        foreach (var file in _files)
        {
            fileSystem.File.Move(
                file,
                fileSystem.Path.Combine(
                    settings.Value.ProcessedDirectory,
                    fileSystem.Path.GetFileName(file)));
        }
    }

    private List<Transaction> GetTransactionsFromCsvFile(string statementFilePath)
    {
        logger.LogDebug("Identifying file type...");
        var csvString = fileSystem.File.ReadAllText(statementFilePath);

        using var csv = new CsvReader(new StringReader(csvString), CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        if (csv.HeaderRecord == null) throw new Exception($"Problem reading file: {statementFilePath}");
        var headers = csv.HeaderRecord.ToList();

        var statementTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BankStatement)) && !t.IsAbstract);

        foreach (var type in statementTypes)
        {
            var instance = (BankStatement)Activator.CreateInstance(type);
            if (!instance.IsMatch(headers)) continue;
            logger.LogInformation($"Identified file type as {type.Name}");
            instance.LoadTransactions(csv.GetRecords<dynamic>());
            return instance.GetTransactions();
        }

        throw new Exception($"No statement type found for file {statementFilePath}");
    }


    private List<string> GetInputFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _files = [..Directory.GetFiles(directory, "*.csv")];
        logger.LogInformation("Found {files} files in input folder.",_files.Count);

        return _files;
    }
}