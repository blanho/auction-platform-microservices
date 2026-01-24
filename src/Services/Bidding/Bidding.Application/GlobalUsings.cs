
global using BuildingBlocks.Application.Abstractions;
global using Microsoft.Extensions.Logging;
global using BuildingBlocks.Application.Abstractions.Messaging;
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.CQRS.Commands;
global using BuildingBlocks.Application.CQRS.Queries;
global using BuildingBlocks.Application.Abstractions.Persistence;
global using BuildingBlocks.Infrastructure.Caching;
global using BuildingBlocks.Infrastructure.Locking;
global using BuildingBlocks.Infrastructure.Repository;

global using Bidding.Domain.Entities;
global using Bidding.Domain.Enums;
global using Bidding.Domain.ValueObjects;
global using Bidding.Domain.Events;

global using Bidding.Application.DTOs;
global using Bidding.Application.Interfaces;
global using Bidding.Application.Constants;

global using BidService.Contracts.Events;

global using MediatR;

global using AutoMapper;

global using FluentValidation;

global using MassTransit;
