"use client";

import { useState } from "react";
import {
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, AlertCircle, CheckCircle2 } from "lucide-react";

interface StripePaymentFormProps {
  onSuccess: (paymentIntentId: string) => void;
  onError?: (error: string) => void;
  returnUrl: string;
  submitButtonText?: string;
}

export function StripePaymentForm({
  onSuccess,
  onError,
  returnUrl,
  submitButtonText = "Pay Now",
}: StripePaymentFormProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsLoading(true);
    setMessage(null);

    const { error, paymentIntent } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: returnUrl,
      },
      redirect: "if_required",
    });

    if (error) {
      const errorMessage = error.message || "An unexpected error occurred";
      setMessage(errorMessage);
      onError?.(errorMessage);
    } else if (paymentIntent && paymentIntent.status === "succeeded") {
      setIsSuccess(true);
      setMessage("Payment successful!");
      onSuccess(paymentIntent.id);
    } else if (paymentIntent && paymentIntent.status === "processing") {
      setMessage("Your payment is processing.");
    } else {
      setMessage("Something went wrong.");
    }

    setIsLoading(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement
        options={{
          layout: "tabs",
        }}
      />
      
      {message && (
        <Alert variant={isSuccess ? "default" : "destructive"}>
          {isSuccess ? (
            <CheckCircle2 className="h-4 w-4" />
          ) : (
            <AlertCircle className="h-4 w-4" />
          )}
          <AlertDescription>{message}</AlertDescription>
        </Alert>
      )}

      <Button
        type="submit"
        disabled={!stripe || isLoading || isSuccess}
        className="w-full"
        size="lg"
      >
        {isLoading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Processing...
          </>
        ) : (
          submitButtonText
        )}
      </Button>
    </form>
  );
}
