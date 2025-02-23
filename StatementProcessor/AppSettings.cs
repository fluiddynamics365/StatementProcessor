namespace StatementProcessor;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string InputDirectory { get; set; }
    public string ProcessedDirectory { get; set; }
    public Logging Logging { get; set; }
    public GoogleSheetsConnection GoogleSheetsConnection { get; set; }
    public string ApplicationName { get; set; }
    public string SpreadsheetId { get; set; }
    public string Sheet { get; set; }
    public string InputRange { get; set; }
    public string SortRange { get; set; }
}

public class Logging
{
    public LogLevel LogLevel { get; set; }
}

public class LogLevel
{
    public string Default { get; set; }
}

public class GoogleSheetsConnection
{
    public string type { get; set; }
    public string project_id { get; set; }
    public string private_key_id { get; set; }
    public string private_key { get; set; }
    public string client_email { get; set; }
    public string client_id { get; set; }
    public string auth_uri { get; set; }
    public string token_uri { get; set; }
    public string auth_provider_x509_cert_url { get; set; }
    public string client_x509_cert_url { get; set; }
    public string universe_domain { get; set; }
}