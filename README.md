# JWT Auth API

ASP.NET Core 8, Identity, EF Core, PostgreSQL, JWT access tokens ve veritabanında saklanan refresh token’lar. Roller: `Individual`, `Corporate`, `Admin`.

## Gereksinimler

- .NET 8 SDK
- PostgreSQL (ör. Docker)

## Proje konumu

Kaynak kod ve çözüm dosyası **`JWT-auth`** klasöründe (`JWT-auth/JWT-auth.sln`). Aşağıdaki terminal komutlarında önce bu klasöre gir: `cd JWT-auth`.

## Yapılandırma

`JWT-auth/appsettings.json` içinde `ConnectionStrings:DefaultConnection` değerini kendi veritabanına göre düzenle.

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=***"
}
```

`Jwt` bölümünde imzalama anahtarını ve süreleri üretimde güvenli değerlerle değiştir.

**Rate limiting (sadece login):** `RateLimiting:LoginPermitLimit` ve `LoginWindowMinutes` — aynı IP için pencere başına istek sınırı.

## Veritabanı

`JWT-auth` klasöründeyken (gerekirse `dotnet tool install --global dotnet-ef`):

```bash
dotnet ef database update
```

## Çalıştırma

`JWT-auth` klasöründeyken:

```bash
dotnet run
```

Geliştirme ortamında Swagger: `http://localhost:<port>/swagger` (port `JWT-auth/Properties/launchSettings.json` veya konsol çıktısından). Swagger’da **Authorize** → **Value** alanına login yanıtındaki **`accessToken`** metnini yapıştır (`Bearer ` yazma; Swagger ekler).

## Özellikler

| Özellik | Açıklama |
|--------|----------|
| API | `POST /api/Auth/register`, `login`, `refresh`, `logout` |
| Rol koruması | `GET /api/RoleProtected/individual`, `corporate`, `admin` |
| Refresh token | `RefreshTokens` tablosu; iptal (`RevokedAtUtc`) |
| Access token blacklist | Logout sonrası JWT `jti` `RevokedAccessTokens` tablosunda |
| Audit | `AuditLogs`: kullanıcı (varsa), endpoint yolu, HTTP metodu, zaman (UTC) |
| Rate limit | `POST /api/Auth/login` için IP bazlı sabit pencere |

## API testleri (Swagger ve Postman)

**Swagger UI** — Uygulama `Development` ortamında çalışırken tarayıcıda `/swagger` açılır. Örnek adresler: `http://localhost:5218/swagger` (http profili), `https://localhost:7181/swagger` (https profili). Gerçek port ve şema için konsol veya IDE çıktısındaki `Now listening on` satırına bakın. Korumalı isteklerde **Authorize** ile access token kullanımı yukarıdaki **Çalıştırma** bölümünde anlatılır.

**Postman** — Collection dosyası: `JWT-auth/postman/JWT-auth.postman_collection.json`. Postman’de **Import** ile eklenir; collection değişkeni **`baseUrl`** çalışan API kök adresine (ör. `http://localhost:5218`) ayarlanır.
