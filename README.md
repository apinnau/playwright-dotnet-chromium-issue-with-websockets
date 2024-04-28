# playwright-dotnet-chromium-issue-with-websockets
Demo application to reproduce issue with Chromium and websockets in Playwright for .NET
Issue reported on 04/26/2024 (26.04.2024): https://github.com/microsoft/playwright/issues/30568

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

## How to build and run (Linux / Bash)

```bash
# build first time
cd src
dotnet build

# install Playwright dependencies (Powershell needs to be installed)
cd src/dotnetDemo.E2ETests
pwsh bin/Debug/net8.0/playwright.ps1 install

# run Playwright E2E tests
cd src/dotnetDemo.E2ETests
dotnet test
```

# Error description

There is a different behavior between Chromium and Firefox browser in Playwright when a server-side rendered web-application (here Blazor server app) switches from HTTP to websocket.

Goal of the demo application is to render a website either in English or German depending on the Accept-Language header ('de' or 'en'). E2E tests with Playwright .NET are created to ensure the correct language.

## Environment

- .NET 8
- Blazor (server-side rendering)
- Playwright .NET 1.43.0
- xUnit 2.5.3
- Tested on:
  - Windows 11 (10.0.22631)
  - Ubuntu 22.04 (via WSL)
  - same behavior

## Expected behavior

- Given pre-condition: Browser for E2E tests (Chromium and Firefox) are localized via browser context **Locale** option

```C#
private BrowserNewContextOptions GetContextOptionsForGermanBrowser()
{
    return new BrowserNewContextOptions()
    {
        BaseURL = _baseURL,
        Locale = "de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6",
    };
}

private BrowserNewContextOptions GetContextOptionsForEnglishBrowser()
{
    return new BrowserNewContextOptions()
    {
        BaseURL = _baseURL,
        Locale = "en,de;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6",
    };
}
```

- Chromium and Firefox are respecting the localized context during whole test run
  - on first load of website via HTTP request
  - on later performed switch from HTTP to websocket (GET request with Upgrade: websocket header)
- Website is loaded with correct language via HTTP and websocket
- All 4 E2E test cases passes

## Actual behavior

- Firefox:
  - The two test cases with Firefox passes as expected (Accept-Language header is respected all the time)
