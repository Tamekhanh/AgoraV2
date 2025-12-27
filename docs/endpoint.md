# UserController Endpoints

---

### Get Paged Users
- **Endpoint:** `GET /api/users`
- **Input:** `PagedRequest` (from query)
- **Output:** `PagedResult<UserDTO>`
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of users.
  - `400 Bad Request`: If an error occurs during fetching.

---

### Get User By ID
- **Endpoint:** `GET /api/users/{id}`
- **Input:** `id` (from route)
- **Output:** `UserDTO`
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: Returns the user.
  - `404 Not Found`: If the user is not found.
  - `400 Bad Request`: If an error occurs.

---

### Create User
- **Endpoint:** `POST /api/users`
- **Input:** `UserCreateDTO` (from body)
- **Output:** The created `User` object.
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the created user.
  - `400 Bad Request`: If the input is invalid or an error occurs.

---

### Update User
- **Endpoint:** `PUT /api/users/{id}`
- **Input:** 
  - `id` (from route)
  - `UserUpdateDTO` (from body)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `400 Bad Request`: If an error occurs.

---

### Update Self
- **Endpoint:** `PUT /api/users/self`
- **Input:** `UserUpdateDTO` (from body)
- **Output:** The updated `User` object.
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns the updated user.
  - `401 Unauthorized`: If the user is not authenticated.
  - `404 Not Found`: If the user is not found.
  - `400 Bad Request`: If an error occurs.

---

### Delete User
- **Endpoint:** `DELETE /api/users/{id}`
- **Input:** `id` (from route)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the deletion is successful (implicit).
  - `400 Bad Request`: If an error occurs.

---

### Login
- **Endpoint:** `POST /api/users/login`
- **Input:** `LoginRequest` (from body)
- **Output:** `LoginResponse`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the login response.
  - `401 Unauthorized`: If login fails.

---

### Update Role
- **Endpoint:** `PUT /api/users/role/{id}`
- **Input:** 
  - `id` (from route)
  - `RoleUpdateRequest` (from body)
- **Output:** None
- **Access:** 1 (Admin)
- **HTTP Status Codes:**
  - `200 OK`: If the role is updated successfully.
  - `400 Bad Request`: If the input is invalid or the user tries to change their own role.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Update Account Self
- **Endpoint:** `PUT /api/users/account/self`
- **Input:** `UserUpdateAccount` (from body)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the account is updated successfully.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If an error occurs.

---

# CartController Endpoints

---

### Get Cart
- **Endpoint:** `GET /api/cart`
- **Input:** None
- **Output:** `CartDTO`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns the user's cart.
  - `400 Bad Request`: If an error occurs.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Add To Cart
- **Endpoint:** `POST /api/cart/items`
- **Input:** `AddToCartRequest` (from body)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the item is added successfully.
  - `400 Bad Request`: If the input is invalid.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Update Cart Item
- **Endpoint:** `PUT /api/cart/items`
- **Input:** `UpdateCartItemRequest` (from body)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the item is updated successfully.
  - `400 Bad Request`: If the input is invalid.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Remove From Cart
- **Endpoint:** `DELETE /api/cart/items/{productId}`
- **Input:** `productId` (from route)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the item is removed successfully.
  - `400 Bad Request`: If the input is invalid.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Clear Cart
- **Endpoint:** `DELETE /api/cart`
- **Input:** None
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the cart is cleared successfully.
  - `400 Bad Request`: If an error occurs.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Increase Cart Item
- **Endpoint:** `POST /api/cart/items/{productId}/increase`
- **Input:** `productId` (from route)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the item quantity is increased successfully.
  - `400 Bad Request`: If the input is invalid.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

### Decrease Cart Item
- **Endpoint:** `POST /api/cart/items/{productId}/decrease`
- **Input:** `productId` (from route)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the item quantity is decreased successfully.
  - `400 Bad Request`: If the input is invalid.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

# CategoryController Endpoints

---

### Get Paged Categories
- **Endpoint:** `GET /api/categories`
- **Input:** `PagedRequest` (from query)
- **Output:** `PagedResult<CategoryDTO>`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of categories.
  - `400 Bad Request`: If an error occurs.

---

### Get Category By ID
- **Endpoint:** `GET /api/categories/{id}`
- **Input:** `id` (from route)
- **Output:** `CategoryDTO`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the category.
  - `404 Not Found`: If the category is not found.
  - `400 Bad Request`: If an error occurs.

---

