# CKC - Console Kernel Control v3.0

**Kernel Seviyesinde Windows Sistem Yönetim ve Diagnostik Aracı**

CKC, Windows işletim sistemleri için geliştirilmiş, kernel seviyesinde bilgi almanıza ve sistem yönetimi yapmanıza olanak tanıyan güçlü bir komut satırı aracıdır. Doğrudan Windows Native API'leri (P/Invoke) kullanarak çalışır ve proses yönetiminden bellek analizine, ağ bağlantılarından servis yönetimine kadar geniş bir yelpazede işlev sunar.

---

## Özellikler

### 🖥️ Sistem Bilgileri
- İşletim sistemi sürümü, yapı numarası, edition bilgisi
- CPU mimarisi, seviyesi, revizyonu, sayfa boyutu
- Sistem çalışma süresi (uptime) ve önyükleme zamanı
- BIOS/UEFI bilgileri
- Bilgisayar adı, kullanıcı adı

### ⚙️ Proses Yönetimi
- Çalışan prosesleri listeleme (PID, isim, CPU, bellek kullanımı)
- Proses ağacı görüntüleme (`ps tree`)
- Proses sonlandırma (`kill`), dondurma (`suspend`), devam ettirme (`resume`)
- Detaylı proses bilgisi (bellek, thread, handle, CPU zamanı)
- Thread, modül ve handle listeleme
- Sıralama desteği (`sort cpu`, `sort mem`, `sort name`)

### 🧠 Bellek Yönetimi
- Fiziksel ve sanal bellek durumu
- Kernel pool (paged/non-paged) bilgisi
- Commit limit, commit peak değerleri
- Sanal bellek haritası (VMMap) - bölge bazında adres, boyut, durum, koruma
- Performans bilgileri (handle, proses, thread sayıları)

### 🌐 Ağ Bağlantıları
- TCP ve UDP bağlantılarını listeleme (yerel/uzak adres, port, durum, PID)
- Protokole göre filtreleme (`-p tcp`, `-p udp`)
- PID'e göre bağlantı sorgulama
- Bağlantı durumları (LISTEN, ESTABLISHED, TIME_WAIT vb.)

### 📁 Dosya Sistemi
- Hex dump görüntüleme (offset ve uzunluk desteği)
- Detaylı dosya bilgisi (boyut, tarihler, sahiplik, öznitelikler)
- Disk/drive bilgisi (toplam, kullanılan, boş alan, dosya sistemi)
- Dosya arama (pattern ve dizin desteği)

### 🔧 Servis Yönetimi
- Tüm Windows servislerini listeleme (çalışan/durmuş)
- Servis detay bilgisi (bağımlılıklar, binary yolu, hesap)
- Servis başlatma, durdurma, yeniden başlatma
- Filtreleme (`service running`, `service stopped`)

### 📝 Registry
- Registry anahtarı sorgulama ve değerleri görüntüleme
- Alt anahtar listeleme
- Tüm kök anahtarları destekler (HKLM, HKCU, HKCR, HKU, HKCC)

### 🧩 Kernel
- Yüklü kernel modüllerini/driverlarını listeleme
- Driver detay bilgisi (base address, size, load order)
- NtQuerySystemInformation ile doğrudan kernel sorgulama

### 🪟 Pencere Yönetimi
- Tüm üst düzey pencereleri listeleme
- Pencere detay bilgisi (konum, boyut, görünürlük, show state)
- Aktif ön plan penceresini görüntüleme

### 🔒 Güvenlik
- Mevcut kullanıcı bilgisi (WhoAmI)
- Proses ayrıcalıklarını (privileges) listeleme ve sorgulama
- Elevation / yönetici yetkisi kontrolü
- UAC elevation talep etme
- Token bilgisi (elevation type, integrity level)

### 📜 Betik & Yardımcılar
- Komut geçmişi (`history`, `history clear/save/load`)
- Alias yönetimi (`alias set`, `alias del`)
- Pipeline desteği (`komut1 | komut2`)
- Yerel shell komutu çalıştırma (`shell <komut>`)
- Komut kategorizasyonlu yardım sistemi

---

## Komut Listesi

### SİSTEM
| Komut | Açıklama |
|-------|----------|
| `sysinfo` | Sistem bilgilerini gösterir (OS, CPU, bellek, uptime) |
| `cpuinfo` | CPU mimari ve özelliklerini gösterir |
| `uptime` | Sistem çalışma süresini gösterir |
| `biosinfo` | BIOS/UEFI bilgilerini gösterir |
| `osver` | İşletim sistemi sürümünü gösterir |
| `elevated` | Yönetici yetkisi kontrolü yapar |
| `whoami` | Mevcut kullanıcı bilgilerini gösterir |
| `privileges` | Mevcut proses yetkilerini listeler |
| `elevate` | Proses'i yönetici olarak yeniden başlatmayı dener |

