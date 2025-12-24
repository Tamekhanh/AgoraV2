# Kịch bản Demo & Thuyết trình: Event-Driven Architecture (Outbox, Saga, RabbitMQ)

Tài liệu này hướng dẫn bạn cách demo và thuyết trình các tính năng kiến trúc nâng cao đã được cài đặt trong dự án AgoraV2.

## 1. Chuẩn bị môi trường
*   **RabbitMQ**: Đảm bảo RabbitMQ đang chạy (Docker hoặc cài trực tiếp).
    *   URL quản trị: `http://localhost:15672` (User/Pass mặc định: `guest`/`guest`).
*   **Database**: Đảm bảo SQL Server đang chạy và database đã được cập nhật (chạy migration nếu cần).
*   **Ứng dụng**: Chạy `Agora.API`.

## 2. Giới thiệu chung (Concept)
Bắt đầu bằng việc giới thiệu vấn đề và giải pháp:
*   **Vấn đề**: Trong hệ thống phân tán, làm sao để đảm bảo tính nhất quán dữ liệu khi một hành động (như tạo đơn hàng) cần cập nhật nhiều nơi (Kho, Thanh toán)? Làm sao để không bị mất sự kiện khi mạng lỗi?
*   **Giải pháp**:
    1.  **RabbitMQ**: Message Broker để giao tiếp bất đồng bộ.
    2.  **Outbox Pattern**: Đảm bảo "Lưu DB thành công thì mới gửi tin nhắn".
    3.  **Saga Pattern**: Quản lý giao dịch nhiều bước (Order -> Stock -> Payment).
    4.  **Idempotency**: Đảm bảo một tin nhắn không bị xử lý 2 lần (tránh trừ tiền 2 lần).

---

## 3. Demo Chi tiết (Step-by-Step)

### Kịch bản 1: Đặt hàng thành công (Happy Path)

**Bước 1: Thực hiện đặt hàng (Checkout)**
*   Mở Swagger (`https://localhost:5000/swagger`).
*   Gọi API `POST /api/Order/checkout`.
*   **Giải thích Code (`OrderService.cs`)**:
    *   Chỉ vào đoạn `BeginTransactionAsync`.
    *   Show code lưu `Order` và `OutboxMessage` trong cùng một transaction.
    *   *Thông điệp*: "Nếu lưu Order thất bại, Outbox cũng không được lưu -> Đảm bảo tính nhất quán."

**Bước 2: Outbox Worker hoạt động**
*   **Quan sát Logs (Console)**: Tìm dòng log `Processing outbox message...`.
*   **Giải thích Code (`OutboxWorker.cs`)**:
    *   Worker chạy ngầm (Background Service), quét bảng `OutboxMessages` mỗi 5 giây.
    *   Lấy tin nhắn -> Gửi sang RabbitMQ -> Đánh dấu đã xử lý.
    *   *Thông điệp*: "Tách biệt việc lưu DB và gửi tin nhắn giúp hệ thống phản hồi nhanh hơn và tin cậy hơn."

**Bước 3: Saga Flow (Quy trình xử lý)**
*   **Quan sát Logs**: Chỉ ra chuỗi sự kiện liên tiếp trong Console:
    1.  `Handling OrderCreatedEvent` -> `StockReservedEventHandler`: Trừ tồn kho.
        *   -> Publish `StockReservedEvent`.
    2.  `Handling StockReservedEvent` -> `StockReservedEventHandler`: Gọi Payment Service.
        *   -> Publish `PaymentCompletedEvent`.
    3.  `Handling PaymentCompletedEvent` -> `PaymentCompletedEventHandler`: Cập nhật trạng thái đơn hàng thành `Paid`.
*   **Kiểm tra Database**:
    *   Bảng `Order`: Trạng thái `PaymentStatus` = "Paid".
    *   Bảng `Product`: `StockQty` đã bị trừ.

### Kịch bản 2: Xử lý trùng lặp (Idempotency)

*   **Giải thích Code (`RabbitMQEventBus.cs`)**:
    *   Show đoạn check bảng `ProcessedMessages`.
    *   Logic: `if (alreadyProcessed) return;`
*   **Demo**:
    *   Mở bảng `ProcessedMessages` trong SQL Server.
    *   Show các dòng dữ liệu: `MessageId` + `ConsumerName`.
    *   *Thông điệp*: "Nếu RabbitMQ gửi lại tin nhắn (do mạng lag), hệ thống sẽ biết tin nhắn này đã xử lý rồi và bỏ qua."

### Kịch bản 3: Giao dịch thất bại & Bù trừ (Compensation)

*   **Tình huống**: Đặt hàng số lượng lớn hơn tồn kho.
*   **Thực hiện**:
    *   Sửa số lượng trong Giỏ hàng (Cart) lớn hơn `StockQty` của sản phẩm.
    *   Gọi API Checkout.
*   **Quan sát Logs**:
    1.  `Handling OrderCreatedEvent`.
    2.  `Stock reservation failed` (Log Warning).
    3.  Publish `StockReservationFailedEvent`.
    4.  `Handling StockReservationFailedEvent` -> Cập nhật Order thành `Cancelled`.
*   **Kết quả**: Đơn hàng bị hủy tự động, hệ thống vẫn nhất quán.

---

## 4. Tổng kết
Hệ thống hiện tại đã đạt được:
1.  **Reliability (Tin cậy)**: Không mất đơn hàng nhờ Outbox.
2.  **Consistency (Nhất quán)**: Saga đảm bảo tiền và hàng khớp nhau.
3.  **Scalability (Mở rộng)**: Các service (Order, Payment, Inventory) hoạt động độc lập qua RabbitMQ.
