"use client";

import Link from "next/link";
import { useWallet, useTransactions, useDeposit, useWithdraw } from "@repo/hooks";
import { formatCurrency, formatDateTime } from "@repo/utils";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Button,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Input,
  Label,
  Skeleton,
  Badge,
} from "@repo/ui";
import { useState } from "react";
import { ArrowDownLeft, ArrowUpRight, Wallet } from "lucide-react";

export default function WalletPage() {
  const { data: wallet, isLoading: walletLoading } = useWallet();
  const { data: transactions, isLoading: transactionsLoading } = useTransactions();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Wallet</h1>
        <p className="mt-2 text-muted-foreground">
          Manage your funds and view transaction history
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Total Balance</CardTitle>
            <Wallet className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {walletLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold">
                {formatCurrency(wallet?.balance || 0)}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Available</CardTitle>
            <ArrowUpRight className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            {walletLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold text-green-600">
                {formatCurrency(wallet?.availableBalance || 0)}
              </div>
            )}
            <p className="text-xs text-muted-foreground">Can be used for bidding</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">On Hold</CardTitle>
            <ArrowDownLeft className="h-4 w-4 text-orange-500" />
          </CardHeader>
          <CardContent>
            {walletLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <div className="text-2xl font-bold text-orange-600">
                {formatCurrency(wallet?.heldBalance || 0)}
              </div>
            )}
            <p className="text-xs text-muted-foreground">Reserved for active bids</p>
          </CardContent>
        </Card>
      </div>

      <div className="flex gap-4">
        <DepositDialog />
        <WithdrawDialog availableBalance={wallet?.availableBalance || 0} />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Transaction History</CardTitle>
        </CardHeader>
        <CardContent>
          {transactionsLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-12" />
              ))}
            </div>
          ) : !transactions?.items.length ? (
            <p className="text-center text-muted-foreground py-8">
              No transactions yet
            </p>
          ) : (
            <div className="space-y-2">
              {transactions.items.map((tx) => (
                <div
                  key={tx.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div className="flex items-center gap-3">
                    <div
                      className={`flex h-8 w-8 items-center justify-center rounded-full ${
                        tx.type === "Deposit" || tx.type === "Refund"
                          ? "bg-green-100 text-green-600"
                          : "bg-red-100 text-red-600"
                      }`}
                    >
                      {tx.type === "Deposit" || tx.type === "Refund" ? (
                        <ArrowDownLeft className="h-4 w-4" />
                      ) : (
                        <ArrowUpRight className="h-4 w-4" />
                      )}
                    </div>
                    <div>
                      <p className="font-medium">{tx.type}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatDateTime(tx.createdAt)}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p
                      className={`font-semibold ${
                        tx.type === "Deposit" || tx.type === "Refund"
                          ? "text-green-600"
                          : "text-red-600"
                      }`}
                    >
                      {tx.type === "Deposit" || tx.type === "Refund" ? "+" : "-"}
                      {formatCurrency(tx.amount)}
                    </p>
                    <Badge variant="outline" className="text-xs">
                      {tx.status}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function DepositDialog() {
  const [open, setOpen] = useState(false);
  const [amount, setAmount] = useState("");
  const { mutate: deposit, isPending } = useDeposit();

  function handleDeposit() {
    deposit(
      { amount: Number(amount) },
      {
        onSuccess: () => {
          setOpen(false);
          setAmount("");
        },
      }
    );
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>Deposit Funds</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Deposit Funds</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 pt-4">
          <div className="space-y-2">
            <Label htmlFor="amount">Amount</Label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                $
              </span>
              <Input
                id="amount"
                type="number"
                min="10"
                step="0.01"
                placeholder="100.00"
                className="pl-7"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
            </div>
            <p className="text-xs text-muted-foreground">Minimum deposit: $10.00</p>
          </div>
          <Button
            className="w-full"
            onClick={handleDeposit}
            disabled={isPending || Number(amount) < 10}
          >
            {isPending ? "Processing..." : "Continue to Payment"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function WithdrawDialog({ availableBalance }: { availableBalance: number }) {
  const [open, setOpen] = useState(false);
  const [amount, setAmount] = useState("");
  const { mutate: withdraw, isPending } = useWithdraw();

  function handleWithdraw() {
    withdraw(
      { amount: Number(amount) },
      {
        onSuccess: () => {
          setOpen(false);
          setAmount("");
        },
      }
    );
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline">Withdraw</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Withdraw Funds</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 pt-4">
          <p className="text-sm text-muted-foreground">
            Available balance: {formatCurrency(availableBalance)}
          </p>
          <div className="space-y-2">
            <Label htmlFor="withdraw-amount">Amount</Label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                $
              </span>
              <Input
                id="withdraw-amount"
                type="number"
                min="1"
                max={availableBalance}
                step="0.01"
                placeholder="50.00"
                className="pl-7"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
            </div>
          </div>
          <Button
            className="w-full"
            onClick={handleWithdraw}
            disabled={
              isPending ||
              Number(amount) <= 0 ||
              Number(amount) > availableBalance
            }
          >
            {isPending ? "Processing..." : "Withdraw"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
