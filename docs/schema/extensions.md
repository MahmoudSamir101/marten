# Schema Feature Extensions

New in Marten 5.4.0 is the ability to add additional features with custom database schema objects that simply plug into Marten's [schema management facilities)[/schema/migrations). The key abstraction is the `IFeatureSchema` interface from the [Weasel.Core](https://www.nuget.org/packages/Weasel.Core/) library.

Not to worry though, Marten comes with a base class that makes it a bit simpler to build out new features. Here's a very simple example that defines a custom table with one column:

<!-- snippet: sample_creating-a-fake-schema-feature -->
<a id='snippet-sample_creating-a-fake-schema-feature'></a>
```cs
public class FakeStorage : FeatureSchemaBase
{
    private readonly StoreOptions _options;

    public FakeStorage(StoreOptions options) : base("fake", options.Advanced.Migrator)
    {
        _options = options;
    }

    protected override IEnumerable<ISchemaObject> schemaObjects()
    {
        var table = new Table(new DbObjectName(_options.DatabaseSchemaName, "mt_fake_table"));
        table.AddColumn("name", "varchar");

        yield return table;
    }
}
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/DocumentDbTests/Configuration/ability_to_add_custom_storage_features.cs#L48-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_creating-a-fake-schema-feature' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, to actually apply this feature to your Marten applications, use this syntax:

<!-- snippet: sample_adding-schema-feature -->
<a id='snippet-sample_adding-schema-feature'></a>
```cs
var store = DocumentStore.For(_ =>
{
    // Creates a new instance of FakeStorage and
    // passes along the current StoreOptions
    _.Storage.Add<FakeStorage>();

    // or

    _.Storage.Add(new FakeStorage(_));
});
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/DocumentDbTests/Configuration/ability_to_add_custom_storage_features.cs#L33-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_adding-schema-feature' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that when you use the `Add<T>()` syntax, Marten will pass along the current `StoreOptions` to the constructor function if there is a constructor with that signature. Otherwise, it uses the no-arg constructor.

While you *can* directly implement the `ISchemaObject` interface for something Marten doesn't already support. Marten provides an even easier extensibility mechanism to add custom database objects such as Postgres tables, functions and sequences using `StorageFeatures.ExtendedSchemaObjects` using [Weasel](https://github.com/JasperFx/weasel).

## Table

Postgresql tables can be modeled with the `Table` class from `Weasel.Postgresql.Tables` as shown in this example below:

<!-- snippet: sample_CustomSchemaTable -->
<a id='snippet-sample_customschematable'></a>
```cs
StoreOptions(opts =>
{
    opts.RegisterDocumentType<Target>();

    var table = new Table("adding_custom_schema_objects.names");
    table.AddColumn<string>("name").AsPrimaryKey();

    opts.Storage.ExtendedSchemaObjects.Add(table);
});

await theStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/CoreTests/adding_custom_schema_objects.cs#L46-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customschematable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Function

Postgresql functions can be managed by creating a function using `Weasel.Postgresql.Functions.Function` as below:

<!-- snippet: sample_CustomSchemaFunction -->
<a id='snippet-sample_customschemafunction'></a>
```cs
StoreOptions(opts =>
{
    opts.RegisterDocumentType<Target>();

    // Create a user defined function to act as a ternary operator similar to SQL Server
    var function = new Function(new DbObjectName("public", "iif"), @"
create or replace function iif(
condition boolean,       -- if condition
true_result anyelement,  -- then
false_result anyelement  -- else
) returns anyelement as $f$
select case when condition then true_result else false_result end
$f$  language sql immutable;
");

    opts.Storage.ExtendedSchemaObjects.Add(function);
});

await theStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/CoreTests/adding_custom_schema_objects.cs#L96-L116' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customschemafunction' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Sequence

[Postgresql sequences](https://www.postgresql.org/docs/10/static/sql-createsequence.html) can be created using `Weasel.Postgresql.Sequence` as below:

<!-- snippet: sample_CustomSchemaSequence -->
<a id='snippet-sample_customschemasequence'></a>
```cs
StoreOptions(opts =>
{
    opts.RegisterDocumentType<Target>();

    // Create a sequence to generate unique ids for documents
    var sequence = new Sequence("banana_seq");

    opts.Storage.ExtendedSchemaObjects.Add(sequence);
});

await theStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/CoreTests/adding_custom_schema_objects.cs#L130-L142' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customschemasequence' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Extension

Postgresql extensions can be enabled using `Weasel.Postgresql.Extension` as below:

<!-- snippet: sample_CustomSchemaExtension -->
<a id='snippet-sample_customschemaextension'></a>
```cs
StoreOptions(opts =>
{
    opts.RegisterDocumentType<Target>();

    // Unaccent is an extension ships with postgresql
    // and removes accents (diacritic signs) from strings
    var extension = new Extension("unaccent");

    opts.Storage.ExtendedSchemaObjects.Add(extension);
});

await theStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
```
<sup><a href='https://github.com/JasperFx/marten/blob/master/src/CoreTests/adding_custom_schema_objects.cs#L71-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_customschemaextension' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
