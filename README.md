1.	"START_HERE.bat" opens the PowerShell launcher script at "tools\start-here.ps1".
2.	The launcher sets the project paths, app host, preferred port 5093, fallback port range, and .NET SDK package name.
3.	It shows the "MovieWatchlistTracker Setup" heading.
4.	It asks whether to create a desktop shortcut.
5.	If accepted, it creates "MovieWatchlistTracker.lnk" on the Desktop pointing to "START_HERE.bat".
6.	It checks whether the ".NET" command (dotnet) is available.
7.	It checks whether a ".NET 10.x SDK" is installed.
8.	If ".NET 10" is missing, it asks whether to install it with "winget".
9.	If accepted, it runs "winget install Microsoft.DotNet.SDK.10".
10.	It shows the Server heading.
11.	It asks whether to start the server now.
12.	If starting, it runs "dotnet clean" on the web project.
13.	It restores web project dependencies with "dotnet restore".
14.	It restores test project dependencies with "dotnet restore".
15.	It builds the web project with "dotnet build --no-restore".
16.	It finds an available local URL, preferring "http://127.0.0.1:5093" and falling back through port 5100.
17.	It sets "ASPNETCORE_ENVIRONMENT" to "Development".
18.	It starts the app with "dotnet run --no-build --project ... --urls ....".
19.	Once the server is running, the launcher opens the selected local URL in the host system's preferred web browser.
20.	The browser is used to access the “MovieWatchlistTracker” web application while the server continues running in the terminal.
21.	The terminal remains open because the running server process is attached to that terminal session.
22.	The normal stopping method is to return to the terminal running the app and press “Ctrl+C”.
23.	“Ctrl+C” sends the shutdown command to the running “ASP.NET Core” host.
24.	The server then stops listening on the local URL and the application becomes unavailable in the browser.
25.	If Windows shows "Terminate batch job (Y/N)?", the user should enter "Y" to finish stopping the launcher session.
26.	If the terminal window is closed instead, the attached server process should also stop.
27.	If the server does not stop normally, the fallback method is to end the related dotnet process through the terminal, PowerShell, or Task Manager.
28.	If setup cannot continue, or the user declines to start the server, the launcher prints manual commands instead.
