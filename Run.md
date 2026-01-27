# Running Backend:

cd OnePro.API
dotnet run --urls "https://localhost:58644"

# Running Frontend

cd OnePro.Front
dotnet run --urls "https://localhost:7095"


cd OnePro.Front
dotnet watch --urls "https://localhost:7095"


# Migrations dan Update Database

cd Core
dotnet ef migrations add addRicRollOut --startup-project ../OnePro.API
dotnet ef database update --startup-project ../OnePro.API

# Delete Database

sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases"

sqlcmd -S (localdb)\MSSQLLocalDB -Q "DROP DATABASE DatabaseRic"


# Open Sql Server in CMD


sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "SELECT TOP 10 AsIsProcessRasciFile FROM FormRics"

sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "SELECT * FROM Groups"

sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "SELECT FormRicRollOutId FROM FormRicRollOutHistories"



sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "SELECT TOP 2 Hashtag FROM FormRicRollOuts"
 
sqlcmd -S (localdb)\MSSQLLocalDB -d DatabaseRic -Q "DELETE FROM FormRicRollOuts"


SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FormRicHistories';
GO

sqlcmd -S "(localdb)\MSSQLLocalDB" -d "DatabaseRic" -Q "UPDATE FormRics SET Status = Role.Review_BR WHERE Id = 'e0fd9684-fa37-4baa-b43c-0da3c1a4ea7c'"