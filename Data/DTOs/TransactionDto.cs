using System.Text.Json.Serialization;

namespace BankAPI.Data.DTOs;

public class TransactionDto
{
    public int AccountId { get; set; }
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
    public int? ExternalAccount { get; set; }
}