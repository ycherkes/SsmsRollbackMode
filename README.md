# SsmsRollbackMode

An SQL Server Management Studio 2019 (v18) extension. Wraps the query with a transaction. So all the query execution side effects, excepting an identity seed, will be rolled back.

As I found out later, this is the equivalent of the "**Test mode**" of **ApexSQL Complete** extension - see an [article](https://solutioncenter.apexsql.com/how-to-prevent-accidental-data-loss-from-executing-a-query-in-sql-server-aka-practicing-safe-coding/#:~:text=Test%20mode,to%20a%20database.).

How to install (2 ways):
 - Build the  solution with Visual Studio in Administrator mode
 - Unzip [release archive](https://github.com/ycherkes/SsmsRollbackMode/releases) to C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\

Usage: Main menu -> Tools -> Rollback Mode
