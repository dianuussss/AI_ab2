namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class Book
{
    public Book(BookIdentifier identifier, int editionSize, int initialCopies)
    {
        Identifier = identifier;
        EditionSize = editionSize;
        StoredCopies = initialCopies;
        AvailableCopies = initialCopies;
    }

    public BookIdentifier Identifier { get; }

    public int EditionSize { get; }

    public int StoredCopies { get; private set; }

    public int AvailableCopies { get; private set; }

    public void AddCopies(int amount)
    {
        StoredCopies += amount;
        AvailableCopies += amount;
    }

    public void BorrowCopy()
    {
        if (AvailableCopies <= 0)
            throw new InvalidOperationException("No available copies.");

        AvailableCopies--;
    }

    public void ReturnCopy()
    {
        if (AvailableCopies >= StoredCopies)
            throw new InvalidOperationException("Cannot return more copies than are out on loan.");

        AvailableCopies++;
    }
}
