using Itmo.ObjectOrientedProgramming.Lab1.Application.Abstractions;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Presentation.Controllers;

public class LibraryController : ILibraryController
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public User RegisterUser(string name, IEnumerable<UserRole> roles)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return _libraryService.RegisterUser(name, roles);
    }

    public LibraryRequest SubmitAddBook(string writerName, string title, string authorName, int editionSize, int quantity)
    {
        if (editionSize <= 0)
            throw new ArgumentException("Edition size must be positive.", nameof(editionSize));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        return _libraryService.CreateAddBookRequest(writerName, title, authorName, editionSize, quantity);
    }

    public LibraryRequest SubmitBorrowBook(string readerName, string title, string authorName)
    {
        return _libraryService.CreateBorrowBookRequest(readerName, title, authorName);
    }

    public LibraryRequest SubmitReturnBook(string readerName, string title, string authorName)
    {
        return _libraryService.CreateReturnBookRequest(readerName, title, authorName);
    }

    public LibraryRequest Approve(string librarianName, int requestId)
    {
        return _libraryService.ApproveRequest(librarianName, requestId);
    }

    public LibraryRequest Reject(string librarianName, int requestId)
    {
        return _libraryService.RejectRequest(librarianName, requestId);
    }

    public LibraryRequest Cancel(string requesterName, int requestId)
    {
        if (string.IsNullOrWhiteSpace(requesterName))
            throw new ArgumentException("Requester name cannot be empty.", nameof(requesterName));

        return _libraryService.CancelRequest(requesterName, requestId);
    }

    public LibraryRequest? GetRequest(int requestId) => _libraryService.FindRequest(requestId);

    public Book? GetBook(BookIdentifier identifier) => _libraryService.FindBook(identifier);
}
