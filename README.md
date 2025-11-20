# 📦 Agora

**Agora** là dự án được xây dựng dựa trên kiến trúc **N-Layers / Clean Architecture**, tập trung vào sự tách biệt giữa các mối quan tâm (Separation of Concerns) và khả năng mở rộng.

## 🏗 Kiến trúc Dự án

Solution được chia thành các tầng chức năng rõ ràng:

* **Agora.Domain**: Chứa các Entities, Interfaces và Core Logic.
* **Agora.Application**: Chứa Business Logic, DTOs, Services.
* **Agora.Infrastructure**: Xử lý Database Context, Repositories, External Services.
* **Agora.Auth**: Module xử lý xác thực và phân quyền.
* **Agora.Payment**: Module tích hợp thanh toán.
* **Agora.API**: Entry point của ứng dụng (Web API).

---

## 🚀 Getting Started

Làm theo các bước dưới đây để cài đặt và khởi chạy dự án trên môi trường local.

### 1. Clone Project

Mở terminal và chạy lệnh sau để tải mã nguồn về máy:

```bash
git clone [https://github.com/HSUxTHP/Agora](https://github.com/HSUxTHP/Agora)
cd Agora
```
### 2. Restore Dependencies
Tải và khôi phục toàn bộ các thư viện NuGet cần thiết cho solution:

```bash
dotnet restore
```
### 3. Cấu hình (App Settings)
⚠️ Lưu ý: File appsettings.json thường không được commit lên git vì lý do bảo mật.

Bạn cần tạo file appsettings.json trong thư mục Agora.API/ hoặc sử dụng User Secrets cho môi trường Development.

Cách 1: Tạo file config Tạo file Agora.API/appsettings.json và thêm các keys cần thiết (ConnectionStrings, JWT Settings, v.v.).

Cách 2: Sử dụng User Secrets (Khuyên dùng)

```bash
cd Agora.API
dotnet user-secrets set "Jwt:Key" "your_super_secret_key_here"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=AgoraDB;..."
```
4. Khởi tạo Database SQL Server 20 +
Dự án sử dụng SQL Server
Yêu cầu tải SQL Server để có thể sử dụng

- mở File Agora.sql và chạy từng khối

```bash
cd Agora.API
```
dotnet ef database update
5. Chạy ứng dụng (Run API)
Khởi động Web API:

```bash

cd Agora.API
dotnet build
dotnet run
```
#### Sau khi khởi động thành công, API sẽ lắng nghe tại: 👉 https://localhost:5000 (hoặc port được cấu hình trong launchSettings.json).

## 🧩 Công cụ phát triển
Bạn có thể mở dự án bằng các IDE phổ biến:

Visual Studio: Mở file Agora.sln.

Visual Studio Code: Mở thư mục root và gõ code .

## 📂 Cấu trúc thư mục
Plaintext

Agora/
├── Agora.sln                # Solution file
├── Agora.Domain/            # Core Entities & Domain Logic
├── Agora.Application/       # Business Services & Use Cases
├── Agora.Infrastructure/    # Data Access & External Libs
├── Agora.Auth/              # Authentication Module
├── Agora.Payment/           # Payment Processing Module
├── Agora.API/               # REST API Layer
└── .gitignore               # Git ignore rules