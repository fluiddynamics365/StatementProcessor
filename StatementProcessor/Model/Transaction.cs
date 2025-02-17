using System.Security.Cryptography;
using System.Text;

namespace StatementProcessor.Model;

public class Transaction
{
    public DateTime TxDate { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public string? Memo { get; set; }
    public string? CardHolder { get; set; }
    public required string Source { get; set; }

    public override string ToString()
    {
        return UniqueId + ", " + TxDate + ", " + Description + ", " + Amount + ", " + Memo + ", " + Source + ", " + CardHolder + ", " + UploadResult;
    }
    
    public string? UniqueId { get; private set; }
    public string? UploadResult { get; set; }

    public void GenerateId(int counter = 0)
    {
        string combinedData;
        combinedData = $"{TxDate}-{Description}-{Amount}-{Source}-{counter}";
        using (var md5 = MD5.Create())
        {
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combinedData));
            UniqueId = BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // This is the unique ID
        }
    }

    public IList<object> AsValueRange<TResult>()
    {
        return new List<object>()
        {
            UniqueId,
            TxDate.ToString("yyyy-MM-dd"),
            Description,
            Memo,
            Amount,
            Source,
            CardHolder
        };
    }
}