# Tubes2_toBeAnnounced

Aplikasi berbasis web untuk memvisualisasikan struktur DOM dari sebuah URL atau potongan kode HTML, serta melakukan penelusuran elemen berdasarkan CSS selektor menggunakan algoritma BFS dan DFS.

## Aplikasi BFS dan DFS
1. Breadth-First Search (BFS)
Menelusuri pohon DOM secara lapis demi lapis menggunakan Queue. BFS sangat optimal untuk menemukan elemen yang posisinya dangkal atau dekat dengan root.

2. Depth-First Search (DFS)
Menelusuri pohon DOM hingga ke elemen paling ujung sebelum backtrack. Dengan menggunakan Stack atau rekursif, DFS sangat efektif untuk mencari elemen yang bersarang sangat dalam.

## Tech Stack
- **Backend:** C# (.NET)
- **Frontend:** TypeScript (Bun + Vite)

## Requirements

### Opsi 1: Menjalankan dengan Docker

1. Install **Docker Desktop**
   - Download dari [docker.com](https://www.docker.com/products/docker-desktop/)
2. Pastikan Docker *engine* sudah berjalan di *background*.

### Opsi 2 : Menjalankan secara lokal

#### Backend (C#)
1. Install **.NET 10.0 SDK**
   - Windows: Download from [dot.net](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - macOS (Homebrew): `brew install --cask dotnet-sdk`
2. Verify: `dotnet --version`

#### Frontend (TypeScript)
1. Install **Bun** runtime
   - Windows (PowerShell): `powershell -c "irm bun.sh/install.ps1 | iex"`
   - macOS/Linux: `curl -fsSL https://bun.sh/install | bash`
2. Verify: `bun --version`

## Getting Started

### Docker

Sebelum menjalankan Web App dengan Docker Desktop, Anda perlu mengatur konfigurasi *environment variables* untuk menghubungkan *frontend* dengan *backend*.

Buka direktori *frontend* dan buat file `.env` lalu tambahkan baris ini di dalamnya:
```bash
VITE_API_BASE_URL=http://localhost:5080
```

Setelah itu, jalankan perintah ini di *root directory*:
```bash
docker-compose up -d --build
```

Akses halaman Frontend di Docker Desktop

Untuk mematikan aplikasi:
```bash
docker-compose down
```

### Manual (lokal)

#### 1. Run Backend
```bash
cd backend
dotnet run
```

#### 2. Run Frontend
```bash
cd frontend
bun install
bun run dev
```

## Daftar Fitur & Status Implementasi

| No | Poin | Ya | Tidak |
|:--:|:---|:---:|:---:|
| 1 | Aplikasi berhasil di kompilasi tanpa kesalahan | ✔ |  |
| 2 | Aplikasi berhasil dijalankan | ✔ |  |
| 3 | Aplikasi dapat menerima input URL web, pilihan algoritma, CSS selector, dan jumlah hasil | ✔ |  |
| 4 | Aplikasi dapat melakukan scraping terhadap web pada input | ✔ |  |
| 5 | Aplikasi dapat menampilkan visualisasi pohon DOM | ✔ |  |
| 6 | Aplikasi dapat menelusuri pohon DOM dan menampilkan hasil penelusuran | ✔ |  |
| 7 | Aplikasi dapat menandai jalur tempuh oleh algoritma | ✔ |  |
| 8 | Aplikasi dapat menyimpan jalur yang ditempuh algoritma dalam traversal log | ✔ |  |
| 9 | **[Bonus]** Membuat video |  | ✔ |
| 10 | **[Bonus]** Deploy aplikasi | ✔ |  |
| 11 | **[Bonus]** Implementasi animasi pada penelusuran pohon | ✔ |  |
| 12 | **[Bonus]** Implementasi multithreading | ✔ |  |
| 13 | **[Bonus]** Implementasi LCA Binary Lifting | ✔ |  |

## Contributors
| Nama                           | NIM      | Github                                |
| ------------------------------ | -------- | --------------------------------------|
| Ray Owen Martin.               | 13524033 | [Tensai-033](https://github.com/Tensai-033)
| Emilio Justin                  | 13524043 | [Valz0504](https://github.com/Valz0504)
| Farrell Limjaya                | 13524046 | [Defaro123](https://github.com/Defaro123)

Tugas Besar 2 - IF2211 Strategi Algoritma