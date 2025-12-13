"use client";

import { useEffect, useState } from "react";
import { useParams, useSearchParams, useRouter } from "next/navigation";
import Link from "next/link";
import { CheckCircle2, Loader2, AlertCircle, ArrowRight, Home, Package } from "lucide-react";

import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { stripeService } from "@/services/stripe.service";
import { ROUTES } from "@/constants/routes";

export default function PaymentSuccessPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const router = useRouter();
  const auctionId = params?.auctionId as string;
  const paymentIntentId = searchParams?.get("payment_intent");

  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [message, setMessage] = useState("");

  useEffect(() => {
    const verifyPayment = async () => {
      if (!paymentIntentId) {
        setStatus("error");
        setMessage("Payment information not found");
        return;
      }

      try {
        const paymentIntent = await stripeService.getPaymentIntentStatus(paymentIntentId);
        
        if (paymentIntent.status === "succeeded") {
          setStatus("success");
          setMessage("Your payment was successful!");
        } else if (paymentIntent.status === "processing") {
          setStatus("loading");
          setMessage("Your payment is being processed...");
        } else {
          setStatus("error");
          setMessage("Payment was not completed. Please try again.");
        }
      } catch {
        setStatus("error");
        setMessage("Unable to verify payment status");
      }
    };

    verifyPayment();
  }, [paymentIntentId]);

  return (
    <MainLayout>
      <div className="max-w-lg mx-auto py-16">
        <Card>
          <CardHeader className="text-center">
            {status === "loading" && (
              <>
                <div className="mx-auto mb-4">
                  <Loader2 className="h-16 w-16 text-amber-500 animate-spin" />
                </div>
                <CardTitle>Processing Payment</CardTitle>
                <CardDescription>{message}</CardDescription>
              </>
            )}
            
            {status === "success" && (
              <>
                <div className="mx-auto mb-4 bg-green-100 dark:bg-green-900/30 rounded-full p-4">
                  <CheckCircle2 className="h-16 w-16 text-green-500" />
                </div>
                <CardTitle className="text-2xl">Payment Successful!</CardTitle>
                <CardDescription className="text-base">
                  Thank you for your purchase. Your order has been confirmed.
                </CardDescription>
              </>
            )}
            
            {status === "error" && (
              <>
                <div className="mx-auto mb-4 bg-red-100 dark:bg-red-900/30 rounded-full p-4">
                  <AlertCircle className="h-16 w-16 text-red-500" />
                </div>
                <CardTitle className="text-2xl">Payment Issue</CardTitle>
                <CardDescription className="text-base">
                  {message}
                </CardDescription>
              </>
            )}
          </CardHeader>
          
          <CardContent className="space-y-4">
            {status === "success" && (
              <>
                <div className="bg-zinc-50 dark:bg-zinc-900 rounded-lg p-4 space-y-2">
                  <div className="flex justify-between text-sm">
                    <span className="text-zinc-500">Payment ID</span>
                    <span className="font-mono text-xs">{paymentIntentId}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-zinc-500">Auction ID</span>
                    <span className="font-mono text-xs">{auctionId}</span>
                  </div>
                </div>

                <p className="text-sm text-zinc-600 dark:text-zinc-400 text-center">
                  A confirmation email has been sent to your registered email address.
                  The seller will be notified and will ship your item soon.
                </p>

                <div className="flex flex-col gap-2 pt-4">
                  <Button asChild className="w-full bg-amber-500 hover:bg-amber-600">
                    <Link href={ROUTES.DASHBOARD.ORDERS}>
                      <Package className="h-4 w-4 mr-2" />
                      View My Orders
                    </Link>
                  </Button>
                  <Button asChild variant="outline" className="w-full">
                    <Link href={ROUTES.HOME}>
                      <Home className="h-4 w-4 mr-2" />
                      Continue Shopping
                    </Link>
                  </Button>
                </div>
              </>
            )}

            {status === "error" && (
              <div className="flex flex-col gap-2">
                <Button
                  onClick={() => router.push(`/checkout/${auctionId}`)}
                  className="w-full bg-amber-500 hover:bg-amber-600"
                >
                  <ArrowRight className="h-4 w-4 mr-2" />
                  Try Again
                </Button>
                <Button asChild variant="outline" className="w-full">
                  <Link href={ROUTES.DASHBOARD.ROOT}>
                    Go to Dashboard
                  </Link>
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </MainLayout>
  );
}
