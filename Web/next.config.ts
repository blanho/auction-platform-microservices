import type { NextConfig } from "next";

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

export default nextConfig;
