"use client";

import { useState, useEffect, useCallback } from "react";
import {
    Search,
    MoreHorizontal,
    Plus,
    Pencil,
    Trash2,
    Eye,
    EyeOff,
    Loader2,
    RefreshCw,
    Star,
} from "lucide-react";
import Image from "next/image";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faImage, faTag } from "@fortawesome/free-solid-svg-icons";

import { MESSAGES } from "@/constants";
import { formatNumber } from "@/utils";

import { AdminLayout } from "@/components/layout/admin-layout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Checkbox } from "@/components/ui/checkbox";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { SingleImageUpload } from "@/components/ui/single-image-upload";
import { toast } from "sonner";

import {
    brandService,
    Brand,
    CreateBrandDto,
    UpdateBrandDto,
} from "@/services/brand.service";

function StatCard({
    title,
    value,
    colorClass,
    isLoading,
}: {
    title: string;
    value: number;
    colorClass?: string;
    isLoading: boolean;
}) {
    return (
        <Card>
            <CardContent className="pt-6">
                {isLoading ? (
                    <Skeleton className="h-8 w-20 mb-1" />
                ) : (
                    <div className={`text-2xl font-bold ${colorClass || ""}`}>
                        {formatNumber(value)}
                    </div>
                )}
                <p className="text-sm text-zinc-500">{title}</p>
            </CardContent>
        </Card>
    );
}

function BrandTableRow({
    brand,
    isSelected,
    onSelect,
    onEdit,
    onDelete,
    onToggleActive,
    onToggleFeatured,
}: {
    brand: Brand;
    isSelected: boolean;
    onSelect: (id: string, selected: boolean) => void;
    onEdit: (brand: Brand) => void;
    onDelete: (brand: Brand) => void;
    onToggleActive: (brand: Brand) => void;
    onToggleFeatured: (brand: Brand) => void;
}) {
    return (
        <TableRow>
            <TableCell className="w-12">
                <Checkbox
                    checked={isSelected}
                    onCheckedChange={(checked) => onSelect(brand.id, !!checked)}
                />
            </TableCell>
            <TableCell>
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-slate-100 dark:bg-slate-800 flex items-center justify-center overflow-hidden">
                        {brand.logoUrl ? (
                            <Image
                                src={brand.logoUrl}
                                alt={brand.name}
                                width={40}
                                height={40}
                                className="object-cover"
                                unoptimized
                            />
                        ) : (
                            <FontAwesomeIcon
                                icon={faTag}
                                className="w-5 h-5 text-slate-400"
                            />
                        )}
                    </div>
                    <div>
                        <p className="font-medium">{brand.name}</p>
                        <p className="text-sm text-zinc-500">{brand.slug}</p>
                    </div>
                </div>
            </TableCell>
            <TableCell className="text-zinc-500 max-w-xs truncate">
                {brand.description || "-"}
            </TableCell>
            <TableCell className="text-center">
                <Badge variant="secondary">{brand.displayOrder}</Badge>
            </TableCell>
            <TableCell className="text-center">
                <Badge variant="secondary">{brand.auctionCount || 0}</Badge>
            </TableCell>
            <TableCell>
                {brand.isFeatured && (
                    <Badge className="bg-amber-500/10 text-amber-600">
                        <Star className="w-3 h-3 mr-1 fill-current" />
                        Featured
                    </Badge>
                )}
            </TableCell>
            <TableCell>
                <Badge
                    className={
                        brand.isActive
                            ? "bg-green-500/10 text-green-600"
                            : "bg-zinc-500/10 text-zinc-600"
                    }
                >
                    {brand.isActive ? "Active" : "Inactive"}
                </Badge>
            </TableCell>
            <TableCell className="text-right">
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => onEdit(brand)}>
                            <Pencil className="h-4 w-4 mr-2" />
                            Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => onToggleFeatured(brand)}>
                            <Star className="h-4 w-4 mr-2" />
                            {brand.isFeatured ? "Unfeature" : "Feature"}
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => onToggleActive(brand)}>
                            {brand.isActive ? (
                                <>
                                    <EyeOff className="h-4 w-4 mr-2" />
                                    Deactivate
                                </>
                            ) : (
                                <>
                                    <Eye className="h-4 w-4 mr-2" />
                                    Activate
                                </>
                            )}
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                            className="text-red-600"
                            onClick={() => onDelete(brand)}
                        >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Delete
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </TableCell>
        </TableRow>
    );
}

