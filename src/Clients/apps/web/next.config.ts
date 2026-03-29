import type { NextConfig } from "next";

const nextConfig: NextConfig = {
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
        destination: "http://diploma.com:5102/api/:path*",
      },
    ];
  },
};

export default nextConfig;
