using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StatementProcessor.Model;

namespace StatementProcessor.StatementConnector;

public class CSVBankStatementFactory(IFileSystem fileSystem, IOptions<AppSettings> settings, ILogger<StatementProcessorService> logger, IServiceProvider serviceProvider) : IBankStatementFactory
{
    private static IList<CSVFile> _files = [];

    public IList<Transaction> GetTransactions()
    {
        _files = GetInputFiles(settings.Value.InputDirectory);
        logger.LogDebug("Found {fileCount} files to process", _files.Count);
        var aggregatedTransactions = new List<Transaction>();
        
        foreach (var statementFilePath in _files)
        {
            try
            {
                var transactions = GetTransactionsFromCsvFile(statementFilePath.FilePath);
            
                logger.LogTrace("Transactions for {statementFilePath}", statementFilePath);
                foreach (var transaction in transactions)
                {
                    logger.LogTrace(transaction.ToString());
                }

                aggregatedTransactions.AddRange(transactions);
                
                statementFilePath.ProcessedSuccessfully = true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to read file {filePath}", statementFilePath);
            }
        }
        
        return aggregatedTransactions;
    }
    
    public void UpdateAndArchive()
    {
        foreach (var file in _files.Where(f => f.ProcessedSuccessfully))
        {
            fileSystem.File.Move(
                file.FilePath,
                fileSystem.Path.Combine(
                    settings.Value.ProcessedDirectory,
                    fileSystem.Path.GetFileName(file.FilePath)));
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
            var instance = (BankStatement)ActivatorUtilities.CreateInstance(serviceProvider, type);
            if (!instance.IsMatch(headers)) continue;
            logger.LogInformation("Identified file type as {typeName}", type.Name);
            instance.LoadTransactions(csv.GetRecords<dynamic>());
            return instance.GetTransactions();
        }

        throw new Exception($"No statement type found for file {statementFilePath}");
    }

    private List<CSVFile> GetInputFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var files = Directory.GetFiles(directory, "*.csv").Select(f => new CSVFile() { FilePath = f }).ToList();
        logger.LogInformation("Found {files} files in input folder.", files.Count);

        return files;
    }
}