#!/bin/bash

echo "Creating fresh migrations for all services..."
echo ""

# Analytics Service
echo "1. Analytics Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Analytics/Analytics.Api/Analytics.Api.csproj \
  --startup-project src/Services/Analytics/Analytics.Api/Analytics.Api.csproj \
  --context AnalyticsDbContext \
  --output-dir Data/Migrations

# Auction Service  
echo ""
echo "2. Auction Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Auction/Auction.Infrastructure/Auction.Infrastructure.csproj \
  --startup-project src/Services/Auction/Auction.Api/Auction.Api.csproj \
  --context AuctionDbContext \
  --output-dir Migrations

# Bidding Service
echo ""
echo "3. Bidding Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Bidding/Bidding.Infrastructure/Bidding.Infrastructure.csproj \
  --startup-project src/Services/Bidding/Bidding.Api/Bidding.Api.csproj \
  --context BidDbContext \
  --output-dir Migrations

# Identity Service
echo ""
echo "4. Identity Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Identity/Identity.Api/Identity.Api.csproj \
  --startup-project src/Services/Identity/Identity.Api/Identity.Api.csproj \
  --context ApplicationDbContext \
  --output-dir Migrations

# Notification Service
echo ""
echo "5. Notification Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Notification/Notification.Infrastructure/Notification.Infrastructure.csproj \
  --startup-project src/Services/Notification/Notification.Api/Notification.Api.csproj \
  --context NotificationDbContext \
  --output-dir Persistence/Migrations

# Payment Service
echo ""
echo "6. Payment Service..."
dotnet ef migrations add InitialCreate \
  --project src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj \
  --startup-project src/Services/Payment/Payment.Api/Payment.Api.csproj \
  --context PaymentDbContext \
  --output-dir Persistence/Migrations

echo ""
echo "✅ All migrations created successfully!"
