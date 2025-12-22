import type { NextConfig } from "next";
import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin("./i18n/request.ts");

const nextConfig: NextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "**"
      },
      {
        protocol: "http",
        hostname: "**"
      }
    ]
  },
  logging: {
    fetches: {
      fullUrl: true
    }
  },
  output: "standalone",
  turbopack: {
    root: ".."
  }
};

export default withNextIntl(nextConfig);
