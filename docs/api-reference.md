# API Reference

This document describes the public API surface exposed through the YARP gateway. All endpoints are accessed via the gateway at `http://localhost:6001` (development) or your production domain.

---

## Table of Contents

- [Authentication](#authentication)
- [Common Patterns](#common-patterns)
- [Identity Endpoints](#identity-endpoints)
- [Auction Endpoints](#auction-endpoints)
- [Bidding Endpoints](#bidding-endpoints)
- [Payment Endpoints](#payment-endpoints)
- [Notification Endpoints](#notification-endpoints)
- [Search Endpoints](#search-endpoints)
- [Storage Endpoints](#storage-endpoints)
- [Analytics Endpoints](#analytics-endpoints)
- [WebSocket (SignalR)](#websocket-signalr)
- [Health Checks](#health-checks)
- [Error Responses](#error-responses)

---

## Authentication

The platform uses **JWT Bearer tokens** for authentication.

### Obtaining a Token

```
POST /identity/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
  "expiresIn": 3600
}
```

### Using the Token

Include the token in the `Authorization` header for all authenticated requests:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Refreshing Tokens

```
POST /identity/refresh-token
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g..."
}
```

### OAuth Login

```
GET /identity/external-login?provider=Google&returnUrl=/
GET /identity/external-login?provider=Facebook&returnUrl=/
```

Redirects to the OAuth provider. After authentication, the user is redirected back with a JWT.

---

## Common Patterns

### Pagination

List endpoints support pagination via query parameters:

```
GET /auctions?pageNumber=1&pageSize=20
```

**Response wrapper:**

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Filtering and Sorting

```
GET /auctions?status=Active&categoryId=123&sortBy=endTime&sortOrder=desc
```

### Error Format

All errors follow a consistent format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "amount": ["Bid amount must be greater than the current price."]
  }
}
```

---

## Identity Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/identity/register` | No | Register a new user |
| POST | `/identity/login` | No | Login and receive JWT |
| POST | `/identity/refresh-token` | No | Refresh an expired access token |
| POST | `/identity/revoke-token` | Yes | Revoke a refresh token |
| GET | `/identity/me` | Yes | Get current user profile |
| PUT | `/identity/me` | Yes | Update current user profile |
| POST | `/identity/change-password` | Yes | Change password |
| POST | `/identity/forgot-password` | No | Request password reset email |
| POST | `/identity/reset-password` | No | Reset password with token |
| GET | `/identity/external-login` | No | Initiate OAuth flow |

---

## Auction Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/auctions` | No | List auctions (paginated, filterable) |
| GET | `/auctions/{id}` | No | Get auction details |
| POST | `/auctions` | Yes | Create a new auction |
| PUT | `/auctions/{id}` | Yes | Update auction (owner only, draft status) |
| DELETE | `/auctions/{id}` | Yes | Delete auction (owner only, draft status) |
| POST | `/auctions/{id}/publish` | Yes | Publish a draft auction |
| POST | `/auctions/{id}/cancel` | Yes | Cancel an auction (owner only) |
| POST | `/auctions/{id}/buy-now` | Yes | Execute buy-now on an auction |
| GET | `/auctions/{id}/bids` | No | Get bid history for an auction |

### Categories

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/categories` | No | List all categories |
| GET | `/categories/{id}` | No | Get category details |
| POST | `/categories` | Yes (Admin) | Create category |
| PUT | `/categories/{id}` | Yes (Admin) | Update category |

### Brands

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/brands` | No | List all brands |
| POST | `/brands` | Yes (Admin) | Create brand |

### Bookmarks

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/bookmarks` | Yes | Get user's bookmarked auctions |
| POST | `/bookmarks` | Yes | Bookmark an auction |
| DELETE | `/bookmarks/{auctionId}` | Yes | Remove bookmark |

### Reviews

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/reviews?auctionId={id}` | No | Get reviews for an auction |
| POST | `/reviews` | Yes | Create a review (after auction completion) |

---

## Bidding Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/bids` | Yes | Place a bid |
| GET | `/bids/my` | Yes | Get current user's bid history |
| DELETE | `/bids/{id}` | Yes | Retract a bid (if allowed) |

### Auto-Bids

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/autobids` | Yes | Set up an auto-bid |
| GET | `/autobids` | Yes | Get user's auto-bid configurations |
| PUT | `/autobids/{id}` | Yes | Update auto-bid max amount |
| DELETE | `/autobids/{id}` | Yes | Cancel an auto-bid |

### Request: Place a Bid

```
POST /bids
Authorization: Bearer {token}
Content-Type: application/json

{
  "auctionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 150.00
}
```

**Response (201 Created):**

```json
{
  "id": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "auctionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bidderId": "user-123",
  "amount": 150.00,
  "placedAt": "2026-03-15T10:30:00Z"
}
```

---

## Payment Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/payments/wallet` | Yes | Get user's wallet |
| POST | `/payments/wallet/deposit` | Yes | Deposit funds (via Stripe) |
| POST | `/payments/wallet/withdraw` | Yes | Withdraw funds |
| GET | `/payments/orders` | Yes | Get user's orders |
| GET | `/payments/orders/{id}` | Yes | Get order details |
| POST | `/payments/webhooks/stripe` | No* | Stripe webhook endpoint |

*Stripe webhooks are verified via webhook signature, not JWT.

---

## Notification Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/notifications` | Yes | Get user's notifications (paginated) |
| GET | `/notifications/unread-count` | Yes | Get unread notification count |
| PUT | `/notifications/{id}/read` | Yes | Mark notification as read |
| PUT | `/notifications/read-all` | Yes | Mark all notifications as read |
| DELETE | `/notifications/{id}` | Yes | Delete a notification |
| PUT | `/notifications/{id}/archive` | Yes | Archive a notification |

---

## Search Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/search/auctions` | No | Full-text search with filters |
| GET | `/search/auctions/suggest` | No | Autocomplete suggestions |

### Search Query Parameters

| Parameter | Type | Description |
|---|---|---|
| `q` | string | Search query text |
| `category` | string | Filter by category |
| `brand` | string | Filter by brand |
| `minPrice` | decimal | Minimum current price |
| `maxPrice` | decimal | Maximum current price |
| `status` | string | Auction status filter |
| `sortBy` | string | Sort field (relevance, price, endTime) |
| `pageNumber` | int | Page number |
| `pageSize` | int | Items per page |

---

## Storage Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/storage/upload` | Yes | Upload a file (multipart/form-data) |
| GET | `/storage/files/{id}` | Yes | Get file metadata |
| DELETE | `/storage/files/{id}` | Yes | Delete a file (owner only) |

### Upload Request

```
POST /storage/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: (binary)
```

**Response:**

```json
{
  "id": "file-uuid",
  "fileName": "photo.jpg",
  "url": "https://storage.blob.core.windows.net/uploads/photo.jpg",
  "contentType": "image/jpeg",
  "size": 245760
}
```

---

## Analytics Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/analytics/dashboard` | Yes (Admin) | Dashboard summary data |
| GET | `/analytics/auctions/{id}/stats` | Yes | Auction performance stats |

---

## WebSocket (SignalR)

### Connection

```
URL: wss://localhost:6001/hubs/notifications
Query: ?access_token={jwt_token}
```

### Client Events (Server → Client)

| Event | Payload | Description |
|---|---|---|
| `BidPlaced` | `{ auctionId, amount, bidderName, timestamp }` | New bid on a watched auction |
| `AuctionStatusChanged` | `{ auctionId, newStatus }` | Auction status update |
| `OutBid` | `{ auctionId, newAmount }` | User has been outbid |
| `AuctionWon` | `{ auctionId, finalPrice }` | User won an auction |
| `NotificationReceived` | `{ id, title, message, type }` | New notification |

### Groups

Users are automatically added to groups:
- `auction:{auctionId}` — when viewing an auction detail page
- `user:{userId}` — personal notification channel (always connected)

### Frontend Usage

```typescript
import { HubConnectionBuilder } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl('/hubs/notifications', {
    accessTokenFactory: () => getAccessToken(),
  })
  .withAutomaticReconnect()
  .build();

connection.on('BidPlaced', (data) => {
  // Handle new bid
});

await connection.start();
```

---

## Health Checks

Every service exposes three health endpoints:

| Endpoint | Purpose | Used By |
|---|---|---|
| `GET /health` | Overall service health | General monitoring |
| `GET /health/ready` | Readiness check | Kubernetes readiness probe |
| `GET /health/live` | Liveness check | Kubernetes liveness probe |

**Response:**

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "postgresql", "status": "Healthy" },
    { "name": "redis", "status": "Healthy" },
    { "name": "rabbitmq", "status": "Healthy" }
  ]
}
```

---

## Error Responses

### Standard HTTP Status Codes

| Code | Meaning |
|---|---|
| 200 | Success |
| 201 | Created |
| 204 | No Content (successful delete) |
| 400 | Bad Request (validation errors) |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 409 | Conflict (e.g., duplicate bid, auction already published) |
| 422 | Unprocessable Entity (business rule violation) |
| 429 | Too Many Requests (rate limited) |
| 500 | Internal Server Error |

### Validation Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "amount": ["Amount must be greater than 0."],
    "auctionId": ["Auction ID is required."]
  }
}
```

### Business Rule Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Business Rule Violation",
  "status": 422,
  "detail": "Cannot place a bid on your own auction."
}
```

### Rate Limit Response

```
HTTP/1.1 429 Too Many Requests
Retry-After: 60
```
