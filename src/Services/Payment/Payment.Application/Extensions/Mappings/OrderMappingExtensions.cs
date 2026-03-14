using AutoMapper;
using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Extensions.Mappings;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order, IMapper mapper)
    {
        return mapper.Map<OrderDto>(order);
    }

    public static List<OrderDto> ToDtoList(this IEnumerable<Order> orders, IMapper mapper)
    {
        return orders.Select(o => o.ToDto(mapper)).ToList();
    }
}
