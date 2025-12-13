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
    FolderTree,
    Package,
} from "lucide-react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faBox,
    faCar,
    faGem,
    faPalette,
    faMusic,
    faGamepad,
    faBook,
    faHome,
    faTshirt,
    faFootball,
    faLaptop,
    faCamera,
    faRing,
    faWineGlass,
    faCouch,
} from "@fortawesome/free-solid-svg-icons";
import type { IconDefinition } from "@fortawesome/fontawesome-svg-core";

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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

import {
    adminCategoryService,
    CreateCategoryDto,
    UpdateCategoryDto,
    CategoryStats,
} from "@/services/admin-category.service";
import { Category } from "@/types/auction";

const ICON_OPTIONS: { value: string; label: string; icon: IconDefinition }[] = [
    { value: "fa-box", label: "Box", icon: faBox },
    { value: "fa-car", label: "Car", icon: faCar },
    { value: "fa-gem", label: "Gem", icon: faGem },
    { value: "fa-palette", label: "Art", icon: faPalette },
    { value: "fa-music", label: "Music", icon: faMusic },
    { value: "fa-gamepad", label: "Gaming", icon: faGamepad },
    { value: "fa-book", label: "Books", icon: faBook },
    { value: "fa-home", label: "Home", icon: faHome },
    { value: "fa-tshirt", label: "Fashion", icon: faTshirt },
    { value: "fa-football", label: "Sports", icon: faFootball },
    { value: "fa-laptop", label: "Electronics", icon: faLaptop },
    { value: "fa-camera", label: "Camera", icon: faCamera },
    { value: "fa-ring", label: "Jewelry", icon: faRing },
    { value: "fa-wine-glass", label: "Wine", icon: faWineGlass },
    { value: "fa-couch", label: "Furniture", icon: faCouch },
];

function getIconComponent(iconName: string): IconDefinition {
    const found = ICON_OPTIONS.find((opt) => opt.value === iconName);
    return found?.icon || faBox;
}

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

function CategoryTableRow({
    category,
    allCategories,
    onEdit,
    onDelete,
    onToggleActive,
}: {
    category: Category;
    allCategories: Category[];
    onEdit: (category: Category) => void;
    onDelete: (category: Category) => void;
    onToggleActive: (category: Category) => void;
}) {
    const parentCategory = category.parentCategoryId
        ? allCategories.find((c) => c.id === category.parentCategoryId)
        : null;

    return (
        <TableRow>
            <TableCell>
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-purple-500/10 flex items-center justify-center">
                        <FontAwesomeIcon
                            icon={getIconComponent(category.icon)}
                            className="w-5 h-5 text-purple-600"
                        />
                    </div>
                    <div>
                        <p className="font-medium">{category.name}</p>
                        <p className="text-sm text-zinc-500">{category.slug}</p>
                    </div>
                </div>
            </TableCell>
            <TableCell className="text-zinc-500 max-w-xs truncate">
                {category.description || "-"}
            </TableCell>
            <TableCell>
                {parentCategory ? (
                    <Badge variant="outline">{parentCategory.name}</Badge>
                ) : (
                    <span className="text-zinc-400">-</span>
                )}
            </TableCell>
            <TableCell className="text-center">
                <Badge variant="secondary">{category.displayOrder}</Badge>
            </TableCell>
            <TableCell className="text-center">
                <Badge variant="secondary">{category.auctionCount}</Badge>
            </TableCell>
            <TableCell>
                <Badge
                    className={
                        category.isActive
                            ? "bg-green-500/10 text-green-600"
                            : "bg-zinc-500/10 text-zinc-600"
                    }
                >
                    {category.isActive ? "Active" : "Inactive"}
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
                        <DropdownMenuItem onClick={() => onEdit(category)}>
                            <Pencil className="h-4 w-4 mr-2" />
                            Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => onToggleActive(category)}>
                            {category.isActive ? (
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
                            onClick={() => onDelete(category)}
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
                    <TableCell>
                        <Skeleton className="h-5 w-20" />
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
                        <Skeleton className="h-8 w-8 ml-auto" />
                    </TableCell>
                </TableRow>
            ))}
        </>
    );
}

interface CategoryFormData {
    name: string;
    slug: string;
    icon: string;
    description: string;
    imageUrl: string;
    displayOrder: number;
    isActive: boolean;
    parentCategoryId: string | null;
}

const initialFormData: CategoryFormData = {
    name: "",
    slug: "",
    icon: "fa-box",
    description: "",
    imageUrl: "",
    displayOrder: 0,
    isActive: true,
    parentCategoryId: null,
};