function TableSkeleton() {
    return (
        <>
            {[...Array(5)].map((_, i) => (
                <TableRow key={i}>
                    <TableCell>
                        <Skeleton className="w-5 h-5" />
                    </TableCell>
                    <TableCell>
                        <div className="flex items-center gap-3">
                            <Skeleton className="w-10 h-10 rounded-lg" />
                            <div>
                                <Skeleton className="h-4 w-32 mb-2" />
                                <Skeleton className="h-3 w-24" />
                            </div>
                        </div>
                    </TableCell>
                    <TableCell>
                        <Skeleton className="h-4 w-40" />
                    </TableCell>
                    <TableCell className="text-center">
                        <Skeleton className="h-5 w-8 mx-auto" />
                    </TableCell>
                    <TableCell className="text-center">
                        <Skeleton className="h-5 w-8 mx-auto" />
                    </TableCell>
                    <TableCell>
                        <Skeleton className="h-5 w-16" />
                    </TableCell>
                    <TableCell>
                        <Skeleton className="h-5 w-16" />
                    </TableCell>
                    <TableCell>
                        <Skeleton className="h-8 w-8 ml-auto" />
                    </TableCell>
                </TableRow>
            ))}
        </>
    );
}

interface BrandFormData {
    name: string;
    logoUrl: string;
    description: string;
    displayOrder: number;
    isActive: boolean;
    isFeatured: boolean;
}

const initialFormData: BrandFormData = {
    name: "",
    logoUrl: "",
    description: "",
    displayOrder: 0,
    isActive: true,
    isFeatured: false,
};

