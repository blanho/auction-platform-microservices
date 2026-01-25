
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.Constants;
global using BuildingBlocks.Infrastructure.Caching;
global using BuildingBlocks.Infrastructure.Repository;
global using BuildingBlocks.Infrastructure.Scheduling;

global using Auctions.Application.Interfaces;
global using Auctions.Application.DTOs.Auctions;
global using Auctions.Application.DTOs.Stats;
global using Auctions.Application.DTOs.Categories;
global using Auctions.Infrastructure.Persistence.Repositories;
global using Auctions.Infrastructure.Grpc;

global using AuctionService.Contracts.Events;
global using BidService.Contracts.Events;
global using StorageService.Contracts;

global using Orchestration.Sagas.BuyNow;
global using Orchestration.Sagas.BuyNow.Events;
