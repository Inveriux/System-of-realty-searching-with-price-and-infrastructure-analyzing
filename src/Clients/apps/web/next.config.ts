import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  devIndicators: false,
  images : {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "upload.wikimedia.org",
      },
    ],
  },
  transpilePackages: [
    "@diploma/contracts",
    "@diploma/validation",
    "@diploma/api",
    "@diploma/features",
    //"@diploma/domain",
    //"@diploma/utils"
  ],
  reactCompiler: true,
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "http://localhost:5102/api/:path*",
      },
      {
        source: "/hubs/:path*",
        destination: "http://localhost:5102/hubs/:path*",
      },
      {
        source: "/seed-images/:path*",
        destination: "http://127.0.0.1:9001/:path*",
      },
    ];
  },
};

export default nextConfig;
