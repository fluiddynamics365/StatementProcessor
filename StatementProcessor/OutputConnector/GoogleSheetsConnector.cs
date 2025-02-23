using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;git 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using StatementProcessor.Model;

namespace StatementProcessor.OutputConnector;

public class GoogleSheetsConnector(IOptions<AppSettings> settings, ILogger<StatementProcessorService> logger)
    : IOutputConnector
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private const string ApplicationName = "Statement Processor";
    private const string SpreadsheetId = "1GtQR4ISNCY-2Otsi0XNEs0EYoobOXHGjPk0Ghfrsymk";
    private const string Sheet = "Transactions";
    private const string Range = $"{Sheet}!A2:G";
    private SheetsService _service;

    private IList<GSheetTransaction> RetrieveTransactions()
    {
        // Get the "GoogleConnection" section as a JSON string
        /*var googleConnectionSection = configuration.GetSection("GoogleSheetsConnection")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(kv => kv.Key, kv => kv.Value);*/

        var googleCredentialsJson = JsonSerializer.Serialize(settings.Value.GoogleSheetsConnection);// JsonConvert.SerializeObject(googleConnectionSection);
        // Load Google credentials
        var credential = GoogleCredential.FromJson(googleCredentialsJson).CreateScoped(Scopes);

        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    

    var request = _service.Spreadsheets.Values.Get(SpreadsheetId, Range);
    var response = request.Execute();

    var values = response.Values ?? new List<IList<object>>();
        return values.Select(t => new GSheetTransaction(t)).ToList();
}

public void AddTransactions(IEnumerable<Transaction> transactions)
{
    var existingTransactions = RetrieveTransactions();
    logger.LogInformation("Retrieved {existingTransactions} existing transactions",
        existingTransactions?.Count ?? 0);

    var transactionsToAdd = transactions.Where(transaction =>
        !existingTransactions.Any(t => t[Literals.Fields.TransactionId].ToString() == transaction.UniqueId)).ToList();

    var valueRange = new ValueRange
    {
        Values = transactionsToAdd.Select(t => t.AsValueRange<IList<object>>()).ToList()
    };

    var appendRequest = _service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, Range);
    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
    var appendResponse = appendRequest.Execute();

    logger.LogDebug(appendResponse.ToString());
}

public IEnumerable<string> GetInputFiles(string directory)
{
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    var files = new List<string>(Directory.GetFiles(directory, "*.csv"));
    //Logger.LogInformation($"Found {files.Count} files in input folder.");

    return files;
}

public void MoveFileToProcessedFolder(string statementPath, string processedDirectory)
{
    if (!Directory.Exists(processedDirectory))
    {
        Directory.CreateDirectory(processedDirectory);
    }

    File.Move(statementPath, Path.Combine(processedDirectory, Path.GetFileName(statementPath)));
}

}