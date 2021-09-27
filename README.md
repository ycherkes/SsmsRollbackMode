# SsmsRollbackMode

An SQL Server Management Studio 2019 (v18) extension. Wraps the query with a transaction. So all the query execution side effects, excepting an identity seed, will be rolled back.

How to install (2 ways):
 - Build the  solution with Visual Studio in Administrator mode
 - Unzip [release archive](https://github.com/ycherkes/SsmsRollbackMode/releases) to C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\

Usage: Main menu -> Tools -> Rollback Mode
