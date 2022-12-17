# SsmsRollbackMode

An SQL Server Management Studio 2019 (v18) extension. Wraps the query with a transaction. So all the query execution side effects, excepting an identity seed, will be rolled back.

As I found out later, this is the equivalent of the "**Test mode**" of **ApexSQL Complete** extension - see an [article](https://blog.apexsql.com/apexsql-complete-2014-sneak-peek-at-the-new-features/#:~:text=The%20Test%20mode%20feature%20allows%20executing%20query%20without%20consequence%20and%20impact%20to%20a%20database.%20Before%20executing%20a%20query%20in%20the%20query%20window%20choose%20the%20Test%20mode%20option%20from%20the%20SSMS%20Toolbar%3A).

How to install (2 ways):
 - Build the  solution with Visual Studio in Administrator mode
 - Unzip [release archive](https://github.com/ycherkes/SsmsRollbackMode/releases) to C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\

Usage: Main menu -> Tools -> Rollback Mode
