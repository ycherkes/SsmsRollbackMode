# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

# SsmsRollbackMode

An SQL Server Management Studio 2019 (v18) extension. Wraps the query with a transaction. So all the query execution side effects, excepting an identity seed, will be rolled back.

As I found out later, this is the equivalent of the "**Test mode**" of **ApexSQL Complete** extension - see an [article](https://solutioncenter.apexsql.com/how-to-prevent-accidental-data-loss-from-executing-a-query-in-sql-server-aka-practicing-safe-coding/#:~:text=Test%20mode,to%20a%20database.).

How to install (2 ways):
 - Build the  solution with Visual Studio in Administrator mode
 - Unzip [release archive](https://github.com/ycherkes/SsmsRollbackMode/releases) to C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\

Usage: Main menu -> Tools -> Rollback Mode

![SsmsRollbackModeDemo](https://user-images.githubusercontent.com/13467759/208288303-78cf4aca-4a16-4ca2-a1c6-bcce17fe47b3.gif)

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+SSMS+Rollback+Mode+extension+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.
