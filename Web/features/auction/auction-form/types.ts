import * as z from "zod";
import { ShippingType, Auction, Category } from "@/types/auction";
import { UploadedImage } from "@/components/ui/cloudinary-upload";

export const auctionFormSchema = z
  .object({
    title: z.string().min(3, "Title must be at least 3 characters"),
    description: z.string().min(10, "Description must be at least 10 characters"),
    yearManufactured: z
      .number()
      .min(1900)
      .max(new Date().getFullYear() + 1)
      .optional(),
    condition: z.string().optional(),
    reservePrice: z.number().min(1, "Reserve price must be at least 1"),
    buyNowPrice: z.number().min(1, "Buy Now price must be at least 1").optional(),
    auctionEnd: z.string().refine((date) => new Date(date) > new Date(), {
      message: "Auction end date must be in the future",
    }),
    categoryId: z.string().optional(),
    isFeatured: z.boolean(),
    imageUrl: z.string().url("Must be a valid URL").optional().or(z.literal("")),
    shippingType: z.nativeEnum(ShippingType),
    shippingCost: z.number().min(0).optional(),
    handlingTime: z.number().min(1).max(30),
    shipsFrom: z.string().optional(),
    localPickupAvailable: z.boolean(),
    localPickupAddress: z.string().optional(),
  })
  .refine((data) => !data.buyNowPrice || data.buyNowPrice > data.reservePrice, {
    message: "Buy Now price must be greater than reserve price",
    path: ["buyNowPrice"],
  })
  .refine(
    (data) =>
      data.shippingType !== ShippingType.Flat ||
      (data.shippingCost && data.shippingCost > 0),
    {
      message: "Flat rate shipping requires a shipping cost",
      path: ["shippingCost"],
    }
  )
  .refine((data) => !data.localPickupAvailable || data.localPickupAddress, {
    message: "Local pickup requires an address",
    path: ["localPickupAddress"],
  });

export type AuctionFormValues = z.infer<typeof auctionFormSchema>;

export type AuctionFormMode = "create" | "edit";

export type FormStep = "details" | "pricing" | "shipping" | "images" | "review";

export const FORM_STEPS: { id: FormStep; label: string; description: string }[] = [
  { id: "details", label: "Details", description: "Basic information" },
  { id: "pricing", label: "Pricing", description: "Set your prices" },
  { id: "shipping", label: "Shipping", description: "Delivery options" },
  { id: "images", label: "Images", description: "Add photos" },
  { id: "review", label: "Review", description: "Confirm & submit" },
];

export const DEFAULT_FORM_VALUES: AuctionFormValues = {
  title: "",
  description: "",
  yearManufactured: new Date().getFullYear(),
  condition: "Used",
  reservePrice: 0,
  buyNowPrice: undefined,
  auctionEnd: "",
  categoryId: "",
  isFeatured: false,
  imageUrl: "",
  shippingType: ShippingType.Flat,
  shippingCost: 0,
  handlingTime: 3,
  shipsFrom: "",
  localPickupAvailable: false,
  localPickupAddress: "",
};

export interface AuctionFormProps {
  mode: AuctionFormMode;
  initialData?: Auction;
  categories: Category[];
  onSubmit: (values: AuctionFormValues, images: UploadedImage[]) => Promise<void>;
}

export interface StepProps {
  form: ReturnType<typeof import("react-hook-form").useForm<AuctionFormValues>>;
  categories: Category[];
  uploadedImages: UploadedImage[];
  onImagesChange: (images: UploadedImage[]) => void;
  mode: AuctionFormMode;
}
