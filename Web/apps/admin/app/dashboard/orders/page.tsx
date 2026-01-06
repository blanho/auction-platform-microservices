"use client";

import { Card, CardContent, CardHeader, CardTitle, Badge, Skeleton } from "@repo/ui";
import { formatCurrency, formatDateTime } from "@repo/utils";
import { ShoppingCart } from "lucide-react";

const statusColors: Record<string, string> = {
  PendingPayment: "bg-yellow-100 text-yellow-800",
  Paid: "bg-blue-100 text-blue-800",
  Shipped: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
  Refunded: "bg-gray-100 text-gray-800",
};

export default function AdminOrdersPage() {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Orders</h1>
        <p className="mt-2 text-muted-foreground">
          View all platform orders
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Orders</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12">
            <ShoppingCart className="h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No orders</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Connect to PaymentService to view orders
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
