using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Tests;

public class AdminAccountTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 9, 0, 0);

    private static AdminAccount NewAdmin() =>
        new("Nessa", "Pailaga", "admin", "hash", SecurityQuestion.FirstPetName, "answer-hash", Now);

    [Fact]
    public void Fifth_failed_login_locks_the_account_for_five_minutes()
    {
        var admin = NewAdmin();

        for (var i = 0; i < AdminAccount.MaxFailedAttempts - 1; i++)
            Assert.False(admin.RecordFailedLogin(Now));

        Assert.True(admin.RecordFailedLogin(Now));
        Assert.True(admin.IsLoginLockedOut(Now.AddMinutes(4)));
        Assert.False(admin.IsLoginLockedOut(Now.AddMinutes(6)));
    }

    [Fact]
    public void Successful_login_resets_failures()
    {
        var admin = NewAdmin();
        admin.RecordFailedLogin(Now);
        admin.RecordFailedLogin(Now);

        admin.RecordSuccessfulLogin(Now);

        Assert.Equal(0, admin.FailedLoginAttempts);
        Assert.Equal(Now, admin.LastLoginAt);
    }

    [Fact]
    public void Initials_come_from_first_and_last_name()
    {
        Assert.Equal("NP", NewAdmin().Initials);
    }
}