export default function AdminBrandsPage() {
    const [brands, setBrands] = useState<Brand[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isBulkDeleteDialogOpen, setIsBulkDeleteDialogOpen] = useState(false);

    const [editingBrand, setEditingBrand] = useState<Brand | null>(null);
    const [deletingBrand, setDeletingBrand] = useState<Brand | null>(null);
    const [formData, setFormData] = useState<BrandFormData>(initialFormData);
    const [isSaving, setIsSaving] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    const fetchBrands = useCallback(async () => {
        try {
            const data = await brandService.getBrands();
            setBrands(data);
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
            setIsRefreshing(false);
        }
    }, []);

    useEffect(() => {
        fetchBrands();
    }, [fetchBrands]);

    const handleRefresh = useCallback(() => {
        setIsRefreshing(true);
        fetchBrands();
    }, [fetchBrands]);

    const handleOpenCreate = useCallback(() => {
        setEditingBrand(null);
        setFormData(initialFormData);
        setIsDialogOpen(true);
    }, []);

    const handleOpenEdit = useCallback((brand: Brand) => {
        setEditingBrand(brand);
        setFormData({
            name: brand.name,
            logoUrl: brand.logoUrl || "",
            description: brand.description || "",
            displayOrder: brand.displayOrder,
            isActive: brand.isActive,
            isFeatured: brand.isFeatured,
        });
        setIsDialogOpen(true);
    }, []);

    const handleSave = useCallback(async () => {
        if (!formData.name.trim()) {
            toast.error("Brand name is required");
            return;
        }

        setIsSaving(true);
        try {
            if (editingBrand) {
                const updateDto: UpdateBrandDto = {
                    name: formData.name,
                    logoUrl: formData.logoUrl || undefined,
                    description: formData.description || undefined,
                    displayOrder: formData.displayOrder,
                    isActive: formData.isActive,
                    isFeatured: formData.isFeatured,
                };
                await brandService.updateBrand(editingBrand.id, updateDto);
                toast.success("Brand updated successfully");
            } else {
                const createDto: CreateBrandDto = {
                    name: formData.name,
                    logoUrl: formData.logoUrl || undefined,
                    description: formData.description || undefined,
                    displayOrder: formData.displayOrder,
                    isFeatured: formData.isFeatured,
                };
                await brandService.createBrand(createDto);
                toast.success("Brand created successfully");
            }
            setIsDialogOpen(false);
            fetchBrands();
        } catch {
            toast.error(editingBrand ? "Failed to update brand" : "Failed to create brand");
        } finally {
            setIsSaving(false);
        }
    }, [editingBrand, formData, fetchBrands]);

    const handleDelete = useCallback(async () => {
        if (!deletingBrand) return;

        setIsDeleting(true);
        try {
            await brandService.deleteBrand(deletingBrand.id);
            toast.success("Brand deleted successfully");
            setIsDeleteDialogOpen(false);
            setDeletingBrand(null);
            fetchBrands();
        } catch {
            toast.error("Failed to delete brand");
        } finally {
            setIsDeleting(false);
        }
    }, [deletingBrand, fetchBrands]);

    const handleToggleActive = useCallback(async (brand: Brand) => {
        try {
            await brandService.updateBrand(brand.id, { isActive: !brand.isActive });
            toast.success(`Brand ${brand.isActive ? "deactivated" : "activated"}`);
            fetchBrands();
        } catch {
            toast.error("Failed to update brand");
        }
    }, [fetchBrands]);

    const handleToggleFeatured = useCallback(async (brand: Brand) => {
        try {
            await brandService.updateBrand(brand.id, { isFeatured: !brand.isFeatured });
            toast.success(`Brand ${brand.isFeatured ? "unfeatured" : "featured"}`);
            fetchBrands();
        } catch {
            toast.error("Failed to update brand");
        }
    }, [fetchBrands]);

    const handleSelectAll = useCallback((checked: boolean) => {
        if (checked) {
            setSelectedIds(new Set(filteredBrands.map((b) => b.id)));
        } else {
            setSelectedIds(new Set());
        }
    }, []);

    const handleSelectOne = useCallback((id: string, checked: boolean) => {
        setSelectedIds((prev) => {
            const next = new Set(prev);
            if (checked) {
                next.add(id);
            } else {
                next.delete(id);
            }
            return next;
        });
    }, []);

    const filteredBrands = brands.filter((brand) =>
        brand.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        brand.slug.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const stats = {
        total: brands.length,
        active: brands.filter((b) => b.isActive).length,
        featured: brands.filter((b) => b.isFeatured).length,
    };

    return (
        <AdminLayout
            title="Brand Management"
            description="Manage product brands"
        >
            <div className="space-y-6">
                <div className="grid gap-4 md:grid-cols-3">
                    <StatCard
                        title="Total Brands"
                        value={stats.total}
                        isLoading={isLoading}
                    />
                    <StatCard
                        title="Active Brands"
                        value={stats.active}
                        colorClass="text-green-600"
                        isLoading={isLoading}
                    />
                    <StatCard
                        title="Featured Brands"
                        value={stats.featured}
                        colorClass="text-amber-600"
                        isLoading={isLoading}
                    />
                </div>

                <Card>
                    <CardHeader>
                        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                            <div>
                                <CardTitle>Brands</CardTitle>
                                <CardDescription>
                                    {filteredBrands.length} brand(s) found
                                </CardDescription>
                            </div>
                            <div className="flex flex-col gap-2 sm:flex-row">
                                <div className="relative">
                                    <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-zinc-500" />
                                    <Input
                                        placeholder="Search brands..."
                                        className="pl-9 w-full sm:w-64"
                                        value={searchTerm}
                                        onChange={(e) => setSearchTerm(e.target.value)}
                                    />
                                </div>
                                <Button
                                    variant="outline"
                                    size="icon"
                                    onClick={handleRefresh}
                                    disabled={isRefreshing}
                                >
                                    <RefreshCw
                                        className={`h-4 w-4 ${isRefreshing ? "animate-spin" : ""}`}
                                    />
                                </Button>
                                <Button onClick={handleOpenCreate}>
                                    <Plus className="h-4 w-4 mr-2" />
                                    Add Brand
                                </Button>
                            </div>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div className="rounded-md border">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead className="w-12">
                                            <Checkbox
                                                checked={
                                                    filteredBrands.length > 0 &&
                                                    selectedIds.size === filteredBrands.length
                                                }
                                                onCheckedChange={handleSelectAll}
                                            />
                                        </TableHead>
                                        <TableHead>Brand</TableHead>
                                        <TableHead>Description</TableHead>
                                        <TableHead className="text-center">Order</TableHead>
                                        <TableHead className="text-center">Auctions</TableHead>
                                        <TableHead>Featured</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {isLoading ? (
                                        <TableSkeleton />
                                    ) : filteredBrands.length === 0 ? (
                                        <TableRow>
                                            <TableCell
                                                colSpan={8}
                                                className="h-32 text-center text-zinc-500"
                                            >
                                                No brands found
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        filteredBrands.map((brand) => (
                                            <BrandTableRow
                                                key={brand.id}
                                                brand={brand}
                                                isSelected={selectedIds.has(brand.id)}
                                                onSelect={handleSelectOne}
                                                onEdit={handleOpenEdit}
                                                onDelete={(b) => {
                                                    setDeletingBrand(b);
                                                    setIsDeleteDialogOpen(true);
                                                }}
                                                onToggleActive={handleToggleActive}
                                                onToggleFeatured={handleToggleFeatured}
                                            />
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        </div>
                    </CardContent>
                </Card>
            </div>

            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent className="sm:max-w-lg">
                    <DialogHeader>
                        <DialogTitle>
                            {editingBrand ? "Edit Brand" : "Create Brand"}
                        </DialogTitle>
                        <DialogDescription>
                            {editingBrand
                                ? "Update the brand details below"
                                : "Fill in the details for the new brand"}
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4 py-4">
                        <div className="space-y-2">
                            <Label htmlFor="name">Name</Label>
                            <Input
                                id="name"
                                value={formData.name}
                                onChange={(e) =>
                                    setFormData((prev) => ({ ...prev, name: e.target.value }))
                                }
                                placeholder="Brand name"
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>
                                <FontAwesomeIcon icon={faImage} className="w-3 h-3 mr-1.5" />
                                Brand Logo
                            </Label>
                            <SingleImageUpload
                                value={formData.logoUrl}
                                onChange={(url) =>
                                    setFormData((prev) => ({ ...prev, logoUrl: url }))
                                }
                                aspectRatio="square"
                                disabled={isSaving}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="description">Description</Label>
                            <Textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) =>
                                    setFormData((prev) => ({
                                        ...prev,
                                        description: e.target.value,
                                    }))
                                }
                                placeholder="Brief description of the brand"
                                rows={3}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="displayOrder">Display Order</Label>
                            <Input
                                id="displayOrder"
                                type="number"
                                min={0}
                                value={formData.displayOrder}
                                onChange={(e) =>
                                    setFormData((prev) => ({
                                        ...prev,
                                        displayOrder: parseInt(e.target.value) || 0,
                                    }))
                                }
                            />
                        </div>

                        <div className="flex items-center gap-6">
                            <div className="flex items-center gap-2">
                                <Switch
                                    id="isActive"
                                    checked={formData.isActive}
                                    onCheckedChange={(checked) =>
                                        setFormData((prev) => ({ ...prev, isActive: checked }))
                                    }
                                />
                                <Label htmlFor="isActive">Active</Label>
                            </div>
                            <div className="flex items-center gap-2">
                                <Switch
                                    id="isFeatured"
                                    checked={formData.isFeatured}
                                    onCheckedChange={(checked) =>
                                        setFormData((prev) => ({ ...prev, isFeatured: checked }))
                                    }
                                />
                                <Label htmlFor="isFeatured">Featured</Label>
                            </div>
                        </div>
                    </div>

                    <DialogFooter>
                        <Button
                            variant="outline"
                            onClick={() => setIsDialogOpen(false)}
                            disabled={isSaving}
                        >
                            Cancel
                        </Button>
                        <Button onClick={handleSave} disabled={isSaving}>
                            {isSaving && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            {editingBrand ? "Save Changes" : "Create Brand"}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Delete Brand</AlertDialogTitle>
                        <AlertDialogDescription>
                            Are you sure you want to delete &quot;{deletingBrand?.name}&quot;?
                            This action cannot be undone.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel disabled={isDeleting}>Cancel</AlertDialogCancel>
                        <AlertDialogAction
                            onClick={handleDelete}
                            disabled={isDeleting}
                            className="bg-red-600 hover:bg-red-700"
                        >
                            {isDeleting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            Delete
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>

            <AlertDialog
                open={isBulkDeleteDialogOpen}
                onOpenChange={setIsBulkDeleteDialogOpen}
            >
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Delete Selected Brands</AlertDialogTitle>
                        <AlertDialogDescription>
                            Are you sure you want to delete {selectedIds.size} brand(s)?
                            This action cannot be undone.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction className="bg-red-600 hover:bg-red-700">
                            Delete All
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </AdminLayout>
    );
}
