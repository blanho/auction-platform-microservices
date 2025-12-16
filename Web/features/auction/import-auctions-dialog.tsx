"use client";

import { useState, useRef } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faUpload,
  faFileExcel,
  faDownload,
  faSpinner,
  faCircleCheck,
  faCircleXmark,
  faCircleExclamation
} from "@fortawesome/free-solid-svg-icons";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { auctionService } from "@/services/auction.service";
import { ImportAuctionsResultDto } from "@/types/auction";
import { cn } from "@/lib/utils";

interface ImportAuctionsDialogProps {
  onSuccess?: () => void;
  trigger?: React.ReactNode;
}

export function ImportAuctionsDialog({
  onSuccess,
  trigger
}: ImportAuctionsDialogProps) {
  const [open, setOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<ImportAuctionsResultDto | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      const validTypes = [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel"
      ];
      if (!validTypes.includes(file.type) && !file.name.match(/\.xlsx?$/i)) {
        setError("Please select a valid Excel file (.xlsx or .xls)");
        return;
      }
      setSelectedFile(file);
      setError(null);
      setResult(null);
    }
  };

  const handleDownloadTemplate = async () => {
    setIsDownloading(true);
    try {
      const blob = await auctionService.downloadImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "auction_import_template.xlsx";
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      setError("Failed to download template");
    } finally {
      setIsDownloading(false);
    }
  };

  const handleImport = async () => {
    if (!selectedFile) return;

    setIsLoading(true);
    setError(null);

    try {
      const importResult = await auctionService.importFromExcel(selectedFile);
      setResult(importResult);

      if (importResult.successCount > 0) {
        onSuccess?.();
      }
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to import auctions";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setOpen(false);
    setSelectedFile(null);
    setError(null);
    setResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => (isOpen ? setOpen(true) : handleClose())}>
      <DialogTrigger asChild>
        {trigger || (
          <Button variant="outline">
            <FontAwesomeIcon icon={faUpload} className="mr-2 h-4 w-4" />
            Import Auctions
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FontAwesomeIcon icon={faFileExcel} className="h-5 w-5" />
            Import Auctions
          </DialogTitle>
          <DialogDescription>
            Import auctions from an Excel file. Download the template to see the required format.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Template Download */}
          <div className="flex items-center justify-between rounded-lg border p-3 bg-muted/50">
            <div className="flex items-center gap-2">
              <FontAwesomeIcon icon={faFileExcel} className="h-4 w-4 text-green-600" />
              <span className="text-sm">Download import template</span>
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={handleDownloadTemplate}
              disabled={isDownloading}
            >
              {isDownloading ? (
                <FontAwesomeIcon icon={faSpinner} className="h-4 w-4 animate-spin" />
              ) : (
                <>
                  <FontAwesomeIcon icon={faDownload} className="mr-2 h-4 w-4" />
                  Template
                </>
              )}
            </Button>
          </div>

          {/* File Upload */}
          <div
            className={cn(
              "border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors",
              selectedFile
                ? "border-green-500 bg-green-50 dark:bg-green-950/20"
                : "border-muted-foreground/25 hover:border-muted-foreground/50"
            )}
            onClick={() => fileInputRef.current?.click()}
          >
            <input
              type="file"
              ref={fileInputRef}
              className="hidden"
              accept=".xlsx,.xls"
              onChange={handleFileSelect}
            />
            {selectedFile ? (
              <div className="flex flex-col items-center gap-2">
                <FontAwesomeIcon icon={faFileExcel} className="h-10 w-10 text-green-600" />
                <p className="font-medium">{selectedFile.name}</p>
                <p className="text-xs text-muted-foreground">
                  {(selectedFile.size / 1024).toFixed(1)} KB
                </p>
              </div>
            ) : (
              <div className="flex flex-col items-center gap-2">
                <FontAwesomeIcon icon={faUpload} className="h-10 w-10 text-muted-foreground" />
                <p className="text-sm text-muted-foreground">
                  Click to select or drag an Excel file
                </p>
              </div>
            )}
          </div>

          {/* Error */}
          {error && (
            <Alert variant="destructive">
              <FontAwesomeIcon icon={faCircleExclamation} className="h-4 w-4" />
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          {/* Results */}
          {result && (
            <div className="space-y-3">
              <div className="flex items-center gap-4">
                <Badge variant="outline" className="text-sm">
                  Total: {result.totalRows}
                </Badge>
                <Badge className="bg-green-500 text-sm">
                  <FontAwesomeIcon icon={faCircleCheck} className="mr-1 h-3 w-3" />
                  Success: {result.successCount}
                </Badge>
                {result.failureCount > 0 && (
                  <Badge variant="destructive" className="text-sm">
                    <FontAwesomeIcon icon={faCircleXmark} className="mr-1 h-3 w-3" />
                    Failed: {result.failureCount}
                  </Badge>
                )}
              </div>

              {result.results.some((r) => !r.success) && (
                <ScrollArea className="h-32 rounded-md border p-2">
                  <div className="space-y-1">
                    {result.results
                      .filter((r) => !r.success)
                      .map((r) => (
                        <div
                          key={r.rowNumber}
                          className="text-xs text-red-600 flex items-start gap-1"
                        >
                          <span className="font-medium">Row {r.rowNumber}:</span>
                          <span>{r.error}</span>
                        </div>
                      ))}
                  </div>
                </ScrollArea>
              )}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            {result ? "Close" : "Cancel"}
          </Button>
          {!result && (
            <Button onClick={handleImport} disabled={!selectedFile || isLoading}>
              {isLoading ? (
                <>
                  <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                  Importing...
                </>
              ) : (
                <>
                  <FontAwesomeIcon icon={faUpload} className="mr-2 h-4 w-4" />
                  Import
                </>
              )}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
