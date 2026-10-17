using System.Collections.ObjectModel;

namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class User
{
    private readonly HashSet<UserRole> _roles;
    private readonly HashSet<BookIdentifier> _borrowedBooks;

    public User(string name, IEnumerable<UserRole> roles)
    {
        Name = name;
        _roles = new HashSet<UserRole>(roles);
        _borrowedBooks = new HashSet<BookIdentifier>();
    }

    public string Name { get; }

    public IReadOnlyCollection<UserRole> Roles => new ReadOnlyCollection<UserRole>(new List<UserRole>(_roles));

    public bool HasRole(UserRole role) => _roles.Contains(role);

    public bool HasBorrowed(BookIdentifier identifier) => _borrowedBooks.Contains(identifier);

    public void Borrow(BookIdentifier identifier) => _borrowedBooks.Add(identifier);

    public void Return(BookIdentifier identifier) => _borrowedBooks.Remove(identifier);
}
