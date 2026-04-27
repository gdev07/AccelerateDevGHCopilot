using Library.ApplicationCore;
using Library.ApplicationCore.Entities;

namespace Library.Infrastructure.Data;

public class JsonLoanRepository : ILoanRepository
{
    private readonly JsonData _jsonData;

    public JsonLoanRepository(JsonData jsonData)
    {
        _jsonData = jsonData;
    }

    public async Task<Loan?> GetLoan(int id)
    {
        await _jsonData.EnsureDataLoaded();

        foreach (Loan loan in _jsonData.Loans!)
        {
            if (loan.Id == id)
            {
                Loan populated = _jsonData.GetPopulatedLoan(loan);
                return populated;
            }
        }
        return null;
    }

    public async Task UpdateLoan(Loan loan)
    {
        Loan? existingLoan = null;
        foreach (Loan l in _jsonData.Loans!)
        {
            if (l.Id == loan.Id)
            {
                existingLoan = l;
                break;
            }
        }

        if (existingLoan != null)
        {
            existingLoan.BookItemId = loan.BookItemId;
            existingLoan.PatronId = loan.PatronId;
            existingLoan.LoanDate = loan.LoanDate;
            existingLoan.DueDate = loan.DueDate;
            existingLoan.ReturnDate = loan.ReturnDate;

            await _jsonData.SaveLoans(_jsonData.Loans!);

            await _jsonData.LoadData();
        }
    }
        public async Task<Loan?> GetActiveLoanByBookTitle(string bookTitle)
    {
        await _jsonData.EnsureDataLoaded();
    
        // Step 1: Find the book by title (case-insensitive)
        Book? matchingBook = _jsonData.Books?.FirstOrDefault(b => 
            b.Title.Equals(bookTitle, StringComparison.OrdinalIgnoreCase));
        
        if (matchingBook == null)
            return null; // Book not found
    
        // Step 2: Find all BookItems for this book
        List<BookItem> bookItems = _jsonData.BookItems?
            .Where(bi => bi.BookId == matchingBook.Id)
            .ToList() ?? new List<BookItem>();
    
        // Step 3: Find an active loan (ReturnDate == null) for any of these BookItems
        foreach (Loan loan in _jsonData.Loans!)
        {
            if (bookItems.Any(bi => bi.Id == loan.BookItemId) && loan.ReturnDate == null)
            {
                // Return the populated loan with book details
                return _jsonData.GetPopulatedLoan(loan);
            }
        }
        
        return null; // Book is available
    }
}