- Chromium:
  - Chromium respects the localized context as the Accept-Language header is sent while website is loaded via HTTP
  - Chromium DOES NOT respect the localized context as the Accept-Language header is missing when Blazor application requests the switch to websocket (GET request with Upgrade: websocket header)
  - One test case with Chromium still passes (as English is requested and English is default language > so missing Accept-Language header doesn't influence the test result)
  - Second test case with Chromium fails where German is requested via Accept-Language header:
    - Website is first loaded in German via HTTP
    - After a short time website switches to websocket and is loaded again with English as default language

**Remarks**: 
- Using web-application with a real Chrome browser doesn't show this behavior. Here is the Accept-Language header also in place during the switch from HTTP to websocket. So seems to be specific to Chromium in Playwright.
- Behavior could be reproduced with Windows 11 and with Ubuntu 22.04 (WSL)

## Network details (tcpdumps)

### (1) Correct behavior with Firefox in Playwright

```
# Initial load of website via HTTP (including Accept-Language header)

GET / HTTP/1.1
Host: localhost:5000
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8
Accept-Language: de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6
Accept-Encoding: gzip, deflate, br
Connection: keep-alive
Upgrade-Insecure-Requests: 1
Sec-Fetch-Dest: document
Sec-Fetch-Mode: navigate
Sec-Fetch-Site: none
Sec-Fetch-User: ?1

HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Date: Fri, 26 Apr 2024 06:25:01 GMT
Server: Kestrel
Cache-Control: no-cache, no-store, max-age=0
Pragma: no-cache
path=/; samesite=strict; httponly
Transfer-Encoding: chunked
blazor-enhanced-nav: allow
X-Frame-Options: SAMEORIGIN
```

```
# Negotiation of transport type (including Accept-Language header)

POST /_blazor/negotiate?negotiateVersion=1 HTTP/1.1
Host: localhost:5000
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0
Accept: */*
Accept-Language: de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6
Accept-Encoding: gzip, deflate, br
Referer: http://localhost:5000/
X-Requested-With: XMLHttpRequest
X-SignalR-User-Agent: Microsoft SignalR/0.0 (0.0.0-DEV_BUILD; Unknown OS; Browser; Unknown Runtime Version)
Origin: http://localhost:5000
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: same-origin
Content-Length: 0

HTTP/1.1 200 OK
Content-Length: 316
Content-Type: application/json
Date: Fri, 26 Apr 2024 06:25:01 GMT
Server: Kestrel

{"negotiateVersion":1,"connectionId":"*********************","connectionToken":"":"*********************","","availableTransports":[{"transport":"WebSockets","transferFormats":["Text","Binary"]},{"transport":"ServerSentEvents","transferFormats":["Text"]},{"transport":"LongPolling","transferFormats":["Text","Binary"]}]}
```

```
# Protocol switch to websocket (including Accept-Language header)

GET /_blazor?id=************************ HTTP/1.1
Host: localhost:5000
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0
Accept: */*
Accept-Language: de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6
Accept-Encoding: gzip, deflate, br
Sec-WebSocket-Version: 13
Origin: http://localhost:5000
Sec-WebSocket-Extensions: permessage-deflate
Sec-WebSocket-Key: ************************
Connection: keep-alive, Upgrade
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: websocket
Sec-Fetch-Site: same-origin
Pragma: no-cache
Cache-Control: no-cache
Upgrade: websocket

HTTP/1.1 101 Switching Protocols
Connection: Upgrade
Date: Fri, 26 Apr 2024 06:25:01 GMT
Server: Kestrel
Upgrade: websocket
Sec-WebSocket-Accept: ************************
```

### (2) Wrong behavior with Chromium in Playwright

```
# Initial load of website via HTTP (including Accept-Language header)

GET / HTTP/1.1
Host: localhost:5000
Connection: keep-alive
sec-ch-ua: "Chromium";v="124", "HeadlessChrome";v="124", "Not-A.Brand";v="99"
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: "Windows"
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/124.0.6367.29 Safari/537.36
Accept-Language: en,de;q=0.9;q=0.9,en-GB;q=0.8;q=0.8,de-DE;q=0.7;q=0.7,en-US;q=0.6;q=0.6
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Sec-Fetch-Site: none
Sec-Fetch-Mode: navigate
Sec-Fetch-User: ?1
Sec-Fetch-Dest: document
Accept-Encoding: gzip, deflate, br

HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Date: Fri, 26 Apr 2024 06:24:58 GMT
Server: Kestrel
Cache-Control: no-cache, no-store, max-age=0
Pragma: no-cache
path=/; samesite=strict; httponly
Transfer-Encoding: chunked
blazor-enhanced-nav: allow
X-Frame-Options: SAMEORIGIN
```

```
# Negotiation of transport type (including Accept-Language header)

POST /_blazor/negotiate?negotiateVersion=1 HTTP/1.1
Host: localhost:5000
Connection: keep-alive
Content-Length: 0
Cache-Control: max-age=0
sec-ch-ua: "Chromium";v="124", "HeadlessChrome";v="124", "Not-A.Brand";v="99"
X-Requested-With: XMLHttpRequest
Accept-Language: en,de;q=0.9;q=0.9,en-GB;q=0.8;q=0.8,de-DE;q=0.7;q=0.7,en-US;q=0.6;q=0.6
sec-ch-ua-mobile: ?0
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/124.0.6367.29 Safari/537.36
X-SignalR-User-Agent: Microsoft SignalR/0.0 (0.0.0-DEV_BUILD; Unknown OS; Browser; Unknown Runtime Version)
sec-ch-ua-platform: "Windows"
Accept: */*
Origin: http://localhost:5000
Sec-Fetch-Site: same-origin
Sec-Fetch-Mode: cors
Sec-Fetch-Dest: empty
Referer: http://localhost:5000/
Accept-Encoding: gzip, deflate, br

HTTP/1.1 200 OK
Content-Length: 316
Content-Type: application/json
Date: Fri, 26 Apr 2024 06:24:58 GMT
Server: Kestrel

{"negotiateVersion":1,"connectionId":"************************","connectionToken":"************************","availableTransports":[{"transport":"WebSockets","transferFormats":["Text","Binary"]},{"transport":"ServerSentEvents","transferFormats":["Text"]},{"transport":"LongPolling","transferFormats":["Text","Binary"]}]}
```

```
# Protocol switch to websocket (Missing Accept-Language header)

GET /_blazor?id=************************ HTTP/1.1
Host: localhost:5000
Connection: Upgrade
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/124.0.6367.29 Safari/537.36
Upgrade: websocket
Origin: http://localhost:5000
Sec-WebSocket-Version: 13
Accept-Encoding: gzip, deflate, br
Sec-WebSocket-Key: ************************
Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits

HTTP/1.1 101 Switching Protocols
Connection: Upgrade
Date: Fri, 26 Apr 2024 06:24:58 GMT
Server: Kestrel
Upgrade: websocket
Sec-WebSocket-Accept: ************************
```

### Correct behavior with regular Chrome (no call via Playwright)

```
# Protocol switch to websocket (including Accept-Language header)

GET /_blazor?id=************************ HTTP/1.1
Host: localhost:5000
Connection: Upgrade
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36
Upgrade: websocket
Origin: http://localhost:5000
Sec-WebSocket-Version: 13
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: de,en;q=0.9,en-GB;q=0.8,de-DE;q=0.7,en-US;q=0.6
Sec-WebSocket-Key: ************************
Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits

HTTP/1.1 101 Switching Protocols
Connection: Upgrade
Date: Fri, 26 Apr 2024 15:14:59 GMT
Server: Kestrel
Upgrade: websocket
Sec-WebSocket-Accept: ************************
```
