using Marten;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Shouldly;
using Xunit;

namespace DocumentDbTests.SessionMechanics;

public class dirty_tracked_sessions : IntegrationContext
{
    public dirty_tracked_sessions(DefaultStoreFixture fixture) : base(fixture)
    {
        DocumentTracking = DocumentTracking.DirtyTracking;
    }

    [Fact]
    public void store_and_update_a_document()
    {
        var user = new User { FirstName = "James", LastName = "Worthy" };

        theSession.Store(user);
        theSession.SaveChanges();

        using (var session2 = theStore.DirtyTrackedSession())
        {
            session2.ShouldNotBeSameAs(theSession);

            var user2 = session2.Load<User>(user.Id);
            user2.FirstName = "Jens";
            user2.LastName = "Pettersson";

            session2.SaveChanges();
        }

        using (var session3 = theStore.OpenSession())
        {
            var user3 = session3.Load<User>(user.Id);
            user3.FirstName.ShouldBe("Jens");
            user3.LastName.ShouldBe("Pettersson");
        }
    }

    [Fact]
    public void store_and_update_a_document_in_same_session()
    {
        var user = new User { FirstName = "James", LastName = "Worthy" };

        theSession.Store(user);
        theSession.SaveChanges();

        user.FirstName = "Jens";
        user.LastName = "Pettersson";
        theSession.SaveChanges();

        using var session3 = theStore.OpenSession();
        var user3 = session3.Load<User>(user.Id);
        user3.FirstName.ShouldBe("Jens");
        user3.LastName.ShouldBe("Pettersson");
    }


    [Fact]
    public void store_reload_and_update_a_document_in_same_dirty_tracked_session()
    {
        var user = new User { FirstName = "James", LastName = "Worthy" };

        theSession.Store(user);
        theSession.SaveChanges();

        var user2 = theSession.Load<User>(user.Id);
        user2.FirstName = "Jens";
        user2.LastName = "Pettersson";
        theSession.SaveChanges();

        using var session = theStore.OpenSession();
        var user3 = session.Load<User>(user.Id);
        user3.FirstName.ShouldBe("Jens");
        user3.LastName.ShouldBe("Pettersson");
    }

}