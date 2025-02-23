using Microsoft.Extensions.Logging;

namespace StatementProcessor.Model;

public class AMEXExtStatement(ILogger<AMEXExtStatement> logger) : BankStatement(logger)
{
    protected override string DateField { get; } = "Date";
    protected override string DescriptionField => "Description";
    protected override string AmountField => "Amount";

    protected override IEnumerable<string> MemoFields { get; } = new List<string>
    {
        "Account #", "Extended Details","Card Member",
        "Address", "Town/City", "Postcode", "Country", "Reference", "Category"
    };

    protected override string CardholderField => "Card Member";
    protected override string SourceName { get; } = "AMEX";

    public override bool IsMatch(List<string> headers)
    {
        return headers.SequenceEqual(new List<string>
        {
            "Date", "Description", "Card Member", "Account #", "Amount", "Extended Details",
            "Appears On Your Statement As", "Address", "Town/City", "Postcode", "Country", "Reference", "Category"
        });
    }
}