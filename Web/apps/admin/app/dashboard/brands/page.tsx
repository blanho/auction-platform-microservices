"use client";

import { useState } from "react";
import { useBrands, useCreateBrand } from "@repo/hooks";
import {
  Card,
  CardContent,
  Button,
  Input,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Label,
  Skeleton,
  Spinner,
} from "@repo/ui";
import { Plus, Edit, Trash2 } from "lucide-react";

export default function BrandsPage() {
  const { data: brands, isLoading } = useBrands();
  const [newBrandName, setNewBrandName] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const { mutate: createBrand, isPending } = useCreateBrand();

  function handleCreate() {
    createBrand(
      { name: newBrandName },
      {
        onSuccess: () => {
          setDialogOpen(false);
          setNewBrandName("");
        },
      }
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Brands</h1>
          <p className="mt-2 text-muted-foreground">
            Manage product brands
          </p>
        </div>
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Add Brand
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create Brand</DialogTitle>
            </DialogHeader>
            <div className="space-y-4 pt-4">
              <div className="space-y-2">
                <Label>Brand Name</Label>
                <Input
                  placeholder="Enter brand name"
                  value={newBrandName}
                  onChange={(e) => setNewBrandName(e.target.value)}
                />
              </div>
              <Button
                className="w-full"
                onClick={handleCreate}
                disabled={isPending || !newBrandName}
              >
                {isPending ? <Spinner size="sm" /> : "Create Brand"}
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-16" />
          ))}
        </div>
      ) : !brands?.length ? (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">No brands yet</p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {brands.map((brand) => (
            <Card key={brand.id}>
              <CardContent className="flex items-center justify-between p-4">
                <span className="font-medium">{brand.name}</span>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon">
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button variant="ghost" size="icon" className="text-destructive">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
