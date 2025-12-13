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
  title: "Privacy Policy | Auction Platform",
  description: "Learn about how we collect, use, and protect your data.",
};

export default function PrivacyPage() {
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
              <BreadcrumbPage>Privacy Policy</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <div className="prose dark:prose-invert max-w-none">
          <h1 className="text-3xl font-bold mb-2">Privacy Policy</h1>
          <p className="text-zinc-600 dark:text-zinc-400 mb-8">
            Last updated: December 13, 2025
          </p>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>1. Introduction</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                We respect your privacy and are committed to protecting your
                personal data. This privacy policy will inform you about how we
                look after your personal data when you visit our website and tell
                you about your privacy rights and how the law protects you.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>2. Information We Collect</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>We collect and process the following types of information:</p>
              
              <h4 className="font-semibold">Personal Information:</h4>
              <ul className="list-disc pl-6 space-y-2">
                <li>Name and contact information (email, phone, address)</li>
                <li>Account credentials</li>
                <li>Payment information</li>
                <li>Identity verification documents</li>
              </ul>

              <h4 className="font-semibold mt-4">Usage Information:</h4>
              <ul className="list-disc pl-6 space-y-2">
                <li>Browsing history on our platform</li>
                <li>Bid and purchase history</li>
                <li>Search queries</li>
                <li>Device and browser information</li>
                <li>IP address and location data</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>3. How We Use Your Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>We use your information for the following purposes:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>To provide and maintain our auction services</li>
                <li>To process transactions and send related information</li>
                <li>To send you updates about your bids and auctions</li>
                <li>To verify your identity and prevent fraud</li>
                <li>To improve our platform and user experience</li>
                <li>To communicate with you about promotions and updates</li>
                <li>To comply with legal obligations</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>4. Information Sharing</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>We may share your information with:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Other users (e.g., sellers and buyers in transactions)</li>
                <li>Payment processors to complete transactions</li>
                <li>Shipping providers to deliver items</li>
                <li>Service providers who assist our operations</li>
                <li>Law enforcement when required by law</li>
              </ul>
              <p className="mt-4">
                We do not sell your personal information to third parties.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>5. Data Security</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                We implement appropriate security measures to protect your
                personal information, including:
              </p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Encryption of data in transit and at rest</li>
                <li>Regular security assessments and audits</li>
                <li>Access controls and authentication measures</li>
                <li>Employee training on data protection</li>
                <li>Incident response procedures</li>
              </ul>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>6. Cookies and Tracking</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>We use cookies and similar technologies to:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Keep you logged in to your account</li>
                <li>Remember your preferences</li>
                <li>Analyze site traffic and usage</li>
                <li>Personalize content and advertisements</li>
              </ul>
              <p className="mt-4">
                You can control cookies through your browser settings. However,
                disabling cookies may affect your ability to use some features.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>7. Your Rights</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>You have the right to:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Access your personal data</li>
                <li>Correct inaccurate data</li>
                <li>Request deletion of your data</li>
                <li>Object to processing of your data</li>
                <li>Request data portability</li>
                <li>Withdraw consent at any time</li>
              </ul>
              <p className="mt-4">
                To exercise these rights, please contact us using the information
                below.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>8. Data Retention</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                We retain your personal data for as long as your account is
                active or as needed to provide you services. We may also retain
                and use your information as necessary to comply with legal
                obligations, resolve disputes, and enforce our agreements.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>9. Children&apos;s Privacy</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                Our services are not intended for individuals under the age of
                18. We do not knowingly collect personal information from
                children. If you believe we have collected information from a
                child, please contact us immediately.
              </p>
            </CardContent>
          </Card>

          <Card className="mb-6">
            <CardHeader>
              <CardTitle>10. Contact Us</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-zinc-700 dark:text-zinc-300">
              <p>
                If you have any questions about this Privacy Policy, please
                contact us:
              </p>
              <p>
                Email: privacy@auctionplatform.com<br />
                Address: 123 Auction Street, City, State 12345
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </MainLayout>
  );
}