### PROCESS
| Komut | Açıklama |
|-------|----------|
| `ps` | Çalışan prosesleri listeler (`ps tree`, `ps <pid>`) |
| `kill <pid>` | Proses sonlandırır |
| `suspend <pid>` | Proses'i dondurur |
| `resume <pid>` | Dondurulmuş proses'i devam ettirir |
| `procinfo <pid>` | Proses detaylı bilgi |
| `threads <pid>` | Proses thread'lerini listeler |
| `modules <pid>` | Proses modüllerini listeler |
| `handles <pid>` | Proses handle'larını gösterir |

### BELLEK
| Komut | Açıklama |
|-------|----------|
| `sysmem` | Fiziksel ve sanal bellek durumu |
| `vmmap` | Sanal bellek haritasını gösterir |
| `pool` | Kernel pool bellek bilgisi |

### AĞ
| Komut | Açıklama |
|-------|----------|
| `netstat` | Aktif ağ bağlantılarını listeler |
| `connections <pid>` | PID'e göre bağlantıları gösterir |

### DOSYA
| Komut | Açıklama |
|-------|----------|
| `hexdump <dosya>` | Dosya hex dump gösterimi |
| `fileinfo <dosya>` | Dosya detaylı bilgi |
| `diskinfo` | Disk/Drive bilgisi |
| `search <pattern>` | Dosya ara |

### SERVİS
| Komut | Açıklama |
|-------|----------|
| `service` | Servisleri listeler (`service running/stopped`) |
| `serviceinfo <ad>` | Servis detay |
| `servicestart <ad>` | Servis başlatır |
| `servicestop <ad>` | Servis durdurur |
| `servicerestart <ad>` | Servisi yeniden başlatır |

### REGISTRY
| Komut | Açıklama |
|-------|----------|
| `regquery <path>` | Registry anahtarı sorgular |
| `regenum <path>` | Registry alt anahtarlarını listeler |

### KERNEL
| Komut | Açıklama |
|-------|----------|
| `kernelmodules` | Yüklü kernel modüllerini/driverlarını listeler |
| `driverinfo` | Driver/modül bilgisi |

### PENCERE
| Komut | Açıklama |
|-------|----------|
| `windows` | Pencere listesini gösterir |
| `wininfo <handle>` | Pencere detay |
| `foreground` | Aktif ön plandaki pencereyi gösterir |

### BETİK
| Komut | Açıklama |
|-------|----------|
| `history` | Komut geçmişini gösterir/yönetir |
| `alias` | Alias yönetimi |
| `echo <metin>` | Metni ekrana yazdırır |

### DİĞER
| Komut | Açıklama |
|-------|----------|
| `shell <komut>` | Yerel shell komutu çalıştırır |
| `clear` / `cls` | Ekranı temizler |
| `exit` / `quit` | Terminali kapatır |
| `help` | Yardım |

---

## Kurulum

### Gereksinimler
- Windows 7 / 8 / 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) veya üzeri
- Bazı komutlar için yönetici yetkileri gerekebilir

### Derleme
```bash
git clone https://github.com/CoYgon/CKC.git
cd CKC
dotnet build -c Release
```

### Çalıştırma
```bash
# REPL (etkileşimli) mod:
dotnet run

# Tek seferlik komut:
dotnet run -- sysinfo
dotnet run -- ps
dotnet run -- netstat
```

### Yayınlama (Standalone Binary)
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Binary dosyaları `bin\Release\net8.0\win-x64\publish\` dizininde oluşacaktır.

---

## Kullanım Örnekleri

```bash
# Sistem bilgilerini görüntüle
ckc@kernel:~$ sysinfo

# Çalışan prosesleri belleğe göre sırala
ckc@kernel:~$ ps sort mem

# Proses ağacını göster
ckc@kernel:~$ ps tree

# Bir prosesi sonlandır
ckc@kernel:~$ kill 1234

# TCP bağlantılarını listele
ckc@kernel:~$ netstat -p tcp

# Bir dosyayı hex görüntüle
ckc@kernel:~$ hexdump C:\Windows\System32\drivers\etc\hosts

# Pipeline kullanımı
ckc@kernel:~$ ps | echo

# Registry sorgula
ckc@kernel:~$ regquery HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion

