using Microsoft.Extensions.Logging;

namespace StatementProcessor.Model;

public class AMEXStatement(ILogger<AMEXStatement> logger) : BankStatement(logger)
{
    protected override string DateField { get; } = "Date";
    protected override string DescriptionField { get; } = "Description";
    protected override string AmountField { get; } = "Amount";
    protected override IEnumerable<string> MemoFields { get; } = Array.Empty<string>();
    protected override string CardholderField { get; }
    protected override string SourceName { get; } = "AMEX";

    public override bool IsMatch(List<string> headers)
    {
        return headers.SequenceEqual(new List<string> { "Date", "Description", "Amount" });
    }
}