import { Metadata } from "next";

import Link from "next/link";
import {
  Users,
  Shield,
  Award,
  Globe,
  Zap,
  Heart,
  Target,
  TrendingUp,
} from "lucide-react";

import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { ROUTES } from "@/constants/routes";
import { PLATFORM_STATS, PLATFORM_VALUES, LEADERSHIP_TEAM } from "@/constants/platform";

export const metadata: Metadata = {
  title: "About Us | Auction Platform",
  description: "Learn about our mission, values, and the team behind the platform.",
};

const ICON_MAP = {
  Users,
  Shield,
  Award,
  Globe,
  Zap,
  Heart,
  Target,
  TrendingUp,
} as const;

export default function AboutPage() {
  return (
    <MainLayout>
      <div className="space-y-16">
        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink href={ROUTES.HOME}>Home</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>About Us</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <section className="text-center max-w-3xl mx-auto">
          <h1 className="text-4xl md:text-5xl font-bold text-zinc-900 dark:text-white mb-6">
            Connecting Buyers and Sellers{" "}
            <span className="text-amber-500">Worldwide</span>
          </h1>
          <p className="text-lg text-zinc-600 dark:text-zinc-400 mb-8">
            We&apos;re building the most trusted and innovative auction platform,
            making it easy for anyone to buy and sell unique items from around
            the world.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <Button asChild size="lg" className="bg-amber-500 hover:bg-amber-600">
              <Link href={ROUTES.AUCTIONS.LIST}>Browse Auctions</Link>
            </Button>
            <Button asChild variant="outline" size="lg">
              <Link href={ROUTES.AUCTIONS.CREATE}>Start Selling</Link>
            </Button>
          </div>
        </section>

        <section className="grid grid-cols-2 md:grid-cols-4 gap-6">
          {PLATFORM_STATS.map((stat) => {
            const Icon = ICON_MAP[stat.iconName];
            return (
              <Card key={stat.label} className="text-center">
                <CardContent className="pt-6">
                  <div className="w-12 h-12 rounded-full bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center mx-auto mb-4">
                    <Icon className="h-6 w-6 text-amber-600" />
                  </div>
                  <p className="text-3xl font-bold text-zinc-900 dark:text-white mb-1">
                    {stat.value}
                  </p>
                  <p className="text-sm text-zinc-600 dark:text-zinc-400">
                    {stat.label}
                  </p>
                </CardContent>
              </Card>
            );
          })}
        </section>

        <section>
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-zinc-900 dark:text-white mb-4">
              Our Mission
            </h2>
            <p className="text-lg text-zinc-600 dark:text-zinc-400 max-w-2xl mx-auto">
              To democratize access to unique and valuable items by creating a
              trusted, transparent, and engaging auction experience for everyone.
            </p>
          </div>

          <div className="grid md:grid-cols-2 gap-6">
            {PLATFORM_VALUES.map((value) => {
              const Icon = ICON_MAP[value.iconName];
              return (
                <Card key={value.title}>
                  <CardHeader>
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 rounded-xl bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
                        <Icon className="h-6 w-6 text-amber-600" />
                      </div>
                      <CardTitle>{value.title}</CardTitle>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <CardDescription className="text-base">
                      {value.description}
                    </CardDescription>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </section>

        <section className="bg-zinc-50 dark:bg-zinc-900 rounded-2xl p-8 md:p-12">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-zinc-900 dark:text-white mb-4">
              Our Story
            </h2>
          </div>
          <div className="max-w-3xl mx-auto space-y-6 text-zinc-700 dark:text-zinc-300">
            <p>
              Founded in 2020, our auction platform was born from a simple idea:
              make buying and selling unique items as easy and trustworthy as
              possible. We saw the potential to transform the traditional auction
              experience into something accessible to everyone, anywhere in the
              world.
            </p>
            <p>
              Starting with just a handful of categories and a small team of
              passionate individuals, we&apos;ve grown to become one of the most
              trusted online auction platforms. Today, we facilitate thousands of
              auctions daily, connecting collectors, enthusiasts, and everyday
              buyers with sellers from around the globe.
            </p>
            <p>
              Our commitment to innovation drives us to continuously improve. From
              real-time bidding technology to our robust buyer protection program,
              every feature we build is designed to make your experience better.
            </p>
            <p>
              We&apos;re proud of what we&apos;ve built, but we&apos;re even more
              excited about the future. With your continued support and feedback,
              we&apos;re committed to making online auctions more accessible,
              secure, and enjoyable for everyone.
            </p>
          </div>
        </section>

        <section>
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-zinc-900 dark:text-white mb-4">
              Leadership Team
            </h2>
            <p className="text-lg text-zinc-600 dark:text-zinc-400 max-w-2xl mx-auto">
              Meet the people driving our mission forward
            </p>
          </div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {LEADERSHIP_TEAM.map((member) => (
              <Card key={member.name} className="text-center">
                <CardContent className="pt-6">
                  <div className="w-20 h-20 rounded-full bg-zinc-200 dark:bg-zinc-800 mx-auto mb-4 flex items-center justify-center">
                    <Users className="h-10 w-10 text-zinc-400" />
                  </div>
                  <h3 className="font-semibold text-zinc-900 dark:text-white">
                    {member.name}
                  </h3>
                  <p className="text-amber-500 text-sm mb-2">{member.role}</p>
                  <p className="text-sm text-zinc-600 dark:text-zinc-400">
                    {member.bio}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        <section className="bg-amber-500 rounded-2xl p-8 md:p-12 text-center">
          <h2 className="text-3xl font-bold text-white mb-4">
            Ready to Join Our Community?
          </h2>
          <p className="text-amber-100 mb-8 max-w-2xl mx-auto">
            Whether you&apos;re looking to find unique treasures or sell your
            prized possessions, our platform is here to help you succeed.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <Button
              asChild
              size="lg"
              variant="secondary"
              className="bg-white text-amber-600 hover:bg-zinc-100"
            >
              <Link href={ROUTES.AUTH.REGISTER}>Create Account</Link>
            </Button>
            <Button
              asChild
              size="lg"
              variant="outline"
              className="border-white text-white hover:bg-amber-600"
            >
              <Link href={ROUTES.HELP}>Learn More</Link>
            </Button>
          </div>
        </section>
      </div>
    </MainLayout>
  );
}