### Create Category
- **Endpoint:** `POST /api/categories`
- **Input:** `CategoryDTO` (from body)
- **Output:** `CategoryDTO`
- **Access:** 1 (Admin)
- **HTTP Status Codes:**
  - `200 OK`: Returns the created category.
  - `400 Bad Request`: If an error occurs.

---

### Update Category
- **Endpoint:** `PUT /api/categories/{id}`
- **Input:** 
  - `id` (from route)
  - `CategoryDTO` (from body)
- **Output:** None
- **Access:** 1 (Admin)
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `400 Bad Request`: If an error occurs.

---

### Delete Category
- **Endpoint:** `DELETE /api/categories/{id}`
- **Input:** `id` (from route)
- **Output:** None
- **Access:** 1 (Admin)
- **HTTP Status Codes:**
  - `200 OK`: If the deletion is successful.
  - `400 Bad Request`: If an error occurs.

---

# ImageController Endpoints

---

### Update User Image (Self)
- **Endpoint:** `PUT /api/images/users`
- **Input:** `IFormFile` (file)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If an error occurs.

---

### Update User Image (Admin/Staff)
- **Endpoint:** `PUT /api/images/controller/users/{userId}`
- **Input:** 
  - `userId` (from route)
  - `IFormFile` (file)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Shop Image (Admin/Staff)
- **Endpoint:** `PUT /api/images/controller/shops/{shopId}`
- **Input:** 
  - `shopId` (from route)
  - `IFormFile` (file)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Shop Image (Self)
- **Endpoint:** `PUT /api/images/shops`
- **Input:** `IFormFile` (file)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `401 Unauthorized`: If the user is not authenticated.
  - `404 Not Found`: If the shop is not found for the user.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Product Image (Admin/Staff)
- **Endpoint:** `PUT /api/images/controller/products/{productId}`
- **Input:** 
  - `productId` (from route)
  - `IFormFile` (file)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Product Image (Self)
- **Endpoint:** `PUT /api/images/products/{productId}`
- **Input:** 
  - `productId` (from route)
  - `IFormFile` (file)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the image is updated successfully.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If the user does not have a shop or the product ID is invalid.
  - `404 Not Found`: If the product is not found for the user's shop.
  - `403 Forbidden`: If the user does not own the product.
  - `500 Internal Server Error`: If an error occurs.

---

### Get Image By ID
- **Endpoint:** `GET /api/images/{id}`
- **Input:** 
  - `id` (from route)
  - `ReSize` (query, optional)
  - `isSmall` (query, optional)
  - `width` (query, optional)
  - `height` (query, optional)
- **Output:** Image file
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the image file.
  - `304 Not Modified`: If the cached image is still valid.
  - `404 Not Found`: If the image is not found.
  - `400 Bad Request`: If the image data is empty or resize dimensions are too large.
  - `500 Internal Server Error`: If an unexpected error occurs.

---

# OrderController Endpoints

---

### Checkout
- **Endpoint:** `POST /api/Order/checkout`
- **Input:** `CheckoutRequest` (from body)
- **Output:** `CheckoutResponse`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns the checkout response.
  - `400 Bad Request`: If an error occurs during checkout.

---

### Get Orders (Self)
- **Endpoint:** `GET /api/Order`
- **Input:** `PagedRequest` (from query)
- **Output:** `PagedResult<OrderDTO>`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of the user's orders.
  - `400 Bad Request`: If an error occurs.

---

### Get Orders By User ID (Admin/Staff)
- **Endpoint:** `GET /api/Order/controller/{userId}`
- **Input:** 
  - `userId` (from route)
  - `PagedRequest` (from query)
- **Output:** `PagedResult<OrderDTO>`
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of orders for the specified user.
  - `400 Bad Request`: If an error occurs.

---

### Get Order Detail (Self)
- **Endpoint:** `GET /api/Order/{orderId}`
- **Input:** `orderId` (from route)
- **Output:** `OrderDTO`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns the order details.
  - `400 Bad Request`: If an error occurs.

---

### Get Order Detail By Order ID (Admin/Staff)
- **Endpoint:** `GET /api/Order/controller/detail/{orderId}`
- **Input:** `orderId` (from route)
- **Output:** `OrderDTO`
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: Returns the order details.
  - `400 Bad Request`: If an error occurs.

---

# PaymentController Endpoints

---

### Process Payment
- **Endpoint:** `POST /api/Payment`
- **Input:** `PaymentRequest` (from body)
- **Output:** `PaymentResponse`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: Returns the payment response if successful.
  - `400 Bad Request`: If the model state is invalid or the payment fails.

