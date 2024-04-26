# playwright-dotnet-chromium-issue-with-websockets
Demo application to reproduce issue with Chromium and websockets in Playwright for .NET

## How to build and run (Windows / Powershell)

```powershell
# build first time
cd src
dotnet build

# install Playwright dependencies
cd src/dotnetDemo.E2ETests
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
bin/Debug/net8.0/playwright.ps1 install

# run Playwright E2E tests
cd src/dotnetDemo.E2ETests
dotnet test
```
