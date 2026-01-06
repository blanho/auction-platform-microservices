"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateAuction, useCategories, useBrands } from "@repo/hooks";
import { createAuctionSchema, type CreateAuctionInput } from "@repo/validators";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Button,
  Input,
  Label,
  Textarea,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Spinner,
} from "@repo/ui";
import { ArrowLeft, ArrowRight } from "lucide-react";

const steps = [
  { id: 1, title: "Basic Info" },
  { id: 2, title: "Item Details" },
  { id: 3, title: "Pricing" },
  { id: 4, title: "Schedule" },
  { id: 5, title: "Review" },
];

export default function CreateAuctionPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState(1);
  const { data: categories } = useCategories();
  const { data: brands } = useBrands();
  const { mutate: createAuction, isPending } = useCreateAuction();

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<CreateAuctionInput>({
    resolver: zodResolver(createAuctionSchema),
    defaultValues: {
      title: "",
      description: "",
      reservePrice: 0,
      imageUrls: [],
      item: {
        condition: "New",
      },
    },
  });

  const formValues = watch();

  function onSubmit(data: CreateAuctionInput) {
    createAuction(data, {
      onSuccess: (auction) => {
        router.push(`/seller/auctions/${auction.id}/edit`);
      },
    });
  }

  function nextStep() {
    if (currentStep < steps.length) {
      setCurrentStep(currentStep + 1);
    }
  }

  function prevStep() {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Create Auction</h1>
        <p className="mt-2 text-muted-foreground">
          List your item for auction in a few simple steps
        </p>
      </div>

      <div className="flex gap-2">
        {steps.map((step) => (
          <div
            key={step.id}
            className={`flex-1 rounded-lg p-2 text-center text-sm ${
              step.id === currentStep
                ? "bg-primary text-primary-foreground"
                : step.id < currentStep
                  ? "bg-primary/20"
                  : "bg-muted"
            }`}
          >
            {step.title}
          </div>
        ))}
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        {currentStep === 1 && (
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="title">Title *</Label>
                <Input
                  id="title"
                  placeholder="Enter a descriptive title"
                  {...register("title")}
                />
                {errors.title && (
                  <p className="text-sm text-destructive">{errors.title.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description *</Label>
                <Textarea
                  id="description"
                  placeholder="Describe your item in detail"
                  rows={6}
                  {...register("description")}
                />
                {errors.description && (
                  <p className="text-sm text-destructive">{errors.description.message}</p>
                )}
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label>Category *</Label>
                  <Select
                    value={formValues.categoryId}
                    onValueChange={(value) => setValue("categoryId", value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      {categories?.map((cat) => (
                        <SelectItem key={cat.id} value={cat.id}>
                          {cat.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.categoryId && (
                    <p className="text-sm text-destructive">{errors.categoryId.message}</p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label>Brand (Optional)</Label>
                  <Select
                    value={formValues.brandId || ""}
                    onValueChange={(value) => setValue("brandId", value || undefined)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select brand" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="">No brand</SelectItem>
                      {brands?.map((brand) => (
                        <SelectItem key={brand.id} value={brand.id}>
                          {brand.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {currentStep === 2 && (
          <Card>
            <CardHeader>
              <CardTitle>Item Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Condition *</Label>
                <Select
                  value={formValues.item?.condition}
                  onValueChange={(value) => setValue("item.condition", value)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select condition" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="New">New</SelectItem>
                    <SelectItem value="LikeNew">Like New</SelectItem>
                    <SelectItem value="Good">Good</SelectItem>
                    <SelectItem value="Fair">Fair</SelectItem>
                    <SelectItem value="Poor">Poor</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="model">Model</Label>
                  <Input
                    id="model"
                    placeholder="Model number or name"
                    {...register("item.model")}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="year">Year</Label>
                  <Input
                    id="year"
                    type="number"
                    placeholder="Manufacturing year"
                    {...register("item.year", { valueAsNumber: true })}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="color">Color</Label>
                <Input id="color" placeholder="Color" {...register("item.color")} />
              </div>
            </CardContent>
          </Card>
        )}

        {currentStep === 3 && (
          <Card>
            <CardHeader>
              <CardTitle>Pricing</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="reservePrice">Reserve Price *</Label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                    $
                  </span>
                  <Input
                    id="reservePrice"
                    type="number"
                    step="0.01"
                    min="1"
                    className="pl-7"
                    placeholder="0.00"
                    {...register("reservePrice", { valueAsNumber: true })}
                  />
                </div>
                <p className="text-xs text-muted-foreground">
                  Minimum price you&apos;re willing to accept
                </p>
                {errors.reservePrice && (
                  <p className="text-sm text-destructive">{errors.reservePrice.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="buyNowPrice">Buy Now Price (Optional)</Label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                    $
                  </span>
                  <Input
                    id="buyNowPrice"
                    type="number"
                    step="0.01"
                    className="pl-7"
                    placeholder="0.00"
                    {...register("buyNowPrice", { valueAsNumber: true })}
                  />
                </div>
                <p className="text-xs text-muted-foreground">
                  Allow instant purchase at this price
                </p>
              </div>
            </CardContent>
          </Card>
        )}

        {currentStep === 4 && (
          <Card>
            <CardHeader>
              <CardTitle>Schedule</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="auctionStart">Start Date & Time *</Label>
                <Input
                  id="auctionStart"
                  type="datetime-local"
                  {...register("auctionStart")}
                />
                {errors.auctionStart && (
                  <p className="text-sm text-destructive">{errors.auctionStart.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="auctionEnd">End Date & Time *</Label>
                <Input
                  id="auctionEnd"
                  type="datetime-local"
                  {...register("auctionEnd")}
                />
                {errors.auctionEnd && (
                  <p className="text-sm text-destructive">{errors.auctionEnd.message}</p>
                )}
              </div>
            </CardContent>
          </Card>
        )}

        {currentStep === 5 && (
          <Card>
            <CardHeader>
              <CardTitle>Review Your Auction</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <p className="text-sm text-muted-foreground">Title</p>
                  <p className="font-medium">{formValues.title || "-"}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Category</p>
                  <p className="font-medium">
                    {categories?.find((c) => c.id === formValues.categoryId)?.name || "-"}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Reserve Price</p>
                  <p className="font-medium">${formValues.reservePrice || 0}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Condition</p>
                  <p className="font-medium">{formValues.item?.condition || "-"}</p>
                </div>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Description</p>
                <p className="whitespace-pre-wrap">{formValues.description || "-"}</p>
              </div>
            </CardContent>
          </Card>
        )}

        <div className="mt-6 flex justify-between">
          <Button
            type="button"
            variant="outline"
            onClick={prevStep}
            disabled={currentStep === 1}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Previous
          </Button>
          {currentStep < steps.length ? (
            <Button type="button" onClick={nextStep}>
              Next
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          ) : (
            <Button type="submit" disabled={isPending}>
              {isPending ? <Spinner size="sm" /> : "Create Auction"}
            </Button>
          )}
        </div>
      </form>
    </div>
  );
}
