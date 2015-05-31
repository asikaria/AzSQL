# AzSQL
#### Query Azure Tables using SQL syntax

AzSQL supports a limited dialect of SQL, and translates user's input SQL queries into equivalent
Azure Tables query that can be run by Azure storage servers. The SQL dialect is limited by the
capabilities of Azure storage.

## SQL Dialect
```
select <* or comma-separated column names>
from <azure table name>
where <expression>     
```

Expression in WHERE clause can use:
-  comparison operators: '<' | '<=' | '>' | '>=' | '=' | '==' | '!=' | '<>'
-  logical operators to combine expressions: and or not
-  comparisons are of the form columnName op value, where columnName is a table column name, op is an operator, and value is a literal

## Using

### On Command Line

Run `AzSQL.exe` with two command-line parameters: storage account name, and storage account key.
This brings up the command prompt where you can type the SQL expression, which will be run 
against the table service and results printed on screen, followed by the command prompt again.
To exit, type `quit` or `exit`. 




### In program
Add reference to AzSQL to your project.

```
using AzSQL; 

...

IEnumerable<DynamicTableEntity> results = AzSQL.RunQuery(SQLQueryString, AzureStorageCreds);

```


## Todo:
Will do these, based on need or requests.

-  Support 'TOP n' in SQL, and translate that to 'take n' in Azure's URL parameter
-  support datetime literals
-  test all variations of numeric literals
-  implement client-side GROUP BY and HAVING (no worse than doin the query and doing aggregation in code)
  -  Likely have a different method/different grammar, to emphasize that this method is doing client-side processing
-  implement client-side joins
  -  this can really blow in the user's face, if the joined tables are large. Be careful about warning user and see if thee is a way to do this without bowing up client memory for bad queries
-  add `INTO <filename>` to grammar, to export the results of the query as csv file
  -  maybe even do `INTO <filename> AS <format>` to export in one of supported formats
- Package into NuGet
  

