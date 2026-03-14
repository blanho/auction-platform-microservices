
global using BuildingBlocks.Application.Abstractions;
global using Microsoft.Extensions.Logging;
global using BuildingBlocks.Application.Abstractions.Messaging;
global using BuildingBlocks.Application.Abstractions.Providers;
global using BuildingBlocks.Application.CQRS.Commands;
global using BuildingBlocks.Application.CQRS.Queries;

global using Bidding.Domain.Entities;
global using Bidding.Domain.Enums;
global using Bidding.Domain.Events;
global using Bidding.Domain.Constants;

global using Bidding.Application.DTOs;
global using Bidding.Application.Filtering;
global using Bidding.Application.Helpers;
global using Bidding.Application.Interfaces;

global using BidService.Contracts.Events;

global using MediatR;

global using AutoMapper;
global using Bidding.Application.Extensions.Mappings;

global using FluentValidation;

global using MassTransit;
