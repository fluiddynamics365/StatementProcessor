namespace StatementProcessor.Model;

public class GSheetTransaction
{
    private readonly IList<object> _values;

    private static readonly Dictionary<int, string> FieldMapByIndex = new()
    {
        { 0, "TransactionId" },
        { 1, "Date" },
        { 2, "Description" },
        { 3, "Memo" },
        { 4, "Amount" },
        { 5, "Card" },
        { 6, "Cardholder" }
    };
    
    private readonly Dictionary<string, int> _fieldMapByName = FieldMapByIndex.ToDictionary((i) => i.Value, (i) => i.Key);

    public GSheetTransaction(IList<object> values)
    {
        _values = values;
    }

    public object this[string fieldName] => _values[_fieldMapByName[fieldName]];
}