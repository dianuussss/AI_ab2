namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class LibraryRequest
{
    public LibraryRequest(int id, RequestType type, string creatorName, BookIdentifier book, int quantity, int editionSize)
    {
        Id = id;
        Type = type;
        CreatorName = creatorName;
        Book = book;
        Quantity = quantity;
        EditionSize = editionSize;
        Status = RequestStatus.Pending;
    }

    public int Id { get; }

    public RequestType Type { get; }

    public string CreatorName { get; }

    public BookIdentifier Book { get; }

    public int Quantity { get; }

    public int EditionSize { get; }

    public RequestStatus Status { get; private set; }

    public void Approve()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        Status = RequestStatus.Approved;
    }

    public void Reject()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        Status = RequestStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be cancelled.");

        Status = RequestStatus.Cancelled;
    }
}
