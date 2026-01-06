"use client";

import { useSellerOrders, useShipOrder } from "@repo/hooks";
import { formatCurrency, formatDateTime } from "@repo/utils";
import {
  Card,
  CardContent,
  Button,
  Badge,
  Skeleton,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Input,
  Label,
} from "@repo/ui";
import { Package, Truck } from "lucide-react";
import { useState } from "react";

const statusColors: Record<string, string> = {
  PendingPayment: "bg-yellow-100 text-yellow-800",
  Paid: "bg-blue-100 text-blue-800",
  Shipped: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
  Refunded: "bg-gray-100 text-gray-800",
};

export default function SellerOrdersPage() {
  const { data: orders, isLoading } = useSellerOrders();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Orders</h1>
        <p className="mt-2 text-muted-foreground">
          Manage orders from your sold auctions
        </p>
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
              Orders from your sold auctions will appear here
            </p>
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
                      {order.paidAt && <p>Paid: {formatDateTime(order.paidAt)}</p>}
                      {order.trackingNumber && <p>Tracking: {order.trackingNumber}</p>}
                    </div>
                    {order.shippingAddress && (
                      <div className="mt-2 text-sm">
                        <p className="font-medium">Ship to:</p>
                        <p className="text-muted-foreground">
                          {order.shippingAddress.street}, {order.shippingAddress.city},{" "}
                          {order.shippingAddress.state} {order.shippingAddress.postalCode}
                        </p>
                      </div>
                    )}
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold">{formatCurrency(order.sellerPayout)}</p>
                    <p className="text-xs text-muted-foreground">
                      Your payout (after {formatCurrency(order.platformFee)} fee)
                    </p>
                    {order.status === "Paid" && <ShipOrderDialog orderId={order.id} />}
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

function ShipOrderDialog({ orderId }: { orderId: string }) {
  const [open, setOpen] = useState(false);
  const [trackingNumber, setTrackingNumber] = useState("");
  const [carrier, setCarrier] = useState("");
  const { mutate: shipOrder, isPending } = useShipOrder();

  function handleShip() {
    shipOrder(
      { orderId, trackingNumber, carrier },
      {
        onSuccess: () => {
          setOpen(false);
          setTrackingNumber("");
          setCarrier("");
        },
      }
    );
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button size="sm" className="mt-2">
          <Truck className="mr-2 h-4 w-4" />
          Mark Shipped
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ship Order</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 pt-4">
          <div className="space-y-2">
            <Label htmlFor="carrier">Carrier</Label>
            <Input
              id="carrier"
              placeholder="e.g., FedEx, UPS, USPS"
              value={carrier}
              onChange={(e) => setCarrier(e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="tracking">Tracking Number</Label>
            <Input
              id="tracking"
              placeholder="Enter tracking number"
              value={trackingNumber}
              onChange={(e) => setTrackingNumber(e.target.value)}
            />
          </div>
          <Button
            className="w-full"
            onClick={handleShip}
            disabled={isPending || !trackingNumber}
          >
            {isPending ? "Processing..." : "Mark as Shipped"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
