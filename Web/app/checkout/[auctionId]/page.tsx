"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import Image from "next/image";
import Link from "next/link";
import {
  Loader2,
  CreditCard,
  Wallet,
  Shield,
  CheckCircle2,
  AlertCircle,
  ArrowLeft,
  Truck,
  Info,
} from "lucide-react";
import { toast } from "sonner";

import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  CardFooter,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/components/ui/alert";
import { Checkbox } from "@/components/ui/checkbox";
import { Textarea } from "@/components/ui/textarea";
import { auctionService } from "@/services/auction.service";
import { walletService } from "@/services/wallet.service";
import { stripeService } from "@/services/stripe.service";
import { StripeCheckout } from "@/features/payment";
import { formatAmountForStripe } from "@/lib/stripe";
import { Auction, AuctionStatus } from "@/types/auction";
import { ROUTES } from "@/constants/routes";
import { formatCurrency } from "@/utils";

const PLATFORM_FEE_PERCENTAGE = 5;
const PAYMENT_METHODS = [
  {
    id: "wallet",
    name: "Wallet Balance",
    description: "Pay using your auction wallet balance",
    icon: Wallet,
  },
  {
    id: "card",
    name: "Credit/Debit Card",
    description: "Pay securely with Stripe",
    icon: CreditCard,
  },
];

interface ShippingAddress {
  fullName: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone: string;
}

