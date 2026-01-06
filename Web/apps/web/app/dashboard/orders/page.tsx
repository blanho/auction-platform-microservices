"use client";

import Link from "next/link";
import { useBuyerOrders } from "@repo/hooks";
import { formatCurrency, formatDateTime } from "@repo/utils";
import { Card, CardContent, CardHeader, CardTitle, Badge, Skeleton, Button } from "@repo/ui";
import { Package, ArrowRight } from "lucide-react";

const statusColors: Record<string, string> = {
  PendingPayment: "bg-yellow-100 text-yellow-800",
  Paid: "bg-blue-100 text-blue-800",
  Shipped: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
  Refunded: "bg-gray-100 text-gray-800",
};

export default function OrdersPage() {
  const { data: orders, isLoading } = useBuyerOrders();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">My Orders</h1>
        <p className="mt-2 text-muted-foreground">Track your purchases and order history</p>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      ) : !orders?.items.length ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Package className="h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No orders yet</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Win an auction to see your orders here
            </p>
            <Link href="/auctions" className="mt-4">
              <Button>Browse Auctions</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {orders.items.map((order) => (
            <Card key={order.id}>
              <CardContent className="p-6">
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-3">
                      <h3 className="font-semibold">{order.auctionTitle}</h3>
                      <Badge className={statusColors[order.status] || "bg-gray-100"}>
                        {order.status}
                      </Badge>
                    </div>
                    <div className="mt-2 grid gap-1 text-sm text-muted-foreground sm:grid-cols-2">
                      <p>Order ID: {order.id.slice(0, 8)}...</p>
                      <p>Date: {formatDateTime(order.createdAt)}</p>
                      {order.trackingNumber && (
                        <p>Tracking: {order.trackingNumber}</p>
                      )}
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold">{formatCurrency(order.amount)}</p>
                    {order.status === "PendingPayment" && (
                      <Link href={`/dashboard/orders/${order.id}`}>
                        <Button size="sm" className="mt-2">
                          Pay Now
                        </Button>
                      </Link>
                    )}
                    {order.status !== "PendingPayment" && (
                      <Link href={`/dashboard/orders/${order.id}`}>
                        <Button variant="ghost" size="sm" className="mt-2">
                          View Details
                          <ArrowRight className="ml-1 h-4 w-4" />
                        </Button>
                      </Link>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
