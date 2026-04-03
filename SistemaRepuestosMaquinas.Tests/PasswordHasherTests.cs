using SistemaRepuestosMaquinas.Business.Security;

namespace SistemaRepuestosMaquinas.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashAndVerify_ShouldReturnTrue_ForValidPassword()
    {
        const string password = "Admin123*";
        var hash = PasswordHasher.Hash(password);

        var valid = PasswordHasher.Verify(password, hash, out var requiresRehash);

        Assert.True(valid);
        Assert.False(requiresRehash);
    }
}
