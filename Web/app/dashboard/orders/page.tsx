"use client";

import { Suspense, useEffect, useState, useCallback } from "react";
import { useSession } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { MainLayout } from "@/components/layout/main-layout";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import {
    Package,
    ShoppingBag,
    Truck,
    CheckCircle,
    Clock,
    CreditCard,
    Star,
    ExternalLink,
    Copy,
} from "lucide-react";
import { ROUTES } from "@/constants/routes";
import {
    orderService,
    Order,
    OrderStatus,
    PaymentStatus,
    getStatusColor,
    getStatusLabel,
    getPaymentStatusColor,
    getPaymentStatusLabel,
} from "@/services/order.service";
import { formatCurrency, formatRelativeTime } from "@/utils";
import { toast } from "sonner";

export default function OrdersPage() {
    return (
        <Suspense fallback={<OrdersPageSkeleton />}>
            <OrdersPageContent />
        </Suspense>
    );
}

function OrdersPageSkeleton() {
    return (
        <MainLayout>
            <div className="flex items-center justify-center min-h-[400px]">
                <LoadingSpinner size="lg" />
            </div>
        </MainLayout>
    );
}

function OrdersPageContent() {
    const { data: session, status } = useSession();
    const router = useRouter();
    const searchParams = useSearchParams();
    const initialTab = searchParams?.get("tab") || "purchases";

    const [activeTab, setActiveTab] = useState(initialTab);
    const [purchases, setPurchases] = useState<Order[]>([]);
    const [sales, setSales] = useState<Order[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
    const [shippingDialogOpen, setShippingDialogOpen] = useState(false);
    const [trackingNumber, setTrackingNumber] = useState("");
    const [carrier, setCarrier] = useState("");

    const fetchOrders = useCallback(async () => {
        try {
            const [purchasesData, salesData] = await Promise.all([
                orderService.getMyPurchases(),
                orderService.getMySales(),
            ]);
            setPurchases(purchasesData);
            setSales(salesData);
        } catch (error) {
            console.error("Failed to fetch orders:", error);
            toast.error("Failed to load orders");
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push(ROUTES.AUTH.LOGIN);
            return;
        }
        if (status === "authenticated") {
            fetchOrders();
        }
    }, [status, router, fetchOrders]);

    const handleMarkAsShipped = async () => {
        if (!selectedOrder || !trackingNumber) return;

        try {
            await orderService.updateShipping(selectedOrder.id, {
                trackingNumber,
                carrier,
            });
            toast.success("Order marked as shipped");
            setShippingDialogOpen(false);
            setTrackingNumber("");
            setCarrier("");
            fetchOrders();
        } catch (error) {
            console.error("Failed to update shipping:", error);
            toast.error("Failed to update shipping");
        }
    };

    const handleMarkAsDelivered = async (orderId: string) => {
        try {
            await orderService.confirmDelivery(orderId);
            toast.success("Order confirmed as delivered");
            fetchOrders();
        } catch (error) {
            console.error("Failed to confirm delivery:", error);
            toast.error("Failed to confirm delivery");
        }
    };

    const copyTrackingNumber = (trackingNumber: string) => {
        navigator.clipboard.writeText(trackingNumber);
        toast.success("Tracking number copied");
    };

    if (status === "loading" || isLoading) {
        return (
            <MainLayout>
                <div className="flex items-center justify-center min-h-[400px]">
                    <LoadingSpinner size="lg" />
                </div>
            </MainLayout>
        );
    }

    const OrderCard = ({ order, isSeller }: { order: Order; isSeller: boolean }) => (
        <Card className="overflow-hidden">
            <div className="flex">
                <div className="relative w-32 h-32 bg-zinc-100 dark:bg-zinc-900 shrink-0">
                    {order.itemImageUrl ? (
                        <Image
                            src={order.itemImageUrl}
                            alt={order.itemTitle}
                            fill
                            className="object-cover"
                        />
                    ) : (
                        <div className="flex items-center justify-center h-full">
                            <Package className="h-10 w-10 text-muted-foreground" />
                        </div>
                    )}
                </div>
                <div className="flex-1 p-4">
                    <div className="flex items-start justify-between">
                        <div>
                            <Link
                                href={`${ROUTES.AUCTIONS.LIST}/${order.auctionId}`}
                                className="font-medium hover:underline line-clamp-1"
                            >
                                {order.itemTitle}
                            </Link>
                            <p className="text-sm text-muted-foreground mt-1">
                                {isSeller ? `Buyer: ${order.buyerUsername}` : `Seller: ${order.sellerUsername}`}
                            </p>
                        </div>
                        <div className="flex items-center gap-2">
                            <Badge className={getStatusColor(order.status)}>
                                {getStatusLabel(order.status)}
                            </Badge>
                            <Badge className={getPaymentStatusColor(order.paymentStatus)}>
                                {getPaymentStatusLabel(order.paymentStatus)}
                            </Badge>
                        </div>
                    </div>

                    <div className="flex items-center justify-between mt-4">
                        <div className="space-y-1">
                            <p className="text-lg font-bold">
                                {formatCurrency(order.totalAmount)}
                            </p>
                            <p className="text-xs text-muted-foreground">
                                Winning bid: {formatCurrency(order.winningBid)}
                            </p>
                        </div>
                        <div className="text-right text-sm text-muted-foreground">
                            <p>Order #{order.id.slice(0, 8)}</p>
                            <p>{formatRelativeTime(order.createdAt)}</p>
                        </div>
                    </div>

                    {order.trackingNumber && (
                        <div className="mt-3 p-2 bg-zinc-50 dark:bg-zinc-900 rounded-lg flex items-center justify-between">
                            <div className="flex items-center gap-2">
                                <Truck className="h-4 w-4 text-muted-foreground" />
                                <span className="text-sm">
                                    {order.carrier && `${order.carrier}: `}
                                    {order.trackingNumber}
                                </span>
                            </div>
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => copyTrackingNumber(order.trackingNumber!)}
                            >
                                <Copy className="h-4 w-4" />
                            </Button>
                        </div>
                    )}

                    <div className="flex items-center gap-2 mt-4">
                        {isSeller && order.status === OrderStatus.PendingPayment && (
                            <p className="text-sm text-amber-600">Waiting for buyer payment</p>
                        )}
                        {isSeller && order.status === OrderStatus.Paid && (
                            <Button
                                size="sm"
                                onClick={() => {
                                    setSelectedOrder(order);
                                    setShippingDialogOpen(true);
                                }}
                            >
                                <Truck className="h-4 w-4 mr-2" />
                                Mark as Shipped
                            </Button>
                        )}
                        {!isSeller && order.status === OrderStatus.Shipped && (
                            <Button
                                size="sm"
                                onClick={() => handleMarkAsDelivered(order.id)}
                            >
                                <CheckCircle className="h-4 w-4 mr-2" />
                                Confirm Delivery
                            </Button>
                        )}
                        {order.status === OrderStatus.Completed && !order.hasReview && (
                            <Link href={`${ROUTES.DASHBOARD.ROOT}/orders/${order.id}/review`}>
                                <Button size="sm" variant="outline">
                                    <Star className="h-4 w-4 mr-2" />
                                    Leave Review
                                </Button>
                            </Link>
                        )}
                        <Link href={`${ROUTES.AUCTIONS.LIST}/${order.auctionId}`}>
                            <Button size="sm" variant="ghost">
                                <ExternalLink className="h-4 w-4 mr-2" />
                                View Auction
                            </Button>
                        </Link>
                    </div>
                </div>
            </div>
        </Card>
    );

    const EmptyState = ({ type }: { type: "purchases" | "sales" }) => (
        <Card>
            <CardContent className="py-12 text-center">
                {type === "purchases" ? (
                    <>
                        <ShoppingBag className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                        <h3 className="text-lg font-semibold mb-2">No purchases yet</h3>
                        <p className="text-muted-foreground mb-4">
                            Win auctions to see your purchases here
                        </p>
                        <Link href={ROUTES.AUCTIONS.LIST}>
                            <Button>Browse Auctions</Button>
                        </Link>
                    </>
                ) : (
                    <>
                        <Package className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                        <h3 className="text-lg font-semibold mb-2">No sales yet</h3>
                        <p className="text-muted-foreground mb-4">
                            List items for auction to start selling
                        </p>
                        <Link href={ROUTES.DASHBOARD.CREATE_LISTING}>
                            <Button>Create Listing</Button>
                        </Link>
                    </>
                )}
            </CardContent>
        </Card>
    );

    return (
        <MainLayout>
            <div className="space-y-6">
                <Breadcrumb>
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href={ROUTES.DASHBOARD.ROOT}>Dashboard</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Orders</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div>
                    <h1 className="text-3xl font-bold">Orders</h1>
                    <p className="text-muted-foreground mt-1">
                        Manage your purchases and sales
                    </p>
                </div>

                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <Card>
                        <CardContent className="pt-4">
                            <div className="flex items-center gap-3">
                                <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
                                    <ShoppingBag className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                                </div>
                                <div>
                                    <p className="text-2xl font-bold">{purchases.length}</p>
                                    <p className="text-sm text-muted-foreground">Purchases</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent className="pt-4">
                            <div className="flex items-center gap-3">
                                <div className="p-2 bg-green-100 dark:bg-green-900 rounded-lg">
                                    <Package className="h-5 w-5 text-green-600 dark:text-green-400" />
                                </div>
                                <div>
                                    <p className="text-2xl font-bold">{sales.length}</p>
                                    <p className="text-sm text-muted-foreground">Sales</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent className="pt-4">
                            <div className="flex items-center gap-3">
                                <div className="p-2 bg-amber-100 dark:bg-amber-900 rounded-lg">
                                    <Clock className="h-5 w-5 text-amber-600 dark:text-amber-400" />
                                </div>
                                <div>
                                    <p className="text-2xl font-bold">
                                        {purchases.filter(o => o.status === OrderStatus.Shipped).length}
                                    </p>
                                    <p className="text-sm text-muted-foreground">In Transit</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent className="pt-4">
                            <div className="flex items-center gap-3">
                                <div className="p-2 bg-purple-100 dark:bg-purple-900 rounded-lg">
                                    <CreditCard className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                                </div>
                                <div>
                                    <p className="text-2xl font-bold">
                                        {formatCurrency(
                                            sales
                                                .filter(o => o.paymentStatus === PaymentStatus.Completed)
                                                .reduce((sum, o) => sum + o.totalAmount, 0)
                                        )}
                                    </p>
                                    <p className="text-sm text-muted-foreground">Total Earned</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                <Tabs value={activeTab} onValueChange={setActiveTab}>
                    <TabsList>
                        <TabsTrigger value="purchases" className="flex items-center gap-2">
                            <ShoppingBag className="h-4 w-4" />
                            Purchases ({purchases.length})
                        </TabsTrigger>
                        <TabsTrigger value="sales" className="flex items-center gap-2">
                            <Package className="h-4 w-4" />
                            Sales ({sales.length})
                        </TabsTrigger>
                    </TabsList>

                    <TabsContent value="purchases" className="mt-4 space-y-4">
                        {purchases.length === 0 ? (
                            <EmptyState type="purchases" />
                        ) : (
                            purchases.map((order) => (
                                <OrderCard key={order.id} order={order} isSeller={false} />
                            ))
                        )}
                    </TabsContent>

                    <TabsContent value="sales" className="mt-4 space-y-4">
                        {sales.length === 0 ? (
                            <EmptyState type="sales" />
                        ) : (
                            sales.map((order) => (
                                <OrderCard key={order.id} order={order} isSeller={true} />
                            ))
                        )}
                    </TabsContent>
                </Tabs>
            </div>

            <Dialog open={shippingDialogOpen} onOpenChange={setShippingDialogOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Add Shipping Information</DialogTitle>
                        <DialogDescription>
                            Enter the tracking details for this order
                        </DialogDescription>
                    </DialogHeader>
                    <div className="space-y-4">
                        <div>
                            <label className="text-sm font-medium">Tracking Number *</label>
                            <Input
                                value={trackingNumber}
                                onChange={(e) => setTrackingNumber(e.target.value)}
                                placeholder="Enter tracking number"
                            />
                        </div>
                        <div>
                            <label className="text-sm font-medium">Carrier (Optional)</label>
                            <Input
                                value={carrier}
                                onChange={(e) => setCarrier(e.target.value)}
                                placeholder="e.g., UPS, FedEx, USPS"
                            />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="outline" onClick={() => setShippingDialogOpen(false)}>
                            Cancel
                        </Button>
                        <Button onClick={handleMarkAsShipped} disabled={!trackingNumber}>
                            Mark as Shipped
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </MainLayout>
    );
}
