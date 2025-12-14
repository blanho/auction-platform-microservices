'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { toast } from 'sonner';
import {
    Calendar,
    DollarSign,
    Loader2,
    Package,
} from 'lucide-react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faImage } from '@fortawesome/free-solid-svg-icons';

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
import { CreateAuctionDto, Category, ShippingType, SHIPPING_TYPE_LABELS } from '@/types/auction';
import { showErrorToast } from '@/utils';

function RequiredIndicator() {
    return <span className="text-red-500 ml-0.5">*</span>;
}

const createAuctionSchema = z.object({
    title: z.string().min(3, 'Title must be at least 3 characters'),
    description: z.string().min(10, 'Description must be at least 10 characters'),
    make: z.string().min(2, 'Make is required'),
    model: z.string().min(1, 'Model is required'),
    year: z.number().min(1900).max(new Date().getFullYear() + 1),
    color: z.string().min(2, 'Color is required'),
    mileage: z.number().min(0, 'Mileage must be positive'),
    reservePrice: z.number().min(1, 'Reserve price must be at least 1'),
    buyNowPrice: z.number().min(1, 'Buy Now price must be at least 1').optional(),
    auctionEnd: z.string().refine((date) => new Date(date) > new Date(), {
        message: 'Auction end date must be in the future',
    }),
    categoryId: z.string().optional(),
    condition: z.string(),
    autoExtend: z.boolean(),
    isFeatured: z.boolean(),
    imageUrl: z.string().url('Must be a valid URL').optional().or(z.literal('')),
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

type CreateAuctionFormValues = z.infer<typeof createAuctionSchema>;

export function CreateAuctionForm() {
    const router = useRouter();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [categories, setCategories] = useState<Category[]>([]);
    const [uploadedImages, setUploadedImages] = useState<UploadedImage[]>([]);

    useEffect(() => {
        auctionService.getCategories().then(setCategories).catch(() => {});
    }, []);

    const form = useForm<CreateAuctionFormValues>({
        resolver: zodResolver(createAuctionSchema),
        defaultValues: {
            title: '',
            description: '',
            make: '',
            model: '',
            year: new Date().getFullYear(),
            color: '',
            mileage: 0,
            reservePrice: 0,
            buyNowPrice: undefined,
            auctionEnd: '',
            categoryId: '',
            condition: 'Used',
            autoExtend: true,
            isFeatured: false,
            imageUrl: '',
            shippingType: ShippingType.Flat,
            shippingCost: 0,
            handlingTime: 3,
            shipsFrom: '',
            localPickupAvailable: false,
            localPickupAddress: '',
        },
    });

    const onSubmit = async (values: CreateAuctionFormValues) => {
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
        const data: CreateAuctionDto = {
            title: values.title,
            description: values.description,
            make: values.make,
            model: values.model,
            year: values.year,
            color: values.color,
            mileage: values.mileage,
            reservePrice: values.reservePrice,
            buyNowPrice: values.buyNowPrice || undefined,
            auctionEnd: values.auctionEnd,
            categoryId: values.categoryId || undefined,
            isFeatured: values.isFeatured,
            imageUrl: primaryImage?.url || values.imageUrl || undefined,
            shippingType: values.shippingType,
            shippingCost: values.shippingType === ShippingType.Flat ? values.shippingCost : undefined,
            handlingTime: values.handlingTime,
            shipsFrom: values.shipsFrom || undefined,
            localPickupAvailable: values.localPickupAvailable,
            localPickupAddress: values.localPickupAvailable ? values.localPickupAddress : undefined,
            files: files.length > 0 ? files : undefined,
        };
        
        try {
            await auctionService.createAuction(data);
            toast.success('Auction created successfully!');
            router.push('/auctions');
        } catch (error: unknown) {
            console.error('Create auction error:', error);
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
                <CardTitle className="text-2xl">Create New Auction</CardTitle>
                <CardDescription>
                    Fill in the details to create a new auction listing
                </CardDescription>
                <p className="text-sm text-slate-500 dark:text-slate-400 mt-2">
                    Fields marked with <span className="text-red-500">*</span> are required
                </p>
            </CardHeader>
            <CardContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
                        {/* Photos Upload Section */}
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
                            />

                            <FormField
                                control={form.control}
                                name="imageUrl"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Or enter image URL directly</FormLabel>
                                        <FormControl>
                                            <Input
                                                type="url"
                                                placeholder="https://example.com/image.jpg"
                                                {...field}
                                                disabled={uploadedImages.length > 0}
                                            />
                                        </FormControl>
                                        <FormDescription>
                                            {uploadedImages.length > 0 
                                                ? 'URL input disabled when images are uploaded' 
                                                : 'Use this if you have an existing image URL'}
                                        </FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <Separator />

                        {/* Basic Information */}
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
                                                <Input placeholder="2020 Toyota Camry XSE V6" {...field} />
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
                                                    placeholder="Describe your item in detail. Include condition, features, history, etc."
                                                    className="min-h-[150px]"
                                                    {...field}
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
                                            <Select onValueChange={field.onChange} value={field.value}>
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

                        {/* Vehicle Details */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold">Vehicle Details</h3>
                            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                                <FormField
                                    control={form.control}
                                    name="make"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Make<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input placeholder="Toyota" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="model"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Model<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input placeholder="Camry" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="year"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Year<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="number"
                                                    placeholder="2020"
                                                    {...field}
                                                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                                                />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="color"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Color<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input placeholder="Silver" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <FormField
                                    control={form.control}
                                    name="mileage"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Mileage<RequiredIndicator /></FormLabel>
                                            <FormControl>
                                                <Input
                                                    type="number"
                                                    placeholder="50000"
                                                    {...field}
                                                    onChange={(e) => field.onChange(e.target.valueAsNumber)}
                                                />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        {/* Pricing Section */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <DollarSign className="h-5 w-5" />
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
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Minimum price you&apos;ll accept (required)
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
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Optional: Instant purchase price
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        {/* Auction Timing */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <Calendar className="h-5 w-5" />
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
                                                <Input type="datetime-local" {...field} />
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
                                    name="autoExtend"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                            <div className="space-y-0.5">
                                                <FormLabel className="text-base">
                                                    Auto-extend last minute?
                                                </FormLabel>
                                                <FormDescription>
                                                    Extend auction by 5 minutes if bid placed in final minute
                                                </FormDescription>
                                            </div>
                                            <FormControl>
                                                <Switch
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                />
                                            </FormControl>
                                        </FormItem>
                                    )}
                                />
                            </div>
                        </div>

                        <Separator />

                        {/* Shipping Options */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <Package className="h-5 w-5" />
                                Shipping Options
                            </h3>
                            <div className="grid gap-6 md:grid-cols-2">
                                <FormField
                                    control={form.control}
                                    name="shippingType"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Shipping Method<RequiredIndicator /></FormLabel>
                                            <Select onValueChange={field.onChange} value={field.value}>
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
                                            >
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select handling time" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    <SelectItem value="1">1 business day</SelectItem>
                                                    <SelectItem value="2">2 business days</SelectItem>
                                                    <SelectItem value="3">3 business days</SelectItem>
                                                    <SelectItem value="5">5 business days</SelectItem>
                                                    <SelectItem value="7">7 business days</SelectItem>
                                                    <SelectItem value="10">10 business days</SelectItem>
                                                </SelectContent>
                                            </Select>
                                            <FormDescription>
                                                Time to prepare and ship after payment
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
                                                    placeholder="City, State or Country"
                                                    {...field}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Location where item will ship from
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>

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
                                                Allow buyers to pick up the item in person
                                            </FormDescription>
                                        </div>
                                        <FormControl>
                                            <Switch
                                                checked={field.value}
                                                onCheckedChange={field.onChange}
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
                                        <FormItem>
                                            <FormLabel>Pickup Address</FormLabel>
                                            <FormControl>
                                                <Input
                                                    placeholder="Enter pickup address or area"
                                                    {...field}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                General area or specific address for pickup
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            )}
                        </div>

                        <Separator />

                        {/* Featured Option */}
                        <FormField
                            control={form.control}
                            name="isFeatured"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 bg-amber-50 dark:bg-amber-900/10">
                                    <div className="space-y-0.5">
                                        <FormLabel className="text-base">
                                            ⭐ Feature this auction
                                        </FormLabel>
                                        <FormDescription>
                                            Featured auctions get more visibility on the homepage
                                        </FormDescription>
                                    </div>
                                    <FormControl>
                                        <Switch
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                </FormItem>
                            )}
                        />

                        {/* Submit Buttons */}
                        <div className="flex gap-4 justify-end pt-4">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => router.back()}
                                disabled={isSubmitting}
                            >
                                Cancel
                            </Button>
                            <Button
                                type="submit"
                                disabled={isSubmitting}
                                className="bg-amber-500 hover:bg-amber-600"
                            >
                                {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                Create Auction
                            </Button>
                        </div>
                    </form>
                </Form>
            </CardContent>
        </Card>
    );
}
