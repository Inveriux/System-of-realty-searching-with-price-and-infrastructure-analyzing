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
};

export default nextConfig;
