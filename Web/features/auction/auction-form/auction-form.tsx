"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faArrowLeft,
  faArrowRight,
  faCheck,
  faSpinner,
} from "@fortawesome/free-solid-svg-icons";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { Card, CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { DraftRestorePrompt } from "@/components/ui/draft-restore-prompt";
import { UploadedImage } from "@/components/ui/cloudinary-upload";
import { useFormDraft } from "@/hooks/use-form-draft";
import { showErrorToast, showSuccessToast } from "@/utils";
import { ROUTES } from "@/constants";
import { ShippingType } from "@/types/auction";
import { cn } from "@/lib/utils";

import {
  auctionFormSchema,
  AuctionFormValues,
  AuctionFormProps,
  FORM_STEPS,
  DEFAULT_FORM_VALUES,
} from "./types";
import { DetailsStep } from "./details-step";
import { PricingStep } from "./pricing-step";
import { ShippingStep } from "./shipping-step";
import { ImagesStep } from "./images-step";
import { ReviewStep } from "./review-step";

const STEP_VALIDATION_FIELDS: Record<string, (keyof AuctionFormValues)[]> = {
  details: ["title", "description"],
  pricing: ["reservePrice", "auctionEnd"],
  shipping: [],
  images: [],
  review: [],
};

export function AuctionForm({
  mode,
  initialData,
  categories,
  onSubmit,
}: AuctionFormProps) {
  const router = useRouter();
  const [currentStepIndex, setCurrentStepIndex] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [uploadedImages, setUploadedImages] = useState<UploadedImage[]>([]);

  const currentStep = FORM_STEPS[currentStepIndex];
  const isFirstStep = currentStepIndex === 0;
  const isLastStep = currentStepIndex === FORM_STEPS.length - 1;
  const progress = ((currentStepIndex + 1) / FORM_STEPS.length) * 100;

  const defaultValues: AuctionFormValues = initialData
    ? {
        title: initialData.title,
        description: initialData.description,
        categoryId: initialData.categoryId || "",
        condition: initialData.condition || "Used",
        yearManufactured: initialData.yearManufactured || new Date().getFullYear(),
        reservePrice: initialData.reservePrice,
        buyNowPrice: initialData.buyNowPrice,
        auctionEnd: initialData.auctionEnd
          ? new Date(initialData.auctionEnd).toISOString().slice(0, 16)
          : "",
        isFeatured: initialData.isFeatured || false,
        imageUrl: "",
        shippingType: initialData.shippingType || ShippingType.Flat,
        shippingCost: initialData.shippingCost || 0,
        handlingTime: initialData.handlingTime || 3,
        shipsFrom: initialData.shipsFrom || "",
        localPickupAvailable: initialData.localPickupAvailable || false,
        localPickupAddress: initialData.localPickupAddress || "",
      }
    : DEFAULT_FORM_VALUES;

  const form = useForm<AuctionFormValues>({
    resolver: zodResolver(auctionFormSchema),
    defaultValues,
    mode: "onChange",
  });

  const draftKey = mode === "create" ? "create-auction" : "";
  const {
    hasDraft,
    draftDate,
    restoreDraft,
    clearDraft,
    discardDraft,
    isRestorePromptOpen,
  } = useFormDraft({
    form,
    key: draftKey,
    exclude: ["imageUrl"],
    onDraftRestored: () => {
      showSuccessToast("Draft restored successfully");
    },
  });

  const validateCurrentStep = async (): Promise<boolean> => {
    const fieldsToValidate = STEP_VALIDATION_FIELDS[currentStep.id];
    if (fieldsToValidate.length === 0) return true;

    const result = await form.trigger(fieldsToValidate);
    return result;
  };

  const goToNextStep = async () => {
    const isValid = await validateCurrentStep();
    if (!isValid) return;

    if (currentStepIndex < FORM_STEPS.length - 1) {
      setCurrentStepIndex(currentStepIndex + 1);
    }
  };

  const goToPreviousStep = () => {
    if (currentStepIndex > 0) {
      setCurrentStepIndex(currentStepIndex - 1);
    }
  };

  const goToStep = (index: number) => {
    if (index <= currentStepIndex) {
      setCurrentStepIndex(index);
    }
  };

  const handleSubmit = async (values: AuctionFormValues) => {
    if (uploadedImages.some((img) => img.status === "uploading")) {
      toast.error("Please wait for images to finish uploading");
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit(values, uploadedImages);
      if (draftKey) clearDraft();
      showSuccessToast(
        mode === "create"
          ? "Auction created successfully!"
          : "Auction updated successfully!"
      );
      router.push(ROUTES.AUCTIONS.LIST);
    } catch (error) {
      showErrorToast(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const stepProps = {
    form,
    categories,
    uploadedImages,
    onImagesChange: setUploadedImages,
    mode,
  };

  const renderStep = () => {
    switch (currentStep.id) {
      case "details":
        return <DetailsStep {...stepProps} />;
      case "pricing":
        return <PricingStep {...stepProps} />;
      case "shipping":
        return <ShippingStep {...stepProps} />;
      case "images":
        return <ImagesStep {...stepProps} />;
      case "review":
        return <ReviewStep {...stepProps} />;
      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      {mode === "create" && (
        <DraftRestorePrompt
          isOpen={isRestorePromptOpen && hasDraft}
          draftDate={draftDate}
          onRestore={restoreDraft}
          onDiscard={discardDraft}
        />
      )}

      <Card>
        <CardContent className="pt-6">
          <div className="mb-8">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-slate-600 dark:text-slate-400">
                Step {currentStepIndex + 1} of {FORM_STEPS.length}
              </span>
              <span className="text-sm font-medium text-slate-600 dark:text-slate-400">
                {Math.round(progress)}%
              </span>
            </div>
            <Progress value={progress} className="h-2" />
          </div>

          <div className="flex items-center justify-center gap-2 mb-8 overflow-x-auto pb-2">
            {FORM_STEPS.map((step, index) => {
              const isActive = index === currentStepIndex;
              const isCompleted = index < currentStepIndex;
              const isClickable = index <= currentStepIndex;

              return (
                <button
                  key={step.id}
                  type="button"
                  onClick={() => isClickable && goToStep(index)}
                  disabled={!isClickable}
                  className={cn(
                    "flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors whitespace-nowrap",
                    isActive &&
                      "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
                    isCompleted &&
                      !isActive &&
                      "text-green-600 dark:text-green-400 hover:bg-green-50 dark:hover:bg-green-900/20",
                    !isActive &&
                      !isCompleted &&
                      "text-slate-400 dark:text-slate-600",
                    isClickable && "cursor-pointer"
                  )}
                >
                  <span
                    className={cn(
                      "flex h-6 w-6 items-center justify-center rounded-full text-xs",
                      isActive && "bg-purple-600 text-white",
                      isCompleted && !isActive && "bg-green-500 text-white",
                      !isActive && !isCompleted && "bg-slate-200 dark:bg-slate-700"
                    )}
                  >
                    {isCompleted && !isActive ? (
                      <FontAwesomeIcon icon={faCheck} className="h-3 w-3" />
                    ) : (
                      index + 1
                    )}
                  </span>
                  <span className="hidden sm:inline">{step.label}</span>
                </button>
              );
            })}
          </div>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(handleSubmit)}>
              <div className="min-h-[400px]">{renderStep()}</div>

              <div className="flex items-center justify-between mt-8 pt-6 border-t">
                <Button
                  type="button"
                  variant="outline"
                  onClick={goToPreviousStep}
                  disabled={isFirstStep}
                  className={cn(isFirstStep && "invisible")}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="mr-2 h-4 w-4" />
                  Previous
                </Button>

                {isLastStep ? (
                  <Button
                    type="submit"
                    disabled={isSubmitting}
                    className="bg-linear-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
                  >
                    {isSubmitting ? (
                      <>
                        <FontAwesomeIcon
                          icon={faSpinner}
                          className="mr-2 h-4 w-4 animate-spin"
                        />
                        {mode === "create" ? "Creating..." : "Saving..."}
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={faCheck} className="mr-2 h-4 w-4" />
                        {mode === "create" ? "Create Auction" : "Save Changes"}
                      </>
                    )}
                  </Button>
                ) : (
                  <Button type="button" onClick={goToNextStep}>
                    Next
                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 h-4 w-4" />
                  </Button>
                )}
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}
