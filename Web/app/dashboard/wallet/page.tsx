"use client";

import { useState, useEffect, useCallback } from "react";
import {
    ArrowDownLeft,
    ArrowUpRight,
    CreditCard,
    DollarSign,
    Download,
    Plus,
    Wallet,
    Loader2,
    RefreshCw,
} from "lucide-react";

import { MESSAGES, PAGINATION } from "@/constants";
import { formatCurrency, formatRelativeTime } from "@/utils";

import { DashboardLayout } from "@/components/layout/dashboard-layout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

import { walletService, WalletBalance, WalletTransaction } from "@/services/wallet.service";

export default function WalletPage() {
    const [balance, setBalance] = useState<WalletBalance | null>(null);
    const [transactions, setTransactions] = useState<WalletTransaction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isTransactionsLoading, setIsTransactionsLoading] = useState(true);
    const [depositAmount, setDepositAmount] = useState("");
    const [withdrawAmount, setWithdrawAmount] = useState("");
    const [isDepositOpen, setIsDepositOpen] = useState(false);
    const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const availableBalance = balance?.availableBalance ?? 0;

    const fetchBalance = useCallback(async () => {
        try {
            const data = await walletService.getBalance();
            setBalance(data);
        } catch (error) {
            console.error("Failed to fetch balance:", error);
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, []);

    const fetchTransactions = useCallback(async () => {
        setIsTransactionsLoading(true);
        try {
            const data = await walletService.getTransactions({
                pageNumber: 1,
                pageSize: PAGINATION.TRANSACTIONS_PAGE_SIZE,
            });
            setTransactions(data.transactions);
        } catch (error) {
            console.error("Failed to fetch transactions:", error);
        } finally {
            setIsTransactionsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchBalance();
        fetchTransactions();
    }, [fetchBalance, fetchTransactions]);

    const handleDeposit = async () => {
        const amount = parseFloat(depositAmount);
        if (!depositAmount || amount <= 0) {
            toast.error("Please enter a valid amount");
            return;
        }
        
        setIsSubmitting(true);
        try {
            await walletService.createDeposit({ amount });
            toast.success(`Deposit of ${formatCurrency(amount)} initiated`);
            setIsDepositOpen(false);
            setDepositAmount("");
            fetchBalance();
            fetchTransactions();
        } catch (error) {
            console.error("Failed to create deposit:", error);
            toast.error("Failed to create deposit");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleWithdraw = async () => {
        const amount = parseFloat(withdrawAmount);
        if (!withdrawAmount || amount <= 0) {
            toast.error("Please enter a valid amount");
            return;
        }
        if (amount > availableBalance) {
            toast.error("Insufficient available balance");
            return;
        }
        
        setIsSubmitting(true);
        try {
            await walletService.createWithdrawal({ amount });
            toast.success(`Withdrawal of ${formatCurrency(amount)} initiated`);
            setIsWithdrawOpen(false);
            setWithdrawAmount("");
            fetchBalance();
            fetchTransactions();
        } catch (error) {
            console.error("Failed to create withdrawal:", error);
            toast.error("Failed to create withdrawal");
        } finally {
            setIsSubmitting(false);
        }
    };

    const getTransactionIcon = (type: string) => {
        switch (type) {
            case "deposit":
            case "refund":
            case "sale":
                return <ArrowDownLeft className="h-4 w-4 text-green-500" />;
            case "withdrawal":
            case "bid_hold":
            case "fee":
                return <ArrowUpRight className="h-4 w-4 text-red-500" />;
            default:
                return <DollarSign className="h-4 w-4" />;
        }
    };

    const getStatusBadge = (status: string) => {
        switch (status) {
            case "completed":
                return <Badge className="bg-green-500">Completed</Badge>;
            case "pending":
                return <Badge className="bg-amber-500">Pending</Badge>;
            case "failed":
                return <Badge variant="destructive">Failed</Badge>;
            default:
                return <Badge variant="secondary">{status}</Badge>;
        }
    };

    return (
        <DashboardLayout
            title="Wallet"
            description="Manage your funds and transactions"
        >
            <div className="space-y-6">
                {/* Balance Cards */}
                <div className="grid gap-4 md:grid-cols-3">
                    <Card>
                        <CardHeader className="pb-2">
                            <CardDescription>Total Balance</CardDescription>
                        </CardHeader>
                        <CardContent>
                            {isLoading ? (
                                <Skeleton className="h-9 w-32" />
                            ) : (
                                <div className="flex items-center gap-2">
                                    <Wallet className="h-5 w-5 text-amber-500" />
                                    <span className="text-3xl font-bold">
                                        {formatCurrency(balance?.totalBalance ?? 0)}
                                    </span>
                                </div>
                            )}
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="pb-2">
                            <CardDescription>Available Balance</CardDescription>
                        </CardHeader>
                        <CardContent>
                            {isLoading ? (
                                <Skeleton className="h-9 w-32" />
                            ) : (
                                <>
                                    <div className="flex items-center gap-2">
                                        <DollarSign className="h-5 w-5 text-green-500" />
                                        <span className="text-3xl font-bold">
                                            {formatCurrency(availableBalance)}
                                        </span>
                                    </div>
                                    <p className="text-xs text-zinc-500 mt-1">
                                        {formatCurrency(balance?.pendingHolds ?? 0)} held for pending bids
                                    </p>
                                </>
                            )}
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="pb-2">
                            <CardDescription>Quick Actions</CardDescription>
                        </CardHeader>
                        <CardContent className="flex gap-2">
                            <Dialog open={isDepositOpen} onOpenChange={setIsDepositOpen}>
                                <DialogTrigger asChild>
                                    <Button className="flex-1 bg-green-600 hover:bg-green-700">
                                        <Plus className="h-4 w-4 mr-1" />
                                        Deposit
                                    </Button>
                                </DialogTrigger>
                                <DialogContent>
                                    <DialogHeader>
                                        <DialogTitle>Deposit Funds</DialogTitle>
                                        <DialogDescription>
                                            Add funds to your wallet
                                        </DialogDescription>
                                    </DialogHeader>
                                    <div className="space-y-4 py-4">
                                        <div className="space-y-2">
                                            <Label>Amount ($)</Label>
                                            <Input
                                                type="number"
                                                placeholder="Enter amount"
                                                value={depositAmount}
                                                onChange={(e) => setDepositAmount(e.target.value)}
                                            />
                                        </div>
                                        <div className="flex gap-2 pt-2">
                                            {[100, 500, 1000, 5000].map((amount) => (
                                                <Button
                                                    key={amount}
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => setDepositAmount(amount.toString())}
                                                >
                                                    {formatCurrency(amount)}
                                                </Button>
                                            ))}
                                        </div>
                                    </div>
                                    <DialogFooter>
                                        <Button variant="outline" onClick={() => setIsDepositOpen(false)}>
                                            Cancel
                                        </Button>
                                        <Button 
                                            onClick={handleDeposit} 
                                            className="bg-green-600"
                                            disabled={isSubmitting}
                                        >
                                            {isSubmitting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                                            Deposit
                                        </Button>
                                    </DialogFooter>
                                </DialogContent>
                            </Dialog>

                            <Dialog open={isWithdrawOpen} onOpenChange={setIsWithdrawOpen}>
                                <DialogTrigger asChild>
                                    <Button variant="outline" className="flex-1">
                                        <Download className="h-4 w-4 mr-1" />
                                        Withdraw
                                    </Button>
                                </DialogTrigger>
                                <DialogContent>
                                    <DialogHeader>
                                        <DialogTitle>Withdraw Funds</DialogTitle>
                                        <DialogDescription>
                                            Transfer funds to your bank account
                                        </DialogDescription>
                                    </DialogHeader>
                                    <div className="space-y-4 py-4">
                                        <div className="p-3 bg-zinc-100 dark:bg-zinc-800 rounded-lg">
                                            <p className="text-sm text-zinc-500">Available to withdraw</p>
                                            <p className="text-2xl font-bold">
                                                {formatCurrency(availableBalance)}
                                            </p>
                                        </div>
                                        <div className="space-y-2">
                                            <Label>Amount ($)</Label>
                                            <Input
                                                type="number"
                                                placeholder="Enter amount"
                                                value={withdrawAmount}
                                                onChange={(e) => setWithdrawAmount(e.target.value)}
                                            />
                                        </div>
                                    </div>
                                    <DialogFooter>
                                        <Button
                                            variant="outline"
                                            onClick={() => setIsWithdrawOpen(false)}
                                        >
                                            Cancel
                                        </Button>
                                        <Button onClick={handleWithdraw} disabled={isSubmitting}>
                                            {isSubmitting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                                            Withdraw
                                        </Button>
                                    </DialogFooter>
                                </DialogContent>
                            </Dialog>
                        </CardContent>
                    </Card>
                </div>

                {/* Transactions */}
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between">
                        <div>
                            <CardTitle>Transaction History</CardTitle>
                            <CardDescription>
                                View all your wallet transactions
                            </CardDescription>
                        </div>
                        <Button 
                            variant="outline" 
                            size="sm"
                            onClick={() => {
                                fetchBalance();
                                fetchTransactions();
                            }}
                        >
                            <RefreshCw className="h-4 w-4 mr-2" />
                            Refresh
                        </Button>
                    </CardHeader>
                    <CardContent>
                        {isTransactionsLoading ? (
                            <div className="space-y-3">
                                {Array.from({ length: 5 }).map((_, i) => (
                                    <Skeleton key={i} className="h-12 w-full" />
                                ))}
                            </div>
                        ) : transactions.length === 0 ? (
                            <div className="text-center py-8 text-zinc-500">
                                <Wallet className="h-12 w-12 mx-auto mb-4 opacity-50" />
                                <p>{MESSAGES.EMPTY.TRANSACTIONS}</p>
                            </div>
                        ) : (
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Type</TableHead>
                                        <TableHead>Description</TableHead>
                                        <TableHead>Reference</TableHead>
                                        <TableHead>Date</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead className="text-right">Amount</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {transactions.map((txn) => (
                                        <TableRow key={txn.id}>
                                            <TableCell>
                                                <div className="flex items-center gap-2">
                                                    {getTransactionIcon(txn.type)}
                                                    <span className="capitalize">
                                                        {txn.type.replace("_", " ")}
                                                    </span>
                                                </div>
                                            </TableCell>
                                            <TableCell>{txn.description ?? "-"}</TableCell>
                                            <TableCell className="font-mono text-xs">
                                                {txn.reference ?? "-"}
                                            </TableCell>
                                            <TableCell>
                                                {formatRelativeTime(txn.createdAt)}
                                            </TableCell>
                                            <TableCell>{getStatusBadge(txn.status)}</TableCell>
                                            <TableCell
                                                className={`text-right font-medium ${
                                                    txn.amount > 0
                                                        ? "text-green-600"
                                                        : "text-red-600"
                                                }`}
                                            >
                                                {txn.amount > 0 ? "+" : ""}
                                                {formatCurrency(Math.abs(txn.amount))}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        )}
                    </CardContent>
                </Card>
            </div>
        </DashboardLayout>
    );
}
