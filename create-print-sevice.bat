REM Define the directory name
set dir_name=release

REM Delete the directory and the service if they exist
if exist C:\%dir_name% (
    sc.exe STOP PrintIt
    timeout /t 2 /nobreak
    sc.exe delete "PrintIt"
    timeout /t 2 /nobreak
    rmdir /s /q C:\%dir_name%
)

REM Create the directory
mkdir %dir_name%

REM Run the dotnet publish command
dotnet publish --configuration Release --output ./%dir_name%

REM Move the directory to C:\
move /y %dir_name% C:\

REM Create and start the service
sc.exe create PrintIt binPath= "C:\release\PrintIt.ServiceHost.exe" start=auto
timeout /t 2 /nobreak
sc.exe start "PrintIt"