# Alias oluştur
ckc@kernel:~$ alias set np netstat -p tcp
ckc@kernel:~$ np

# Kernel modüllerini listele
ckc@kernel:~$ kernelmodules
```

---

## Proje Yapısı

```
CKC/
├── CKC.csproj              # Proje dosyası (.NET 8)
├── Program.cs              # Ana giriş noktası, REPL döngüsü, komut kaydı
├── Commands/               # Komut yürütme sınıfları
│   ├── SystemCommands.cs   # Sistem bilgisi komutları
│   ├── ProcessCommands.cs  # Proses yönetimi komutları
│   ├── MemoryCommands.cs   # Bellek komutları
│   ├── NetworkCommands.cs  # Ağ bağlantısı komutları
│   ├── FileCommands.cs     # Dosya sistemi komutları
│   ├── ServiceCommands.cs  # Servis yönetimi komutları
│   ├── RegistryCommands.cs # Registry komutları
│   ├── KernelCommands.cs   # Kernel modül komutları
│   ├── WindowCommands.cs   # Pencere yönetimi komutları
│   ├── SecurityCommands.cs # Güvenlik komutları
│   └── ScriptingCommands.cs# Betik/yardımcı komutlar
├── Core/                   # Çekirdek altyapı
│   ├── CommandDispatcher.cs# Komut dağıtıcı ve ayrıştırıcı
│   ├── CommandHistory.cs   # Komut geçmişi yönetimi
│   ├── CommandPipeline.cs  # Pipeline işleme
│   ├── ConsoleFormatter.cs # Konsol çıktı biçimlendirme
│   └── ElevationHelper.cs  # Yönetici yetkisi yardımcısı
├── Models/                 # Veri modelleri
│   ├── SystemInfo.cs       # Sistem bilgisi modeli
│   ├── ProcessInfo.cs      # Proses bilgisi modeli
│   ├── MemoryStatus.cs     # Bellek durumu modeli
│   ├── NetworkConnection.cs# Ağ bağlantısı modeli
│   ├── ServiceInfo.cs      # Servis bilgisi modeli
│   └── KernelModule.cs     # Kernel modülü, Thread, Handle, Window modelleri
├── Services/               # İş mantığı katmanı
│   ├── KernelManager.cs    # Kernel/NT sorgulama yöneticisi
│   ├── ProcessManager.cs   # Proses yönetimi
│   ├── MemoryManager.cs    # Bellek yönetimi
│   ├── NetworkManager.cs   # Ağ yönetimi
│   ├── FileSystemManager.cs# Dosya sistemi yönetimi
│   ├── RegistryManager.cs  # Registry yönetimi
│   └── ServiceManager.cs   # Servis yönetimi
└── Native/                 # P/Invoke native API tanımları
    ├── Kernel32.cs         # kernel32.dll API'leri
    ├── AdvApi32.cs         # advapi32.dll API'leri
    ├── NtDll.cs            # ntdll.dll API'leri (NT native)
    ├── PsApi.cs            # psapi.dll API'leri
    ├── IpHlpApi.cs         # iphlpapi.dll API'leri
    └── User32.cs           # user32.dll API'leri
```

---

## Teknik Detaylar

### Kullanılan Native API'ler

| DLL | Kullanım Amacı |
|-----|----------------|
| **kernel32.dll** | Sistem bilgisi, proses/thread yönetimi, bellek sorgulama, toolhelp API |
| **advapi32.dll** | Token yönetimi, ayrıcalık sorgulama, servis yönetimi (SCM) |
| **ntdll.dll** | NT native API: NtQuerySystemInformation, NtQueryInformationProcess, RtlGetVersion |
| **psapi.dll** | Proses enumeration, performans bilgisi, proses bellek bilgisi |
| **iphlpapi.dll** | TCP/UDP bağlantı tabloları (GetExtendedTcpTable, GetExtendedUdpTable) |
| **user32.dll** | Pencere enumeration, window bilgisi, foreground window |

### Güvenlik Notları
- Bazı komutlar (kernelmodules, servis yönetimi, proses sonlandırma) **yönetici yetkisi** gerektirir
- `elevate` komutu ile UAC üzerinden yeniden başlatma talep edilebilir
- Token sorgulama ve ayrıcalık yönetimi için AdvApi32 kullanılır
- Handle ve token işlemlerinde bellek sızıntısını önlemek için `try/finally` ile kaynak yönetimi yapılmıştır

---

## Geliştirici

- **CoYgon** - Proje sahibi ve geliştirici

---

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.
