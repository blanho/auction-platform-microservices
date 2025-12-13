'use client';

import { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { toast } from 'sonner';
import {
    Calendar,
    DollarSign,
    ImagePlus,
    Loader2,
    Upload,
    X,
} from 'lucide-react';
import Image from 'next/image';

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

import { auctionService } from '@/services/auction.service';
import { CreateAuctionDto, Category } from '@/types/auction';

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
}).refine((data) => !data.buyNowPrice || data.buyNowPrice > data.reservePrice, {
    message: 'Buy Now price must be greater than reserve price',
    path: ['buyNowPrice'],
});

type CreateAuctionFormValues = z.infer<typeof createAuctionSchema>;

interface UploadedImage {
    id: string;
    url: string;
    name: string;
}

export function CreateAuctionForm() {
    const router = useRouter();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [categories, setCategories] = useState<Category[]>([]);
    const [uploadedImages, setUploadedImages] = useState<UploadedImage[]>([]);
    const [isDragging, setIsDragging] = useState(false);

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
        },
    });

    const handleDrop = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(false);

        const files = Array.from(e.dataTransfer.files).filter(file =>
            file.type.startsWith('image/')
        );

        files.forEach(file => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const url = e.target?.result as string;
                setUploadedImages(prev => [...prev, {
                    id: Math.random().toString(36).substring(7),
                    url,
                    name: file.name
                }]);
            };
            reader.readAsDataURL(file);
        });
    }, []);

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = Array.from(e.target.files || []).filter(file =>
            file.type.startsWith('image/')
        );

        files.forEach(file => {
            const reader = new FileReader();
            reader.onload = (event) => {
                const url = event.target?.result as string;
                setUploadedImages(prev => [...prev, {
                    id: Math.random().toString(36).substring(7),
                    url,
                    name: file.name
                }]);
            };
            reader.readAsDataURL(file);
        });
    };

    const removeImage = (id: string) => {
        setUploadedImages(prev => prev.filter(img => img.id !== id));
    };

    const onSubmit = async (values: CreateAuctionFormValues) => {
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
            imageUrl: uploadedImages[0]?.url || values.imageUrl || undefined,
        };
        
        try {
            await auctionService.createAuction(data);
            toast.success('Auction created successfully!');
            router.push('/auctions');
        } catch (error) {
            const err = error as { response?: { data?: { message?: string } } };
            toast.error(err?.response?.data?.message || 'Failed to create auction');
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
            </CardHeader>
            <CardContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
                        {/* Photos Upload Section */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                <ImagePlus className="h-5 w-5" />
                                Photos
                            </h3>
                            <div
                                className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
                                    isDragging
                                        ? 'border-amber-500 bg-amber-50 dark:bg-amber-900/10'
                                        : 'border-zinc-300 dark:border-zinc-700'
                                }`}
                                onDragOver={(e) => {
                                    e.preventDefault();
                                    setIsDragging(true);
                                }}
                                onDragLeave={() => setIsDragging(false)}
                                onDrop={handleDrop}
                            >
                                <Upload className="h-10 w-10 mx-auto text-zinc-400 mb-4" />
                                <p className="text-zinc-600 dark:text-zinc-400 mb-2">
                                    Drag and drop images here, or click to browse
                                </p>
                                <input
                                    type="file"
                                    accept="image/*"
                                    multiple
                                    className="hidden"
                                    id="image-upload"
                                    onChange={handleFileSelect}
                                />
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => document.getElementById('image-upload')?.click()}
                                >
                                    <ImagePlus className="h-4 w-4 mr-2" />
                                    Add Images
                                </Button>
                            </div>

                            {uploadedImages.length > 0 && (
                                <div className="flex gap-2 flex-wrap">
                                    {uploadedImages.map((img) => (
                                        <div
                                            key={img.id}
                                            className="relative w-24 h-24 rounded-lg overflow-hidden border group"
                                        >
                                            <Image
                                                src={img.url}
                                                alt={img.name}
                                                fill
                                                className="object-cover"
                                            />
                                            <button
                                                type="button"
                                                onClick={() => removeImage(img.id)}
                                                className="absolute top-1 right-1 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                                            >
                                                <X className="h-3 w-3" />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}

                            <FormField
                                control={form.control}
                                name="imageUrl"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Or enter image URL</FormLabel>
                                        <FormControl>
                                            <Input
                                                type="url"
                                                placeholder="https://example.com/image.jpg"
                                                {...field}
                                            />
                                        </FormControl>
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
                                            <FormLabel>Title</FormLabel>
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
                                            <FormLabel>Description</FormLabel>
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
                                            <FormLabel>Condition</FormLabel>
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
                                            <FormLabel>Make</FormLabel>
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
                                            <FormLabel>Model</FormLabel>
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
                                            <FormLabel>Year</FormLabel>
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
                                            <FormLabel>Color</FormLabel>
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
                                            <FormLabel>Mileage</FormLabel>
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
                                            <FormLabel>Reserve Price ($)</FormLabel>
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
                                            <FormLabel>End Date & Time</FormLabel>
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
