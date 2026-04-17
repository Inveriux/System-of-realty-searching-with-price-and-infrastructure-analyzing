import { NextRequest, NextResponse } from "next/server";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5102";

type InfrastructureEnvelope<T> = {
  isSuccess?: boolean;
  message?: string | null;
  data?: T;
  errors?: Array<{
    message?: string | null;
    errorMessage?: string | null;
    propertyName?: string | null;
  }>;
};

export async function POST(request: NextRequest) {
  const body = await request.text();

  const upstreamResponse = await fetch(`${API_BASE_URL}/api/locations/resolve`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body,
    cache: "no-store",
  });

  const responseText = await upstreamResponse.text();

  if (!upstreamResponse.ok) {
    return new NextResponse(responseText, {
      status: upstreamResponse.status,
      headers: {
        "Content-Type": upstreamResponse.headers.get("content-type") || "application/json; charset=utf-8",
      },
    });
  }

  if (!responseText) {
    return NextResponse.json({}, { status: 200 });
  }

  const payload = JSON.parse(responseText) as InfrastructureEnvelope<unknown>;
  return NextResponse.json(payload.data ?? payload, { status: 200 });
}
