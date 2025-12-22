import { Metadata } from "next";

import { MainLayout } from "@/components/layout/main-layout";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ROUTES } from "@/constants/routes";

export const metadata: Metadata = {
  title: "Terms of Service | Auction Platform",
  description: "Read our terms of service and user agreement.",
};

export default function TermsPage() {
  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto space-y-6">
        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink href={ROUTES.HOME}>Home</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>Terms of Service</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <div className="prose dark:prose-invert max-w-none">
          <h1 className="text-3xl font-bold mb-2">Terms of Service</h1>
          <p className="text-zinc-600 dark:text-zinc-400 mb-8">
            Last updated: December 13, 2025
          </p>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>1. Acceptance of Terms</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                By accessing and using this auction platform, you accept and agree
                to be bound by the terms and provision of this agreement.
                Additionally, when using this platform&apos;s particular services, you
                shall be subject to any posted guidelines or rules applicable to
                such services.
              </p>
              <p>
                If you do not agree to abide by these Terms of Service, please do
                not use this service.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>2. User Registration</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>To use certain features of the platform, you must register for an account. When you register, you agree to:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Provide accurate, current, and complete information</li>
                <li>Maintain and promptly update your account information</li>
                <li>Keep your password confidential and secure</li>
                <li>Accept responsibility for all activities under your account</li>
                <li>Notify us immediately of any unauthorized use</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>3. Auction Rules</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <h4 className="font-semibold">For Bidders:</h4>
              <ul className="list-disc pl-6 space-y-2">
                <li>All bids are binding contracts to purchase</li>
                <li>You may not retract a bid except in exceptional circumstances</li>
                <li>Bid sniping and bid shielding are prohibited</li>
                <li>You must complete the purchase if you win</li>
                <li>Payment must be made within the specified timeframe</li>
              </ul>

              <h4 className="font-semibold mt-4">For Sellers:</h4>
              <ul className="list-disc pl-6 space-y-2">
                <li>You must have the legal right to sell the item</li>
                <li>Item descriptions must be accurate and complete</li>
                <li>You must honor the winning bid</li>
                <li>Items must be shipped within the specified timeframe</li>
                <li>Prohibited items may not be listed</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>4. Fees and Payments</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>The platform charges the following fees:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Seller commission: 5% of the final sale price</li>
                <li>Buyer&apos;s premium: 5% of the winning bid</li>
                <li>Listing fees may apply for featured listings</li>
                <li>Payment processing fees may apply</li>
              </ul>
              <p>
                All fees are subject to change with notice. Current fee schedules
                are available in your account settings.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>5. Prohibited Items</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>The following items may not be listed on the platform:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Illegal items or items that promote illegal activity</li>
                <li>Stolen property</li>
                <li>Counterfeit or replica items</li>
                <li>Weapons and ammunition</li>
                <li>Hazardous materials</li>
                <li>Items that infringe intellectual property rights</li>
                <li>Adult content</li>
                <li>Live animals</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>6. Dispute Resolution</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                In case of disputes between buyers and sellers, the platform
                provides a dispute resolution service. Both parties agree to:
              </p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Attempt to resolve disputes directly first</li>
                <li>Use the platform&apos;s dispute resolution service if needed</li>
                <li>Provide accurate information and documentation</li>
                <li>Accept the platform&apos;s decision as final</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>7. Limitation of Liability</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                The platform acts as a venue for buyers and sellers to transact.
                We are not responsible for:
              </p>
              <ul className="list-disc pl-6 space-y-2">
                <li>The quality, safety, or legality of items listed</li>
                <li>The accuracy of listings</li>
                <li>The ability of sellers to sell items</li>
                <li>The ability of buyers to pay for items</li>
                <li>Any disputes between users</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>8. Contact Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                If you have any questions about these Terms of Service, please
                contact us at:
              </p>
              <p>
                Email: support@auctionplatform.com<br />
                Address: 123 Auction Street, City, State 12345
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </MainLayout>
  );
}
