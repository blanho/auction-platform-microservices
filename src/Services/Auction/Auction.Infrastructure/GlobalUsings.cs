global using BuildingBlocks.Application.Abstractions;
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.Constants;
global using BuildingBlocks.Infrastructure.Caching;
global using BuildingBlocks.Infrastructure.Scheduling;

global using ICacheService = BuildingBlocks.Application.Abstractions.ICacheService;
global using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

global using Auctions.Application.DTOs.Auctions;
global using Auctions.Application.DTOs.Categories;
global using Auctions.Application.DTOs.Stats;
global using Auctions.Application.Interfaces;
global using Auctions.Infrastructure.Persistence.Repositories;

global using AuctionService.Contracts.Events;
global using BidService.Contracts.Events;
global using OrchestrationService.Contracts.Events;
