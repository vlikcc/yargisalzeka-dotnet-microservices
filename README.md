# Yargısal Zeka .NET Mikroservis Projesi

Bu depo; kimlik doğrulama, abonelik yönetimi, yapay zeka destekli analiz, belge (döküman) üretimi ve API Gateway bileşenlerinden oluşan .NET 9 tabanlı bir mikroservis mimarisi örneğidir.

## Mimari Bileşenler
- IdentityService: Kullanıcı kayıt & doğrulama, JWT üretimi.
- SubscriptionService: Kullanıcı abonelik ve kredi (RemainingCredits) takibi (gRPC + EF Core).
- AIService: Abonelikten kalan krediye göre analiz işlemi (örnek uç nokta /api/ai). gRPC client ile SubscriptionService'e gider.
- DocumentService: (Genişletilebilir) Belge üretimi / yönetimi için gRPC servisi iskeleti.
- ApiGateway: Servislere tek giriş noktası (Ocelot konfigürasyonu eklenebilir).
- Protos: Ortak .proto tanımları (subscriptions.proto).
- Tests klasörü: Birim, entegrasyon ve (şimdilik devre dışı bırakılmış) Pact sözleşme testleri örnekleri.

## Teknolojiler
- .NET 9, C# 13
- ASP.NET Core Web API & gRPC
- Entity Framework Core (InMemory & PostgreSQL sağlayıcıları)
- JWT Authentication
- PactNet (Sözleşme testleri – yapılandırma bekliyor)
- xUnit, Moq

## Hızlı Başlangıç
1. Depoyu klonlayın.
2. Gerekli NuGet paketleri restore edilir: `dotnet restore`
3. Çözümü derleyin: `dotnet build`
4. Geliştirme sırasında tek tek servisleri çalıştırabilirsiniz:
   - Örnek: `dotnet run --project IdentityService/IdentityService.csproj`
5. (Opsiyonel) PostgreSQL kullanacaksanız IdentityService ve SubscriptionService appsettings.json içindeki bağlantı bilgisini güncelleyin, migration üretin.

## Migration (Örnek - IdentityService)
```
cd IdentityService
 dotnet ef migrations add InitialIdentitySchema -c IdentityDbContext
 dotnet ef database update -c IdentityDbContext
cd ..
```
SubscriptionService için benzer şekilde kendi DbContext ve modellerinizi ekledikten sonra migration oluşturabilirsiniz.

## Testler
Tüm testleri çalıştırmak:
```
dotnet test
```
Notlar:
- Pact test projeleri (Provider / Consumer) şu anda Skip attribute ile devre dışı. Gerçek bir Pact Broker veya pact dosyası yolu ayarlanana kadar bu şekilde bırakılabilir.
- Entegrasyon testleri InMemory veritabanı ve TestServer (WebApplicationFactory) kullanır.

## Abonelik Akışı (Örnek)
1. İstemci AIService /api/ai uç noktasına yetkili (JWT) istek gönderir.
2. AIService, SubscriptionService gRPC metodunu (`CheckSubscriptionStatus`) çağırır.
3. Kredi yoksa 403 döner, varsa analiz simülasyonu sonucu ve güncel kredi (örnek decrement simülasyonu) döndürülür.

## gRPC Tanımı
`Protos/Protos/subscriptions.proto` içinde:
```
service Subscription {
  rpc CheckSubscriptionStatus (CheckStatusRequest) returns (CheckStatusResponse);
}
```
Kod üretimi SubscriptionService (Server) ve client kullanan servislerde (AIService, test projeleri) csproj içindeki `<Protobuf ...>` öğeleri ile sağlanır.

## Ortam Değişkenleri / Yapılandırma
- IdentityService: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` zorunlu.
- SubscriptionService: `ConnectionStrings:DefaultConnection` (PostgreSQL) veya InMemory için konfigürasyon.
- AIService: `GrpcServices:SubscriptionUrl` (SubscriptionService adresi) – development için `http://localhost:<port>`.

## Geliştirme İpuçları
- Yeni bir gRPC metod eklerken önce `.proto` dosyasını Protos projesine ekleyin, ardından ilgili servis projelerine Client/Server olarak referanslayın.
- Ortak modeller HTTP üzerinden paylaşılmayacaksa (sadece gRPC) tekrar eden DTO sınıflarını minimal tutun.
- Test projelerinde namespace uyuşmazlıklarını azaltmak için ana projelerde kök namespace sabit bırakın.

## Pact Testleri (Gelecek Çalışma)
- Consumer tarafında etkileşim tanımları hazırlandıktan sonra pact dosyaları oluşturulur.
- Provider doğrulaması için Pact Broker veya local pact dosya dizini yapılandırılmalı.
- Şu anda ilgili testler geçici olarak basitleştirildi (derleme sorunsuz olsun diye).

## Yol Haritası (Öneri)
- Kredi tüketimi ve güncellemesi için SubscriptionService'e `ConsumeCredit` gRPC metodu ekleme.
- DocumentService için `GenerateDocument` gRPC sözleşmesi tanımlama.
- API Gateway (Ocelot) konfigürasyonunu tüm servisleri kapsayacak şekilde genişletme.
- Merkezi kimlik doğrulama / yetkilendirme politikaları.
- Docker & docker-compose orkestrasyonu (PostgreSQL + tüm servisler).
- CI (GitHub Actions) pipeline: build + test + (ileride) pact publish / verify adımları.

## Katkı
Pull request gönderirken: 
1. `dotnet build` ve `dotnet test` temiz geçmeli.
2. Yeni public API veya gRPC değişikliklerinde README güncellemesi ekleyin.

## Lisans
(İsteğe bağlı lisans bilgisini buraya ekleyin.)

---
Sorular için issue açabilirsiniz. İyi çalışmalar!
