[![Build status](https://ci.appveyor.com/api/projects/status/egop18jxjsfiublg?svg=true)](https://ci.appveyor.com/project/dei79/azurestoragetable)

# AzureStorageTable
This projects implements an abstraction for Azure Storage Tables to use POCOs because deriving every entity 
from ITableEntity or TableEntity looks like a step backwards. The current implementation is intended to be an 
abstraction to store every existing entity into Azure Table Store.

There are two different principals implemented. The first allows to define an external mapping structure between 
the existing model and the required fields in Azure Table, e.g. Partition and RowKey. The second option is to 
decorate existing  models with attributes to map the properties to partition and rowkey.

## Installation

```
Install-Package CoreHelpers.WindowsAzure.Storage.Table
```

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

## Virtual Partition and Row-Keys
When implementing storage schemes in Azure Table sometimes the partition or the row key are combinations out for two or more properties. Because of that the Azure Storage Table components supports virtual partition and row key attributes as follows:

```csharp
[Storable()]
[VirtualPartitionKey("{{Value1}}-{{Value2}}")]
[VirtualRowKey("{{Value2}}-{{Value3}}")]
public class VirtualPartKeyDemoModel
{
  public string Value1 { get; set;  }
  public string Value2 { get; set;  }				
  public string Value3 { get; set;  }
}
 ```

## Virtual Array Attributes
When storing arrays in Azure Table store there are two options. The first option is to store it as a JSON payload and the second option is to expand the array with his items to separate properties, e.g.

```json
{ DataElements: [1,2,3,4] }
```

becomes 

| DE00 | DE01 | DE02 | DE03 |
| --- | --- | --- | --- |
| 1 | 2 | 3 | 4 |

in Azure Table Store with the following code: 

```csharp
[Storable(Tablename: "VArrayModels")]
public class VArrayModel
{
  [PartitionKey]
  [RowKey]
  public string UUID { get; set; }

  [VirtualList(PropertyFormat: "DE{{index}}", Digits: 2)]
  public List<int> DataElements { get; set; } = new List<int>();
}
