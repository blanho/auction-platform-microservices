global using BuildingBlocks.Application.Abstractions;
global using BuildingBlocks.Application.Abstractions.Auditing;
global using BuildingBlocks.Application.Abstractions.Messaging;
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.Constants;
global using BuildingBlocks.Application.CQRS.Commands;
global using BuildingBlocks.Application.CQRS.Queries;

global using Microsoft.Extensions.Logging;

global using AuctionService.Contracts.Events;

global using Auctions.Application.DTOs;

global using Auctions.Application.Enums;
global using Auctions.Application.Interfaces;
global using Auction = Auctions.Domain.Entities.Auction;
global using MassTransit;