export default function CheckoutPage() {
  const params = useParams();
  const router = useRouter();
  const { data: session, status: sessionStatus } = useSession();
  const auctionId = params?.auctionId as string;

  const [auction, setAuction] = useState<Auction | null>(null);
  const [walletBalance, setWalletBalance] = useState<number>(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState("wallet");
  const [agreedToTerms, setAgreedToTerms] = useState(false);
  const [stripeClientSecret, setStripeClientSecret] = useState<string | null>(null);
  const [isCreatingPaymentIntent, setIsCreatingPaymentIntent] = useState(false);
  const [shippingAddress, setShippingAddress] = useState<ShippingAddress>({
    fullName: "",
    addressLine1: "",
    addressLine2: "",
    city: "",
    state: "",
    postalCode: "",
    country: "",
    phone: "",
  });
  const [notes, setNotes] = useState("");

  useEffect(() => {
    if (sessionStatus === "unauthenticated") {
      router.push(ROUTES.AUTH.LOGIN);
      return;
    }

    const fetchData = async () => {
      if (!auctionId) return;

      setIsLoading(true);
      try {
        const [auctionData, balanceData] = await Promise.all([
          auctionService.getAuctionById(auctionId),
          walletService.getBalance(),
        ]);

        if (auctionData.status !== AuctionStatus.Finished) {
          toast.error("This auction is not available for checkout");
          router.push(ROUTES.AUCTIONS.DETAIL(auctionId));
          return;
        }

        if (auctionData.winner !== session?.user?.name) {
          toast.error("You are not the winner of this auction");
          router.push(ROUTES.AUCTIONS.DETAIL(auctionId));
          return;
        }

        setAuction(auctionData);
        setWalletBalance(balanceData.availableBalance);
      } catch {
        toast.error("Failed to load checkout details");
      } finally {
        setIsLoading(false);
      }
    };

    if (sessionStatus === "authenticated") {
      fetchData();
    }
  }, [auctionId, session?.user?.name, sessionStatus, router]);

  const winningBid = auction?.soldAmount || auction?.currentHighBid || 0;
  const platformFee = (winningBid * PLATFORM_FEE_PERCENTAGE) / 100;
  const totalAmount = winningBid + platformFee;
  const hasInsufficientBalance = walletBalance < totalAmount;

  const handleAddressChange = (field: keyof ShippingAddress, value: string) => {
    setShippingAddress((prev) => ({ ...prev, [field]: value }));
  };

  const isAddressComplete = () => {
    return (
      shippingAddress.fullName &&
      shippingAddress.addressLine1 &&
      shippingAddress.city &&
      shippingAddress.state &&
      shippingAddress.postalCode &&
      shippingAddress.country &&
      shippingAddress.phone
    );
  };

  const handlePaymentMethodChange = async (method: string) => {
    setPaymentMethod(method);
    
    if (method === "card" && !stripeClientSecret && auction && session?.user) {
      setIsCreatingPaymentIntent(true);
      try {
        const response = await stripeService.createPaymentIntent({
          amountInCents: formatAmountForStripe(totalAmount),
          currency: "usd",
          customerEmail: session.user.email || "",
          customerName: session.user.name || "",
          auctionId: auction.id,
          buyerId: session.user.id || "",
        });
        setStripeClientSecret(response.clientSecret);
      } catch {
        toast.error("Failed to initialize card payment");
        setPaymentMethod("wallet");
      } finally {
        setIsCreatingPaymentIntent(false);
      }
    }
  };

  const handleStripeSuccess = (paymentIntentId: string) => {
    toast.success("Payment processed successfully!", {
      description: `Payment ID: ${paymentIntentId}`,
    });
    router.push(ROUTES.DASHBOARD.ORDERS);
  };

  const handleSubmit = async () => {
    if (!agreedToTerms) {
      toast.error("Please agree to the terms and conditions");
      return;
    }

    if (!isAddressComplete()) {
      toast.error("Please fill in all required shipping address fields");
      return;
    }

    if (paymentMethod === "wallet" && hasInsufficientBalance) {
      toast.error("Insufficient wallet balance");
      return;
    }

    setIsProcessing(true);
    try {
      toast.success("Payment processed successfully!", {
        description: "Your order has been created. The seller will be notified.",
      });
      router.push(ROUTES.DASHBOARD.ORDERS);
    } catch {
      toast.error("Payment failed", {
        description: "Please try again or contact support.",
      });
    } finally {
      setIsProcessing(false);
    }
  };

  if (isLoading || sessionStatus === "loading") {
    return (
      <MainLayout>
        <div className="flex items-center justify-center py-20">
          <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
        </div>
      </MainLayout>
    );
  }

  if (!auction) {
    return (
      <MainLayout>
        <div className="flex flex-col items-center justify-center py-20">
          <AlertCircle className="h-16 w-16 text-zinc-400 mb-4" />
          <h2 className="text-xl font-semibold">Auction not found</h2>
          <Button asChild className="mt-4">
            <Link href={ROUTES.DASHBOARD.ROOT}>Go to Dashboard</Link>
          </Button>
        </div>
      </MainLayout>
    );
  }

  const primaryImage = auction.files?.find((f) => f.isPrimary)?.url || auction.files?.[0]?.url;

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto space-y-6">
        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink href={ROUTES.HOME}>Home</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbLink href={ROUTES.DASHBOARD.ROOT}>Dashboard</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>Checkout</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Complete Your Purchase</h1>
            <p className="text-zinc-600 dark:text-zinc-400">
              Congratulations on winning the auction!
            </p>
          </div>
        </div>

        <div className="grid lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Truck className="h-5 w-5 text-amber-500" />
                  Shipping Address
                </CardTitle>
                <CardDescription>
                  Where should we deliver your item?
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid sm:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="fullName">Full Name *</Label>
                    <Input
                      id="fullName"
                      placeholder="John Doe"
                      value={shippingAddress.fullName}
                      onChange={(e) => handleAddressChange("fullName", e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="phone">Phone Number *</Label>
                    <Input
                      id="phone"
                      placeholder="+1 (555) 000-0000"
                      value={shippingAddress.phone}
                      onChange={(e) => handleAddressChange("phone", e.target.value)}
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="addressLine1">Address Line 1 *</Label>
                  <Input
                    id="addressLine1"
                    placeholder="Street address"
                    value={shippingAddress.addressLine1}
                    onChange={(e) => handleAddressChange("addressLine1", e.target.value)}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="addressLine2">Address Line 2</Label>
                  <Input
                    id="addressLine2"
                    placeholder="Apartment, suite, etc. (optional)"
                    value={shippingAddress.addressLine2}
                    onChange={(e) => handleAddressChange("addressLine2", e.target.value)}
                  />
                </div>

                <div className="grid sm:grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="city">City *</Label>
                    <Input
                      id="city"
                      placeholder="City"
                      value={shippingAddress.city}
                      onChange={(e) => handleAddressChange("city", e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="state">State/Province *</Label>
                    <Input
                      id="state"
                      placeholder="State"
                      value={shippingAddress.state}
                      onChange={(e) => handleAddressChange("state", e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="postalCode">Postal Code *</Label>
                    <Input
                      id="postalCode"
                      placeholder="12345"
                      value={shippingAddress.postalCode}
                      onChange={(e) => handleAddressChange("postalCode", e.target.value)}
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="country">Country *</Label>
                  <Input
                    id="country"
                    placeholder="United States"
                    value={shippingAddress.country}
                    onChange={(e) => handleAddressChange("country", e.target.value)}
                  />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <CreditCard className="h-5 w-5 text-amber-500" />
                  Payment Method
                </CardTitle>
                <CardDescription>
                  Select how you would like to pay
                </CardDescription>
              </CardHeader>
              <CardContent>
                <RadioGroup value={paymentMethod} onValueChange={handlePaymentMethodChange}>
                  {PAYMENT_METHODS.map((method) => {
                    const Icon = method.icon;
                    const isDisabled = method.id === "wallet" && hasInsufficientBalance;

                    return (
                      <div
                        key={method.id}
                        className={`flex items-center space-x-4 p-4 border rounded-lg transition-colors ${
                          paymentMethod === method.id && !isDisabled
                            ? "border-amber-500 bg-amber-50 dark:bg-amber-950/30"
                            : "border-zinc-200 dark:border-zinc-800"
                        } ${isDisabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
                        onClick={() => !isDisabled && handlePaymentMethodChange(method.id)}
                      >
                        <RadioGroupItem
                          value={method.id}
                          id={method.id}
                          disabled={isDisabled}
                        />
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <Icon className="h-5 w-5 text-zinc-600 dark:text-zinc-400" />
                            <Label
                              htmlFor={method.id}
                              className={`font-medium ${isDisabled ? "" : "cursor-pointer"}`}
                            >
                              {method.name}
                            </Label>
                            {method.id === "card" && (
                              <Badge variant="outline" className="text-xs">
                                Powered by Stripe
                              </Badge>
                            )}
                          </div>
                          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
                            {method.description}
                          </p>
                          {method.id === "wallet" && (
                            <p className="text-sm mt-1">
                              <span className="text-zinc-500">Balance: </span>
                              <span className={hasInsufficientBalance ? "text-red-500 font-medium" : "text-green-500 font-medium"}>
                                {formatCurrency(walletBalance)}
                              </span>
                              {hasInsufficientBalance && (
                                <span className="text-red-500 ml-2">
                                  (Insufficient funds)
                                </span>
                              )}
                            </p>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </RadioGroup>

                {paymentMethod === "wallet" && hasInsufficientBalance && (
                  <Alert variant="destructive" className="mt-4">
                    <AlertCircle className="h-4 w-4" />
                    <AlertTitle>Insufficient Balance</AlertTitle>
                    <AlertDescription>
                      You need {formatCurrency(totalAmount - walletBalance)} more in your wallet.{" "}
                      <Link
                        href={ROUTES.DASHBOARD.WALLET}
                        className="font-medium underline"
                      >
                        Add funds to wallet
                      </Link>
                    </AlertDescription>
                  </Alert>
                )}

                {paymentMethod === "card" && isCreatingPaymentIntent && (
                  <div className="mt-4 flex items-center justify-center p-8">
                    <Loader2 className="h-6 w-6 animate-spin text-amber-500 mr-2" />
                    <span className="text-zinc-600">Initializing payment...</span>
                  </div>
                )}

                {paymentMethod === "card" && stripeClientSecret && !isCreatingPaymentIntent && (
                  <div className="mt-4">
                    <StripeCheckout
                      clientSecret={stripeClientSecret}
                      amount={totalAmount}
                      productName={auction?.title || "Auction Item"}
                      onSuccess={handleStripeSuccess}
                      returnUrl={`${typeof window !== "undefined" ? window.location.origin : ""}/checkout/${auctionId}/success`}
                    />
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Additional Notes</CardTitle>
                <CardDescription>
                  Any special instructions for the seller (optional)
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Textarea
                  placeholder="Add any notes or special instructions..."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={3}
                />
              </CardContent>
            </Card>
          </div>

          <div className="space-y-6">
            <Card className="sticky top-6">
              <CardHeader>
                <CardTitle>Order Summary</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex gap-4">
                  <div className="relative w-20 h-20 rounded-lg overflow-hidden bg-zinc-100 dark:bg-zinc-800 flex-shrink-0">
                    {primaryImage ? (
                      <Image
                        src={primaryImage}
                        alt={auction.title}
                        fill
                        className="object-cover"
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-zinc-400">
                        No Image
                      </div>
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="font-medium text-zinc-900 dark:text-white line-clamp-2">
                      {auction.title}
                    </h3>
                    <p className="text-sm text-zinc-500 mt-1">
                      {auction.year} {auction.make} {auction.model}
                    </p>
                  </div>
                </div>

                <Separator />

                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-zinc-600 dark:text-zinc-400">
                      Winning Bid
                    </span>
                    <span className="font-medium">{formatCurrency(winningBid)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-zinc-600 dark:text-zinc-400">
                      Platform Fee ({PLATFORM_FEE_PERCENTAGE}%)
                    </span>
                    <span className="font-medium">{formatCurrency(platformFee)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-zinc-600 dark:text-zinc-400">
                      Shipping
                    </span>
                    <span className="text-green-500 font-medium">
                      Arranged with seller
                    </span>
                  </div>
                </div>

                <Separator />

                <div className="flex justify-between text-lg font-bold">
                  <span>Total</span>
                  <span className="text-amber-500">{formatCurrency(totalAmount)}</span>
                </div>

                <div className="flex items-start space-x-2 pt-2">
                  <Checkbox
                    id="terms"
                    checked={agreedToTerms}
                    onCheckedChange={(checked) => setAgreedToTerms(checked as boolean)}
                  />
                  <label htmlFor="terms" className="text-sm text-zinc-600 dark:text-zinc-400 leading-relaxed">
                    I agree to the{" "}
                    <Link href="/terms" className="text-amber-500 hover:underline">
                      Terms of Service
                    </Link>{" "}
                    and{" "}
                    <Link href="/privacy" className="text-amber-500 hover:underline">
                      Privacy Policy
                    </Link>
                  </label>
                </div>
              </CardContent>
              <CardFooter className="flex-col gap-3">
                {paymentMethod === "wallet" && (
                  <Button
                    className="w-full bg-amber-500 hover:bg-amber-600"
                    size="lg"
                    onClick={handleSubmit}
                    disabled={
                      isProcessing ||
                      !agreedToTerms ||
                      !isAddressComplete() ||
                      hasInsufficientBalance
                    }
                  >
                    {isProcessing ? (
                      <>
                        <Loader2 className="h-4 w-4 animate-spin mr-2" />
                        Processing...
                      </>
                    ) : (
                      <>
                        <CheckCircle2 className="h-4 w-4 mr-2" />
                        Pay with Wallet
                      </>
                    )}
                  </Button>
                )}

                {paymentMethod === "card" && !stripeClientSecret && (
                  <p className="text-sm text-zinc-500 text-center">
                    Select card payment above to proceed
                  </p>
                )}

                <div className="flex items-center justify-center gap-2 text-xs text-zinc-500">
                  <Shield className="h-4 w-4" />
                  Secure payment processing
                </div>
              </CardFooter>
            </Card>

            <Alert>
              <Info className="h-4 w-4" />
              <AlertTitle>Buyer Protection</AlertTitle>
              <AlertDescription className="text-sm">
                Your payment is held securely until you confirm receipt of the
                item in the described condition.
              </AlertDescription>
            </Alert>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
