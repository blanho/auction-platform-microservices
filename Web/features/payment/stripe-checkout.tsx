"use client";

import { useEffect, useState } from "react";
import { Elements } from "@stripe/react-stripe-js";
import { StripeElementsOptions } from "@stripe/stripe-js";
import { getStripe } from "@/lib/stripe";
import { StripePaymentForm } from "./stripe-payment-form";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleExclamation, faCreditCard } from "@fortawesome/free-solid-svg-icons";

interface StripeCheckoutProps {
  clientSecret: string;
  amount: number;
  productName: string;
  onSuccess: (paymentIntentId: string) => void;
  onError?: (error: string) => void;
  returnUrl: string;
}

export function StripeCheckout({
  clientSecret,
  amount,
  productName,
  onSuccess,
  onError,
  returnUrl,
}: StripeCheckoutProps) {
  const [stripePromise] = useState(() => getStripe());
  const [isReady, setIsReady] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (clientSecret) {
      setIsReady(true);
    }
  }, [clientSecret]);

  if (error) {
    return (
      <Alert variant="destructive">
        <FontAwesomeIcon icon={faCircleExclamation} className="h-4 w-4" />
        <AlertTitle>Payment Error</AlertTitle>
        <AlertDescription>{error}</AlertDescription>
      </Alert>
    );
  }

  if (!stripePromise) {
    return (
      <Alert variant="destructive">
        <FontAwesomeIcon icon={faCircleExclamation} className="h-4 w-4" />
        <AlertTitle>Configuration Error</AlertTitle>
        <AlertDescription>
          Stripe is not configured. Please contact support.
        </AlertDescription>
      </Alert>
    );
  }

  if (!isReady) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-32" />
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  const options: StripeElementsOptions = {
    clientSecret,
    appearance: {
      theme: "stripe",
      variables: {
        colorPrimary: "#0f172a",
        colorBackground: "#ffffff",
        colorText: "#1e293b",
        colorDanger: "#ef4444",
        fontFamily: "Inter, system-ui, sans-serif",
        borderRadius: "8px",
      },
    },
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <FontAwesomeIcon icon={faCreditCard} className="h-5 w-5" />
          <CardTitle>Pay with Card</CardTitle>
        </div>
        <CardDescription>
          {productName} - ${amount.toFixed(2)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Elements stripe={stripePromise} options={options}>
          <StripePaymentForm
            onSuccess={onSuccess}
            onError={(err) => {
              setError(err);
              onError?.(err);
            }}
            returnUrl={returnUrl}
            submitButtonText={`Pay $${amount.toFixed(2)}`}
          />
        </Elements>
      </CardContent>
    </Card>
  );
}
