'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { toast } from 'sonner';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faImage,
    faCalendar,
    faDollarSign,
    faSpinner,
    faBox,
} from '@fortawesome/free-solid-svg-icons';

import { Button } from '@/components/ui/button';
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Separator } from '@/components/ui/separator';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { CloudinaryUpload, UploadedImage } from '@/components/ui/cloudinary-upload';

import { auctionService } from '@/services/auction.service';
import {
    Auction,
    UpdateAuctionDto,
    Category,
    ShippingType,
    SHIPPING_TYPE_LABELS,
    AuctionStatus,
} from '@/types/auction';
import { showErrorToast } from '@/utils';
import {
    getAuctionTitle,
    getAuctionDescription,
    getAuctionCondition,
    getAuctionYearManufactured,
    getAuctionAttributes,
} from '@/utils/auction';

function RequiredIndicator() {
    return <span className="text-red-500 ml-0.5">*</span>;
}

const updateAuctionSchema = z.object({
    title: z.string().min(3, 'Title must be at least 3 characters'),
    description: z.string().min(10, 'Description must be at least 10 characters'),
    yearManufactured: z.number().min(1900).max(new Date().getFullYear() + 1).optional(),
    reservePrice: z.number().min(1, 'Reserve price must be at least 1'),
    buyNowPrice: z.number().min(1, 'Buy Now price must be at least 1').optional(),
    auctionEnd: z.string().refine((date) => new Date(date) > new Date(), {
        message: 'Auction end date must be in the future',
    }),
    categoryId: z.string().optional(),
    condition: z.string(),
    isFeatured: z.boolean(),
    shippingType: z.nativeEnum(ShippingType),
    shippingCost: z.number().min(0).optional(),
    handlingTime: z.number().min(1).max(30),
    shipsFrom: z.string().optional(),
    localPickupAvailable: z.boolean(),
    localPickupAddress: z.string().optional(),
}).refine((data) => !data.buyNowPrice || data.buyNowPrice > data.reservePrice, {
    message: 'Buy Now price must be greater than reserve price',
    path: ['buyNowPrice'],
}).refine((data) => data.shippingType !== ShippingType.Flat || (data.shippingCost && data.shippingCost > 0), {
    message: 'Flat rate shipping requires a shipping cost',
    path: ['shippingCost'],
}).refine((data) => !data.localPickupAvailable || data.localPickupAddress, {
    message: 'Local pickup requires an address',
    path: ['localPickupAddress'],
});

type UpdateAuctionFormValues = z.infer<typeof updateAuctionSchema>;

interface EditAuctionFormProps {
    auction: Auction;
    categories: Category[];
}

function formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16);
}

