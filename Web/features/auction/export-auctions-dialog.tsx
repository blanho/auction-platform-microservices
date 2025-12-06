"use client";

import { useState } from "react";
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
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Download,
  FileJson,
  FileSpreadsheet,
  FileText,
  Loader2,
  AlertCircle
} from "lucide-react";
import { auctionService } from "@/services/auction.service";
import { ExportAuctionsRequest, AuctionStatus } from "@/types/auction";

interface ExportAuctionsDialogProps {
  defaultSeller?: string;
  trigger?: React.ReactNode;
}

type ExportFormat = "json" | "csv" | "excel";

const FORMAT_OPTIONS: { value: ExportFormat; label: string; icon: React.ReactNode }[] = [
  { value: "json", label: "JSON", icon: <FileJson className="h-4 w-4" /> },
  { value: "csv", label: "CSV", icon: <FileText className="h-4 w-4" /> },
  { value: "excel", label: "Excel", icon: <FileSpreadsheet className="h-4 w-4" /> }
];

const STATUS_OPTIONS = [
  { value: "", label: "All Statuses" },
  { value: AuctionStatus.Live, label: "Live" },
  { value: AuctionStatus.Finished, label: "Finished" },
  { value: AuctionStatus.ReserveNotMet, label: "Reserve Not Met" },
  { value: AuctionStatus.Cancelled, label: "Cancelled" },
  { value: AuctionStatus.Inactive, label: "Inactive" }
];

export function ExportAuctionsDialog({
  defaultSeller,
  trigger
}: ExportAuctionsDialogProps) {
  const [open, setOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [format, setFormat] = useState<ExportFormat>("excel");
  const [status, setStatus] = useState<string>("");
  const [seller, setSeller] = useState<string>(defaultSeller || "");
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");

  const handleExport = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const params: ExportAuctionsRequest = {
        format,
        status: status || undefined,
        seller: seller || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined
      };

      const result = await auctionService.exportAuctions(params);

      if (format === "json") {
        // Download as JSON file
        const blob = new Blob([JSON.stringify(result, null, 2)], {
          type: "application/json"
        });
        downloadBlob(blob, `auctions_export_${Date.now()}.json`);
      } else {
        // CSV or Excel - already a blob
        const extension = format === "csv" ? "csv" : "xlsx";
        downloadBlob(result as Blob, `auctions_export_${Date.now()}.${extension}`);
      }

      setOpen(false);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to export auctions";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  };

  const downloadBlob = (blob: Blob, filename: string) => {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  };

  const handleClose = () => {
    setOpen(false);
    setError(null);
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => (isOpen ? setOpen(true) : handleClose())}>
      <DialogTrigger asChild>
        {trigger || (
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Export Auctions
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Download className="h-5 w-5" />
            Export Auctions
          </DialogTitle>
          <DialogDescription>
            Export your auctions to a file with optional filters.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Format Selection */}
          <div className="space-y-2">
            <Label>Export Format</Label>
            <div className="flex gap-2">
              {FORMAT_OPTIONS.map((option) => (
                <Button
                  key={option.value}
                  variant={format === option.value ? "default" : "outline"}
                  size="sm"
                  className="flex-1"
                  onClick={() => setFormat(option.value)}
                >
                  {option.icon}
                  <span className="ml-2">{option.label}</span>
                </Button>
              ))}
            </div>
          </div>

          {/* Filters */}
          <div className="space-y-3">
            <div className="space-y-2">
              <Label htmlFor="status">Status Filter</Label>
              <Select value={status} onValueChange={setStatus}>
                <SelectTrigger>
                  <SelectValue placeholder="All Statuses" />
                </SelectTrigger>
                <SelectContent>
                  {STATUS_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="seller">Seller Filter</Label>
              <Input
                id="seller"
                placeholder="Filter by seller username"
                value={seller}
                onChange={(e) => setSeller(e.target.value)}
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label htmlFor="startDate">Start Date</Label>
                <Input
                  id="startDate"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="endDate">End Date</Label>
                <Input
                  id="endDate"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </div>
            </div>
          </div>

          {/* Error */}
          {error && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button onClick={handleExport} disabled={isLoading}>
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Exporting...
              </>
            ) : (
              <>
                <Download className="mr-2 h-4 w-4" />
                Export
              </>
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
