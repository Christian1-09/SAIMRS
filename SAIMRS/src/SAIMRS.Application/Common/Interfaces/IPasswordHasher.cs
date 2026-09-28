namespace SAIMRS.Application.Common.Interfaces;

/// <summary>One-way hashing for passwords and security answers.</summary>
public interface IPasswordHasher
{
    string Hash(string value);

    bool Verify(string value, string hash);
}
