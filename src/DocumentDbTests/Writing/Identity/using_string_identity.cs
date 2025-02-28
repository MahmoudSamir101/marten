﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Shouldly;
using Xunit;

namespace DocumentDbTests.Writing.Identity;

public class using_string_identity : IntegrationContext
{
    [Fact]
    public void persist_and_load()
    {
        var account = new Account{Id = "email@server.com"};

        theSession.Store(account);
        theSession.SaveChanges();

        using (var session = theStore.OpenSession())
        {
            SpecificationExtensions.ShouldNotBeNull(session.Load<Account>("email@server.com"));

            SpecificationExtensions.ShouldBeNull(session.Load<Account>("nonexistent@server.com"));
        }

    }

    #region sample_persist_and_load_async
    [Fact]
    public async Task persist_and_load_async()
    {
        var account = new Account { Id = "email@server.com" };

        theSession.Store(account);
        await theSession.SaveChangesAsync();

        using (var session = theStore.OpenSession())
        {
            SpecificationExtensions.ShouldNotBeNull((await session.LoadAsync<Account>("email@server.com")));

            SpecificationExtensions.ShouldBeNull((await session.LoadAsync<Account>("nonexistent@server.com")));
        }
    }
    #endregion

    [Fact]
    public void throws_exception_if_trying_to_save_null_id()
    {
        var account = new Account {Id = null};

        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            theSession.Store(account);
        });


    }


    [Fact]
    public void throws_exception_if_trying_to_save_empty_id()
    {
        var account = new Account { Id = string.Empty };

        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            theSession.Store(account);
        });


    }

    [Fact]
    public void persist_and_delete()
    {
        var account = new Account { Id = "email@server.com" };

        theSession.Store(account);
        theSession.SaveChanges();

        using (var session = theStore.OpenSession())
        {
            session.Delete<Account>(account.Id);
            session.SaveChanges();
        }

        using (var session = theStore.OpenSession())
        {
            SpecificationExtensions.ShouldBeNull(session.Load<Account>(account.Id));
        }
    }

    [Fact]
    public void load_by_array_of_ids()
    {
        theSession.Store(new Account { Id = "A" });
        theSession.Store(new Account { Id = "B" });
        theSession.Store(new Account { Id = "C" });
        theSession.Store(new Account { Id = "D" });
        theSession.Store(new Account { Id = "E" });

        theSession.SaveChanges();

        using (var session = theStore.OpenSession())
        {
            session.LoadMany<Account>("A", "B", "E").Count().ShouldBe(3);
        }
    }

    public using_string_identity(DefaultStoreFixture fixture) : base(fixture)
    {
    }
}