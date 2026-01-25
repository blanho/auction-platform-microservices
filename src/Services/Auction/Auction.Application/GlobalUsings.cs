
global using BuildingBlocks.Application.Abstractions;
global using BuildingBlocks.Application.Abstractions.Auditing;
global using Microsoft.Extensions.Logging;
global using BuildingBlocks.Application.Abstractions.Messaging;
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.Constants;
global using BuildingBlocks.Application.CQRS.Commands;
global using BuildingBlocks.Application.CQRS.Queries;
global using BuildingBlocks.Application.Helpers;
global using BuildingBlocks.Infrastructure.Caching;
global using BuildingBlocks.Infrastructure.Repository;
global using BuildingBlocks.Infrastructure.Repository.Specifications;
global using IUnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

global using AuctionService.Contracts.Events;

global using Auctions.Application.Interfaces;
global using Auctions.Application.DTOs.Auctions;
global using Auctions.Application.DTOs.Brands;
global using Auctions.Application.DTOs.Categories;
global using Auctions.Application.DTOs.Reviews;
global using Auctions.Application.DTOs.Bookmarks;
global using Auctions.Application.DTOs.Stats;
