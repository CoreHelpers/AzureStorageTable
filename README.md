# AzureStorageTable
This projects implements an abstraction for Azure Storage Tables to use POCOs because deriving every entity 
from ITableEntity or TableEntity looks like a step backwards. The current implementation is intended to be an 
abstraction to store every existing entity into Azure Table Store.

There are two different principals implemented. The first allows to define an external mapping structure between 
the existing model and the required fields in Azure Table, e.g. Partition and RowKey. The second option is to 
decorate existing  models with attributes to map the properties to partition and rowkey.

## Manual Entity Mapper

```csharp
// create a new user model
var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };

using (var storageContext = new StorageContext(storageKey, storageSecret))
{
  // configure the entity mapper
  storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = "UserProfiles", PartitionKeyPropery = "Contact", RowKeyProperty = "Contact" });

  // ensure the table exists
  storageContext.CreateTable<UserModel>();

  // inser the model
  storageContext.MergeOrInsert<UserModel>(user);

  // query all
  var result = storageContext.Query<UserModel>();

  foreach (var r in result)
  {
    Console.WriteLine(r.FirstName);
  }
}
```

## Attribute Based Entity Mapper

Decorate your existing model 
```csharp
[Storable()]
public class UserModel2
{                       
  [PartitionKey]
  [RowKey]
  public string Contact { get; set; }

  public string FirstName { get; set; } 
  public string LastName { get; set; }                		
}
```

Configure and use the Storage Context
```csharp
// create a new user model
var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };            

using (var storageContext = new StorageContext(storageKey, storageSecret))
{
  // ensure we are using the attributes
  storageContext.AddAttributeMapper();

  // ensure the table exists
  storageContext.CreateTable<UserModel2>();

  // inser the model
  storageContext.MergeOrInsert<UserModel2>(user);

  // query all
  var result = storageContext.Query<UserModel2>();

  foreach (var r in result)
  {
      Console.WriteLine(r.FirstName);
  }
}
```