export function EditAuctionForm({ auction, categories }: EditAuctionFormProps) {
    const router = useRouter();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [uploadedImages, setUploadedImages] = useState<UploadedImage[]>([]);

    const isLive = auction.status === AuctionStatus.Live;
    const isEnded = auction.status === AuctionStatus.Finished || auction.status === AuctionStatus.ReserveNotMet;

    useEffect(() => {
        if (auction.files && auction.files.length > 0) {
            const existingImages: UploadedImage[] = auction.files
                .filter(f => f.fileType === 'Image' && f.url)
                .sort((a, b) => a.displayOrder - b.displayOrder)
                .map((f, index) => ({
                    id: f.storageFileId || `existing_${index}`,
                    publicId: f.storageFileId || '',
                    url: f.url || '',
                    name: f.fileName,
                    isPrimary: f.isPrimary,
                    status: 'success' as const,
                    progress: 100,
                }));
            setUploadedImages(existingImages);
        }
    }, [auction.files]);

    const form = useForm<UpdateAuctionFormValues>({
        resolver: zodResolver(updateAuctionSchema),
        defaultValues: {
            title: getAuctionTitle(auction),
            description: getAuctionDescription(auction),
            yearManufactured: getAuctionYearManufactured(auction),
            reservePrice: auction.reservePrice,
            buyNowPrice: auction.buyNowPrice || undefined,
            auctionEnd: formatDateForInput(auction.auctionEnd),
            categoryId: auction.categoryId || '',
            condition: getAuctionCondition(auction) || 'Used',
            isFeatured: auction.isFeatured,
            shippingType: auction.shippingType || ShippingType.Flat,
            shippingCost: auction.shippingCost || 0,
            handlingTime: auction.handlingTime || 3,
            shipsFrom: auction.shipsFrom || '',
            localPickupAvailable: auction.localPickupAvailable || false,
            localPickupAddress: auction.localPickupAddress || '',
        },
    });

    const onSubmit = async (values: UpdateAuctionFormValues) => {
        if (uploadedImages.some(img => img.status === 'uploading')) {
            toast.error('Please wait for images to finish uploading');
            return;
        }

        const successfulImages = uploadedImages.filter(img => img.status === 'success');
        const primaryImage = successfulImages.find(img => img.isPrimary) || successfulImages[0];

        const files = successfulImages.map((img, index) => ({
            url: img.url,
            publicId: img.publicId,
            fileName: img.name,
            contentType: 'image/jpeg',
            size: 0,
            displayOrder: index,
            isPrimary: img.isPrimary || (index === 0 && !successfulImages.some(i => i.isPrimary)),
        }));

        setIsSubmitting(true);

        const changedFields: UpdateAuctionDto = {};
        const currentTitle = getAuctionTitle(auction);
        const currentDescription = getAuctionDescription(auction);
        const currentYearManufactured = getAuctionYearManufactured(auction);
        const currentCondition = getAuctionCondition(auction);

        if (values.title !== currentTitle) changedFields.title = values.title;
        if (values.description !== currentDescription) changedFields.description = values.description;
        if (values.yearManufactured !== currentYearManufactured) changedFields.yearManufactured = values.yearManufactured;
        if (values.condition !== currentCondition) changedFields.condition = values.condition;

        if (!isLive) {
            if (values.reservePrice !== auction.reservePrice) changedFields.reservePrice = values.reservePrice;
            if (values.buyNowPrice !== auction.buyNowPrice) changedFields.buyNowPrice = values.buyNowPrice;
        }

        if (values.auctionEnd !== formatDateForInput(auction.auctionEnd)) changedFields.auctionEnd = values.auctionEnd;
        if (values.categoryId !== auction.categoryId) changedFields.categoryId = values.categoryId || undefined;
        if (values.isFeatured !== auction.isFeatured) changedFields.isFeatured = values.isFeatured;

        if (values.shippingType !== auction.shippingType) changedFields.shippingType = values.shippingType;
        if (values.shippingCost !== auction.shippingCost) changedFields.shippingCost = values.shippingCost;
        if (values.handlingTime !== auction.handlingTime) changedFields.handlingTime = values.handlingTime;
        if (values.shipsFrom !== auction.shipsFrom) changedFields.shipsFrom = values.shipsFrom;
        if (values.localPickupAvailable !== auction.localPickupAvailable) changedFields.localPickupAvailable = values.localPickupAvailable;
        if (values.localPickupAddress !== auction.localPickupAddress) changedFields.localPickupAddress = values.localPickupAddress;

        if (files.length > 0) {
            changedFields.files = files;
        }

        if (Object.keys(changedFields).length === 0) {
            toast.info('No changes detected');
            setIsSubmitting(false);
            return;
        }

        try {
            await auctionService.updateAuction(auction.id, changedFields);
            toast.success('Auction updated successfully!');
            router.push(`/auctions/${auction.id}`);
        } catch (error: unknown) {
            showErrorToast(error, {
                description: 'Please check your input and try again',
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-2xl">Edit Auction</CardTitle>
                <CardDescription>
                    Update the details of your auction listing
                </CardDescription>
                <p className="text-sm text-slate-500 dark:text-slate-400 mt-2">
                    Fields marked with <span className="text-red-500">*</span> are required
                </p>
                {isLive && (
                    <p className="text-sm text-amber-600 dark:text-amber-400 mt-2">
                        Note: Reserve price and Buy Now price cannot be changed while the auction is live.
                    </p>
                )}
                {isEnded && (
                    <p className="text-sm text-red-600 dark:text-red-400 mt-2">
                        This auction has ended and cannot be edited.
                    </p>
                )}
            </CardHeader>
            <CardContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <FontAwesomeIcon icon={faImage} className="h-5 w-5 text-purple-500" />
                                Photos
                            </h3>

                            <CloudinaryUpload
                                value={uploadedImages}
                                onChange={setUploadedImages}
                                maxImages={10}
                                maxSizeMB={10}
                                folder="auction"
                                disabled={isEnded}
                            />
                        </div>

                        <Separator />

                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold">Basic Information</h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="title"
                                    render={({ field }) => (
                                        <FormItem className="md:col-span-2">
                                            <FormLabel>Title<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input
                                                    placeholder="2020 Toyota Camry XSE V6"
                                                    {...field}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="description"
                                    render={({ field }) => (
                                        <FormItem className="md:col-span-2">
                                            <FormLabel>Description<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Textarea
                                                    placeholder="Describe your item in detail..."
                                                    className="min-h-[150px]"
                                                    {...field}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="categoryId"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Category</FormLabel>
                                            <Select
                                                onValueChange={field.onChange}
                                                value={field.value}
                                                disabled={isEnded}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select category" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    {categories.map((cat) => (
                                                        <SelectItem key={cat.id} value={cat.id}>
                                                            {cat.name}
                                                        </SelectItem>
                                                    ))}
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="condition"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Condition<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <RadioGroup
                                                    onValueChange={field.onChange}
                                                    defaultValue={field.value}
                                                    className="flex gap-4"
                                                    disabled={isEnded}
                                                >
                                                    <div className="flex items-center space-x-2">
                                                        <RadioGroupItem value="New" id="new" />
                                                        <Label htmlFor="new">New</Label>
                                                    </div>
                                                    <div className="flex items-center space-x-2">
                                                        <RadioGroupItem value="Used" id="used" />
                                                        <Label htmlFor="used">Used</Label>
                                                    </div>
                                                </RadioGroup>
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold">Item Details</h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="condition"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Condition<RequiredIndicator /></FormLabel>
                                            <Select
                                                value={field.value}
                                                onValueChange={field.onChange}
                                                disabled={isEnded}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select condition" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    <SelectItem value="New">New</SelectItem>
                                                    <SelectItem value="Like New">Like New</SelectItem>
                                                    <SelectItem value="Excellent">Excellent</SelectItem>
                                                    <SelectItem value="Good">Good</SelectItem>
                                                    <SelectItem value="Fair">Fair</SelectItem>
                                                    <SelectItem value="Poor">Poor</SelectItem>
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="yearManufactured"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Year Manufactured</FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="number"
                                                    placeholder="2020"
                                                    value={field.value || ''}
                                                    onChange={(e) => field.onChange(e.target.valueAsNumber || undefined)}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Optional: Year the item was manufactured
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <FontAwesomeIcon icon={faDollarSign} className="h-5 w-5" />
                                Pricing
                            </h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="reservePrice"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Reserve Price ($)<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="number"
                                                    placeholder="20000"
                                                    value={field.value || ''}
                                                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                                                    disabled={isLive || isEnded}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                {isLive
                                                    ? 'Cannot change while auction is live'
                                                    : 'Minimum price you\'ll accept (required)'}
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="buyNowPrice"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Buy Now Price ($)</FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="number"
                                                    placeholder="25000"
                                                    value={field.value || ''}
                                                    onChange={(e) => field.onChange(e.target.valueAsNumber || undefined)}
                                                    disabled={isLive || isEnded}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                {isLive
                                                    ? 'Cannot change while auction is live'
                                                    : 'Optional: Instant purchase price'}
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <FontAwesomeIcon icon={faCalendar} className="h-5 w-5" />
                                Auction Timing
                            </h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="auctionEnd"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>End Date & Time<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="datetime-local"
                                                    {...field}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                When the auction will end
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="isFeatured"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                            <div className="space-y-0.5">
                                                <FormLabel className="text-base">
                                                    Featured Listing
                                                </FormLabel>
                                                <FormDescription>
                                                    Display this auction in featured sections
                                                </FormDescription>
                                            </div>
                                            <FormControl>
                                                <Switch
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <FontAwesomeIcon icon={faBox} className="h-5 w-5" />
                                Shipping Options
                            </h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="shippingType"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Shipping Method<RequiredIndicator /></FormLabel>
                                            <Select
                                                onValueChange={field.onChange}
                                                value={field.value}
                                                disabled={isEnded}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select shipping method" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    {Object.entries(SHIPPING_TYPE_LABELS).map(([key, label]) => (
                                                        <SelectItem key={key} value={key}>
                                                            {label}
                                                        </SelectItem>
                                                    ))}
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                {form.watch('shippingType') === ShippingType.Flat && (
                                    <FormField
                                        control={form.control}
                                        name="shippingCost"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Shipping Cost ($)</FormLabel>
                                                <FormControl>
                                                    <Input
                                                        type="number"
                                                        placeholder="15.00"
                                                        value={field.value || ''}
                                                        onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                                                        disabled={isEnded}
                                                    />
                                                </FormControl>
                                                <FormDescription>
                                                    Fixed shipping cost for this item
                                                </FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                )}

                                <FormField
                                    control={form.control}
                                    name="handlingTime"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Handling Time (days)</FormLabel>
                                            <Select
                                                onValueChange={(value) => field.onChange(parseInt(value))}
                                                value={field.value?.toString()}
                                                disabled={isEnded}
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select handling time" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    {[1, 2, 3, 5, 7, 10, 14, 21, 30].map((days) => (
                                                        <SelectItem key={days} value={days.toString()}>
                                                            {days === 1 ? '1 business day' : `${days} business days`}
                                                        </SelectItem>
                                                    ))}
                                                </SelectContent>
                                            </Select>
                                            <FormDescription>
                                                Time to ship after payment received
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="shipsFrom"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Ships From</FormLabel>
                                            <FormControl>
                                                <Input
                                                    placeholder="City, State"
                                                    {...field}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="localPickupAvailable"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                            <div className="space-y-0.5">
                                                <FormLabel className="text-base">
                                                    Local Pickup Available
                                                </FormLabel>
                                                <FormDescription>
                                                    Allow buyers to pick up in person
                                                </FormDescription>
                                            </div>
                                            <FormControl>
                                                <Switch
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                    disabled={isEnded}
                                                />
                                            </FormControl>
                                        </FormItem>
                                    )}
                                />

                                {form.watch('localPickupAvailable') && (
                                    <FormField
                                        control={form.control}
                                        name="localPickupAddress"
                                        render={({ field }) => (
                                            <FormItem className="md:col-span-2">
                                                <FormLabel>Pickup Address</FormLabel>
                                                <FormControl>
                                                    <Input
                                                        placeholder="Enter pickup location"
                                                        {...field}
                                                        disabled={isEnded}
                                                    />
                                                </FormControl>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                )}
                            </div>
                        </div>

                        <Separator />

                        <div className="flex gap-4 justify-end">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => router.back()}
                                disabled={isSubmitting}
                            >
                                Cancel
                            </Button>
                            <Button type="submit" disabled={isSubmitting || isEnded}>
                                {isSubmitting && <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />}
                                Update Auction
                            </Button>
                        </div>
                    </form>
                </Form>
            </CardContent>
        </Card>
    );
}