export default function AdminCategoriesPage() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [stats, setStats] = useState<CategoryStats | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [editingCategory, setEditingCategory] = useState<Category | null>(null);
    const [deletingCategory, setDeletingCategory] = useState<Category | null>(null);
    const [formData, setFormData] = useState<CategoryFormData>(initialFormData);
    const [isSaving, setIsSaving] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    const fetchData = useCallback(async (showRefresh = false) => {
        if (showRefresh) {
            setIsRefreshing(true);
        } else {
            setIsLoading(true);
        }

        try {
            const [categoriesData, statsData] = await Promise.all([
                adminCategoryService.getAllCategories(false),
                adminCategoryService.getStats(),
            ]);

            setCategories(categoriesData);
            setStats(statsData);
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
            setIsRefreshing(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleRefresh = () => {
        fetchData(true);
    };

    const filteredCategories = categories.filter(
        (category) =>
            category.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
            category.slug.toLowerCase().includes(searchTerm.toLowerCase()) ||
            category.description?.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const handleOpenCreateDialog = () => {
        setEditingCategory(null);
        setFormData(initialFormData);
        setIsDialogOpen(true);
    };

    const handleOpenEditDialog = (category: Category) => {
        setEditingCategory(category);
        setFormData({
            name: category.name,
            slug: category.slug,
            icon: category.icon,
            description: category.description || "",
            imageUrl: category.imageUrl || "",
            displayOrder: category.displayOrder,
            isActive: category.isActive,
            parentCategoryId: category.parentCategoryId || null,
        });
        setIsDialogOpen(true);
    };

    const handleOpenDeleteDialog = (category: Category) => {
        setDeletingCategory(category);
        setIsDeleteDialogOpen(true);
    };

    const handleToggleActive = async (category: Category) => {
        try {
            const updateDto: UpdateCategoryDto = {
                name: category.name,
                slug: category.slug,
                icon: category.icon,
                description: category.description,
                imageUrl: category.imageUrl,
                displayOrder: category.displayOrder,
                isActive: !category.isActive,
                parentCategoryId: category.parentCategoryId,
            };

            await adminCategoryService.updateCategory(category.id, updateDto);
            toast.success(
                `Category ${category.isActive ? "deactivated" : "activated"} successfully`
            );
            fetchData(true);
        } catch {
            toast.error("Failed to update category status");
        }
    };

    const generateSlug = (name: string) => {
        return name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-|-$/g, "");
    };

    const handleNameChange = (name: string) => {
        setFormData((prev) => ({
            ...prev,
            name,
            slug: editingCategory ? prev.slug : generateSlug(name),
        }));
    };

    const handleSave = async () => {
        if (!formData.name.trim() || !formData.slug.trim()) {
            toast.error("Name and slug are required");
            return;
        }

        setIsSaving(true);
        try {
            if (editingCategory) {
                const updateDto: UpdateCategoryDto = {
                    name: formData.name,
                    slug: formData.slug,
                    icon: formData.icon,
                    description: formData.description || undefined,
                    imageUrl: formData.imageUrl || undefined,
                    displayOrder: formData.displayOrder,
                    isActive: formData.isActive,
                    parentCategoryId: formData.parentCategoryId,
                };
                await adminCategoryService.updateCategory(editingCategory.id, updateDto);
                toast.success("Category updated successfully");
            } else {
                const createDto: CreateCategoryDto = {
                    name: formData.name,
                    slug: formData.slug,
                    icon: formData.icon,
                    description: formData.description || undefined,
                    imageUrl: formData.imageUrl || undefined,
                    displayOrder: formData.displayOrder,
                    isActive: formData.isActive,
                    parentCategoryId: formData.parentCategoryId,
                };
                await adminCategoryService.createCategory(createDto);
                toast.success("Category created successfully");
            }

            setIsDialogOpen(false);
            fetchData(true);
        } catch {
            toast.error(
                editingCategory
                    ? "Failed to update category"
                    : "Failed to create category"
            );
        } finally {
            setIsSaving(false);
        }
    };

    const handleDelete = async () => {
        if (!deletingCategory) return;

        setIsDeleting(true);
        try {
            await adminCategoryService.deleteCategory(deletingCategory.id);
            toast.success("Category deleted successfully");
            setIsDeleteDialogOpen(false);
            setDeletingCategory(null);
            fetchData(true);
        } catch {
            toast.error(
                "Failed to delete category. It may have associated items."
            );
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <AdminLayout
            title="Category Management"
            description="Create, edit, and manage auction categories"
        >
            <div className="space-y-6">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <StatCard
                        title="Total Categories"
                        value={stats?.totalCategories || 0}
                        isLoading={isLoading}
                    />
                    <StatCard
                        title="Active"
                        value={stats?.activeCategories || 0}
                        colorClass="text-green-600"
                        isLoading={isLoading}
                    />
                    <StatCard
                        title="Inactive"
                        value={stats?.inactiveCategories || 0}
                        colorClass="text-zinc-500"
                        isLoading={isLoading}
                    />
                    <StatCard
                        title="With Items"
                        value={stats?.categoriesWithItems || 0}
                        colorClass="text-purple-600"
                        isLoading={isLoading}
                    />
                </div>

                <Card>
                    <CardHeader>
                        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                            <div>
                                <CardTitle className="flex items-center gap-2">
                                    <FolderTree className="h-5 w-5" />
                                    Categories
                                </CardTitle>
                                <CardDescription>
                                    {filteredCategories.length} categories found
                                </CardDescription>
                            </div>
                            <div className="flex items-center gap-2">
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
                                <Button onClick={handleOpenCreateDialog}>
                                    <Plus className="h-4 w-4 mr-2" />
                                    Add Category
                                </Button>
                            </div>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div className="flex items-center gap-4 mb-6">
                            <div className="relative flex-1 max-w-sm">
                                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                <Input
                                    placeholder="Search categories..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pl-10"
                                />
                            </div>
                        </div>

                        <div className="rounded-lg border">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Category</TableHead>
                                        <TableHead>Description</TableHead>
                                        <TableHead>Parent</TableHead>
                                        <TableHead className="text-center">Order</TableHead>
                                        <TableHead className="text-center">Items</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {isLoading ? (
                                        <TableSkeleton />
                                    ) : filteredCategories.length === 0 ? (
                                        <TableRow>
                                            <TableCell
                                                colSpan={7}
                                                className="text-center py-8 text-zinc-500"
                                            >
                                                <Package className="h-12 w-12 mx-auto mb-4 text-zinc-300" />
                                                <p>No categories found</p>
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        filteredCategories.map((category) => (
                                            <CategoryTableRow
                                                key={category.id}
                                                category={category}
                                                allCategories={categories}
                                                onEdit={handleOpenEditDialog}
                                                onDelete={handleOpenDeleteDialog}
                                                onToggleActive={handleToggleActive}
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
                            {editingCategory ? "Edit Category" : "Create Category"}
                        </DialogTitle>
                        <DialogDescription>
                            {editingCategory
                                ? "Update the category details below"
                                : "Fill in the details for the new category"}
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4 py-4">
                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <Label htmlFor="name">Name</Label>
                                <Input
                                    id="name"
                                    value={formData.name}
                                    onChange={(e) => handleNameChange(e.target.value)}
                                    placeholder="Category name"
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="slug">Slug</Label>
                                <Input
                                    id="slug"
                                    value={formData.slug}
                                    onChange={(e) =>
                                        setFormData((prev) => ({ ...prev, slug: e.target.value }))
                                    }
                                    placeholder="category-slug"
                                />
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <Label htmlFor="icon">Icon</Label>
                                <Select
                                    value={formData.icon}
                                    onValueChange={(value) =>
                                        setFormData((prev) => ({ ...prev, icon: value }))
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue>
                                            <div className="flex items-center gap-2">
                                                <FontAwesomeIcon
                                                    icon={getIconComponent(formData.icon)}
                                                    className="w-4 h-4"
                                                />
                                                <span>
                                                    {ICON_OPTIONS.find((o) => o.value === formData.icon)
                                                        ?.label || "Select icon"}
                                                </span>
                                            </div>
                                        </SelectValue>
                                    </SelectTrigger>
                                    <SelectContent>
                                        {ICON_OPTIONS.map((option) => (
                                            <SelectItem key={option.value} value={option.value}>
                                                <div className="flex items-center gap-2">
                                                    <FontAwesomeIcon
                                                        icon={option.icon}
                                                        className="w-4 h-4"
                                                    />
                                                    <span>{option.label}</span>
                                                </div>
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
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
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="parentCategory">Parent Category</Label>
                            <Select
                                value={formData.parentCategoryId || "none"}
                                onValueChange={(value) =>
                                    setFormData((prev) => ({
                                        ...prev,
                                        parentCategoryId: value === "none" ? null : value,
                                    }))
                                }
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="None (top-level)" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="none">None (top-level)</SelectItem>
                                    {categories
                                        .filter((c) => c.id !== editingCategory?.id)
                                        .map((category) => (
                                            <SelectItem key={category.id} value={category.id}>
                                                {category.name}
                                            </SelectItem>
                                        ))}
                                </SelectContent>
                            </Select>
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
                                placeholder="Brief description of the category"
                                rows={3}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="imageUrl">Image URL</Label>
                            <Input
                                id="imageUrl"
                                value={formData.imageUrl}
                                onChange={(e) =>
                                    setFormData((prev) => ({ ...prev, imageUrl: e.target.value }))
                                }
                                placeholder="https://example.com/image.jpg"
                            />
                        </div>

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
                            {editingCategory ? "Save Changes" : "Create Category"}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Delete Category</AlertDialogTitle>
                        <AlertDialogDescription>
                            Are you sure you want to delete &quot;{deletingCategory?.name}&quot;?
                            This action cannot be undone.
                            {deletingCategory && deletingCategory.auctionCount > 0 && (
                                <span className="block mt-2 text-amber-600">
                                    Warning: This category has {deletingCategory.auctionCount}{" "}
                                    associated items.
                                </span>
                            )}
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel disabled={isDeleting}>Cancel</AlertDialogCancel>
                        <AlertDialogAction
                            onClick={handleDelete}
                            disabled={isDeleting}
                            className="bg-red-600 hover:bg-red-700"
                        >
                            {isDeleting && (
                                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                            )}
                            Delete
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </AdminLayout>
    );
}
