# Katkıda Bulunma Rehberi

CKC projesine katkıda bulunmayı düşündüğünüz için teşekkürler! Aşağıdaki yönergeleri takip ederek katkı sürecini kolaylaştırabilirsiniz.

## Kod Stili

- **Dil**: Tüm kodlar İngilizce yazılmalıdır (kullanıcıya gösterilen mesajlar Türkçe kalabilir)
- **İsimlendirme**: PascalCase (sınıflar, metotlar, property'ler), camelCase (yerel değişkenler, parametreler)
- **Boşluk kullanımı**: 4 boşluk ile girintileme
- **`this` anahtar kelimesi**: Kullanılmamalıdır
- **Yorumlar**: Zorunlu olmadıkça yorum eklenmemelidir, kod kendini açıklamalıdır
- **Dosya sonu**: Her dosya tek bir boş satır ile bitmelidir

## Proje Mimarisi

```
Commands/   → Komut işleyicileri (statik sınıflar, string[] alır, string döndürür)
Core/       → Çekirdek altyapı (dispatcher, pipeline, formatter)
Services/   → İş mantığı (native API çağrılarını soyutlar)
Native/     → P/Invoke tanımları (DLLImport, struct'lar, enum'lar)
Models/     → Veri modelleri (POCO sınıflar)
```

## Pull Request Süreci

1. Fork'layın ve branch oluşturun (`git checkout -b feature/amazing-feature`)
2. Değişikliklerinizi yapın
3. `dotnet build` ile derleme hatası olmadığını doğrulayın
4. Commit mesajınızı açıklayıcı yazın
5. Branch'inizi pushlayın (`git push origin feature/amazing-feature`)
6. Pull Request açın

## Yeni Bir Komut Eklemek

1. `Commands/` altında yeni bir `.cs` dosyası oluşturun (veya var olana ekleyin)
2. Statik bir sınıf ve `static string ExecuteX(string[] args)` metodu tanımlayın
3. `Program.cs` içinde `SetupCommands()` metoduna komutu kaydedin
4. Gerekirse `Services/` katmanına iş mantığını ekleyin
5. Gerekirse `Native/` katmanına P/Invoke tanımlarını ekleyin
6. Gerekirse `Models/` katmanına veri modelini ekleyin