---

# ProductController Endpoints

---

### Get Paged Products
- **Endpoint:** `GET /api/products`
- **Input:** `PagedRequest` (from query)
- **Output:** `PagedResult<ProductDTO>`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of products.
  - `500 Internal Server Error`: If an error occurs.

---

### Get Product By ID
- **Endpoint:** `GET /api/products/{id}`
- **Input:** `id` (from route)
- **Output:** `ProductDTO`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the product.
  - `500 Internal Server Error`: If an error occurs.

---

### Create Product
- **Endpoint:** `POST /api/products`
- **Input:** `CreateProductRequest` (from body)
- **Output:** `ProductDTO`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `201 Created`: Returns the created product.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If the user does not have a shop or the input is invalid.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Product (Admin/Staff)
- **Endpoint:** `PUT /api/products/controller/{id}`
- **Input:** 
  - `id` (from route)
  - `ProductUpdateRequest` (from body)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Product (Self)
- **Endpoint:** `PUT /api/products/{id}`
- **Input:** 
  - `id` (from route)
  - `ProductUpdateRequest` (from body)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If the user does not have a shop or the product ID is invalid.
  - `404 Not Found`: If the product is not found for the user's shop.
  - `403 Forbidden`: If the user does not own the product.
  - `500 Internal Server Error`: If an error occurs.

---

### Delete Product (Admin/Staff)
- **Endpoint:** `DELETE /api/products/controller/{id}`
- **Input:** `id` (from route)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the deletion is successful.
  - `500 Internal Server Error`: If an error occurs.

---

### Delete Product (Self)
- **Endpoint:** `DELETE /api/products/{id}`
- **Input:** `id` (from route)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the deletion is successful.
  - `401 Unauthorized`: If the user is not authenticated.
  - `400 Bad Request`: If the user does not have a shop.
  - `404 Not Found`: If the product is not found for the user's shop.
  - `500 Internal Server Error`: If an error occurs.

---

### Get Product Stock
- **Endpoint:** `GET /api/products/stock/{productId}`
- **Input:** `productId` (from route)
- **Output:** Stock quantity (integer)
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the stock quantity.
  - `404 Not Found`: If the product is not found.
  - `500 Internal Server Error`: If an error occurs.

---

# ShopController Endpoints

---

### Get Paged Shops
- **Endpoint:** `GET /api/shops`
- **Input:** `PagedRequest` (from query)
- **Output:** `PagedResult<ShopRequest>`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns a paged list of shops.
  - `500 Internal Server Error`: If an error occurs.

---

### Get Shop By ID
- **Endpoint:** `GET /api/shops/{id}`
- **Input:** `id` (from route)
- **Output:** `ShopRequest`
- **Access:** Public
- **HTTP Status Codes:**
  - `200 OK`: Returns the shop.
  - `500 Internal Server Error`: If an error occurs.

---

### Create Shop
- **Endpoint:** `POST /api/shops`
- **Input:** `CreateShopRequest` (from body)
- **Output:** `CreateShopRequest`
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `201 Created`: Returns the created shop.
  - `401 Unauthorized`: If the user is not authenticated.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Shop (Admin/Staff)
- **Endpoint:** `PUT /api/shops/{id}`
- **Input:** 
  - `id` (from route)
  - `ShopUpdateRequest` (from body)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `500 Internal Server Error`: If an error occurs.

---

### Update Shop (Self)
- **Endpoint:** `PUT /api/shops/self`
- **Input:** `ShopUpdateRequest` (from body)
- **Output:** None
- **Access:** Authenticated users
- **HTTP Status Codes:**
  - `200 OK`: If the update is successful.
  - `401 Unauthorized`: If the user is not authenticated.
  - `404 Not Found`: If the shop is not found for the user.
  - `403 Forbidden`: If the user does not own the shop.
  - `500 Internal Server Error`: If an error occurs.

---

### Delete Shop
- **Endpoint:** `DELETE /api/shops/{id}`
- **Input:** `id` (from route)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the deletion is successful.
  - `500 Internal Server Error`: If an error occurs.

---

### Change Shop Status
- **Endpoint:** `PUT /api/shops/{shopId}/status/{status}`
- **Input:** 
  - `shopId` (from route)
  - `status` (from route)
- **Output:** None
- **Access:** 1 (Admin), 2 (Staff)
- **HTTP Status Codes:**
  - `200 OK`: If the status is changed successfully.
  - `400 Bad Request`: If the status value is invalid.
  - `500 Internal Server Error`: If an error occurs.

---
