"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCamera } from "@fortawesome/free-solid-svg-icons";

import {
  FormDescription,
  FormItem,
  FormLabel,
} from "@/components/ui/form";
import { ImageUpload } from "@/components/ui/image-upload";

import { StepProps } from "./types";

export function ImagesStep({ uploadedImages, onImagesChange }: StepProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
        <FontAwesomeIcon icon={faCamera} className="h-5 w-5 text-purple-500" />
        Photos
      </div>

      <FormItem>
        <FormLabel>Upload Images</FormLabel>
        <ImageUpload
          value={uploadedImages}
          onChange={onImagesChange}
          maxImages={10}
        />
        <FormDescription>
          Add up to 10 photos. First image will be the cover photo. Drag to reorder.
        </FormDescription>
      </FormItem>
    </div>
  );
}
