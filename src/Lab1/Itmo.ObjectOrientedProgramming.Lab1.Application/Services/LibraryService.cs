using Itmo.ObjectOrientedProgramming.Lab1.Application.Abstractions;
using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;
using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IRequestRepository _requestRepository;
    private int _nextRequestId = 1;

    public LibraryService(
        IUserRepository userRepository,
        IBookRepository bookRepository,
        IRequestRepository requestRepository)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _requestRepository = requestRepository;
    }

    public User RegisterUser(string name, IEnumerable<UserRole> roles)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var roleList = new List<UserRole>(roles);
        if (roleList.Count == 0)
            throw new ArgumentException("At least one role is required.", nameof(roles));

        foreach (UserRole role in roleList)
        {
            if (role == UserRole.Undefined)
                throw new ArgumentException("Undefined role is not allowed.", nameof(roles));
        }

        if (_userRepository.Exists(name))
            throw new BusinessRuleViolationException("User with this name already exists.");

        var user = new User(name, roleList);
        _userRepository.Add(user);
        return user;
    }

    public LibraryRequest CreateAddBookRequest(
        string writerName,
        string title,
        string authorName,
        int editionSize,
        int quantity)
    {
        if (editionSize <= 0)
            throw new ArgumentException("Edition size must be positive.", nameof(editionSize));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (string.IsNullOrWhiteSpace(writerName))
            throw new BusinessRuleViolationException("Writer name is required.");

        User? writer = _userRepository.Find(writerName)
            ?? throw new BusinessRuleViolationException("Writer not found.");

        if (!writer.HasRole(UserRole.Writer))
            throw new BusinessRuleViolationException("Only writers can add books.");

        if (writerName != authorName)
            throw new BusinessRuleViolationException("Writer can add only their own books.");

        if (quantity > editionSize)
            throw new BusinessRuleViolationException("Quantity cannot exceed edition size.");

        var bookId = new BookIdentifier(title, authorName);
        Book? existingBook = _bookRepository.Find(bookId);
        if (existingBook != null && existingBook.StoredCopies + quantity > existingBook.EditionSize)
            throw new BusinessRuleViolationException("Not enough room left in the edition.");

        int id = _nextRequestId;
        var request = new LibraryRequest(id, RequestType.AddBook, writerName, bookId, quantity, editionSize);

        bool auto = writer.HasRole(UserRole.Writer) && writer.HasRole(UserRole.Librarian);
        if (auto)
        {
            ApplyAddBookApproval(request);
            request.Approve();
        }

        CommitRequest(request);
        return request;
    }

    public LibraryRequest CreateBorrowBookRequest(string readerName, string title, string authorName)
    {
        if (string.IsNullOrWhiteSpace(readerName))
            throw new BusinessRuleViolationException("Reader name is required.");

        User? reader = _userRepository.Find(readerName)
            ?? throw new BusinessRuleViolationException("Reader not found.");

        if (!reader.HasRole(UserRole.Reader))
            throw new BusinessRuleViolationException("Only readers can borrow books.");

        var bookId = new BookIdentifier(title, authorName);

        if (reader.HasBorrowed(bookId))
            throw new BusinessRuleViolationException("Reader already borrowed this book.");

        bool auto = reader.HasRole(UserRole.Reader) && reader.HasRole(UserRole.Librarian);
        if (auto)
        {
            Book? book = _bookRepository.Find(bookId);
            if (book == null || book.AvailableCopies <= 0)
                throw new BusinessRuleViolationException("Book is not available for borrowing.");

            int id = _nextRequestId;
            var request = new LibraryRequest(id, RequestType.BorrowBook, readerName, bookId, 1, 0);
            ApplyBorrowBookApproval(request);
            request.Approve();
            CommitRequest(request);
            return request;
        }

        int pendingId = _nextRequestId;
        var pendingRequest = new LibraryRequest(pendingId, RequestType.BorrowBook, readerName, bookId, 1, 0);
        CommitRequest(pendingRequest);
        return pendingRequest;
    }

    public LibraryRequest CreateReturnBookRequest(string readerName, string title, string authorName)
    {
        User? reader = _userRepository.Find(readerName)
            ?? throw new BusinessRuleViolationException("Reader not found.");

        if (!reader.HasRole(UserRole.Reader))
            throw new BusinessRuleViolationException("Only readers can return books.");

        var bookId = new BookIdentifier(title, authorName);

        if (!reader.HasBorrowed(bookId))
            throw new BusinessRuleViolationException("Reader did not borrow this book.");

        int id = _nextRequestId;
        var request = new LibraryRequest(id, RequestType.ReturnBook, readerName, bookId, 1, 0);

        bool auto = reader.HasRole(UserRole.Reader) && reader.HasRole(UserRole.Librarian);
        if (auto)
        {
            ApplyReturnBookApproval(request);
            request.Approve();
        }

        CommitRequest(request);
        return request;
    }

    public LibraryRequest ApproveRequest(string librarianName, int requestId)
    {
        RequireLibrarian(librarianName);
        LibraryRequest request = RequirePendingRequest(requestId);

        switch (request.Type)
        {
            case RequestType.AddBook:
                ApplyAddBookApproval(request);
                break;
            case RequestType.BorrowBook:
                ApplyBorrowBookApproval(request);
                break;
            case RequestType.ReturnBook:
                ApplyReturnBookApproval(request);
                break;
            default:
                throw new BusinessRuleViolationException("Unknown request type.");
        }

        request.Approve();
        return request;
    }

    public LibraryRequest RejectRequest(string librarianName, int requestId)
    {
        RequireLibrarian(librarianName);
        LibraryRequest request = RequirePendingRequest(requestId);
        request.Reject();
        return request;
    }

    public LibraryRequest? FindRequest(int requestId) => _requestRepository.Find(requestId);

    public Book? FindBook(BookIdentifier identifier) => _bookRepository.Find(identifier);

    private User RequireLibrarian(string librarianName)
    {
        if (string.IsNullOrWhiteSpace(librarianName))
            throw new BusinessRuleViolationException("Librarian name is required.");

        User? librarian = _userRepository.Find(librarianName)
            ?? throw new BusinessRuleViolationException("Librarian not found.");

        if (!librarian.HasRole(UserRole.Librarian))
            throw new BusinessRuleViolationException("Only librarians can process requests.");

        return librarian;
    }

    private LibraryRequest RequirePendingRequest(int requestId)
    {
        LibraryRequest? request = _requestRepository.Find(requestId)
            ?? throw new BusinessRuleViolationException("Request not found.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleViolationException("Request is not pending.");

        return request;
    }

    private void CommitRequest(LibraryRequest request)
    {
        _requestRepository.Add(request);
        _nextRequestId = request.Id + 1;
    }

    private void ApplyAddBookApproval(LibraryRequest request)
    {
        Book? existing = _bookRepository.Find(request.Book);
        if (existing == null)
        {
            var book = new Book(request.Book, request.EditionSize, 0);
            book.AddCopies(request.Quantity);
            _bookRepository.Add(book);
            return;
        }

        if (existing.EditionSize != request.EditionSize)
            throw new BusinessRuleViolationException("Edition size mismatch.");

        if (existing.StoredCopies + request.Quantity > existing.EditionSize)
            throw new BusinessRuleViolationException("Cannot store more copies than the edition allows.");

        existing.AddCopies(request.Quantity);
    }

    private void ApplyBorrowBookApproval(LibraryRequest request)
    {
        User? reader = _userRepository.Find(request.CreatorName)
            ?? throw new BusinessRuleViolationException("Reader not found.");

        Book? book = _bookRepository.Find(request.Book)
            ?? throw new BusinessRuleViolationException("Book is not in the library.");

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleViolationException("No available copies.");

        book.BorrowCopy();
        reader.Borrow(request.Book);
    }

    private void ApplyReturnBookApproval(LibraryRequest request)
    {
        User? reader = _userRepository.Find(request.CreatorName)
            ?? throw new BusinessRuleViolationException("Reader not found.");

        if (!reader.HasBorrowed(request.Book))
            throw new BusinessRuleViolationException("Reader did not borrow this book.");

        Book? book = _bookRepository.Find(request.Book)
            ?? throw new BusinessRuleViolationException("Book is not in the library.");

        book.ReturnCopy();
        reader.Return(request.Book);
    }
}
