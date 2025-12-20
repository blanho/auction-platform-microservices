import { NextRequest, NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/token-cookies";

const GATEWAY_URL = process.env.GATEWAY_URL || process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:6001";

const ALLOWED_METHODS = ["GET", "POST", "PUT", "PATCH", "DELETE"];

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  return handleProxy(request, await params);
}

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  return handleProxy(request, await params);
}

export async function PUT(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  return handleProxy(request, await params);
}

export async function PATCH(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  return handleProxy(request, await params);
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
) {
  return handleProxy(request, await params);
}

async function handleProxy(
  request: NextRequest,
  params: { path: string[] }
): Promise<NextResponse> {
  const method = request.method;

  if (!ALLOWED_METHODS.includes(method)) {
    return NextResponse.json(
      { error: "Method not allowed" },
      { status: 405 }
    );
  }

  const path = params.path.join("/");
  const searchParams = request.nextUrl.searchParams.toString();
  const targetUrl = `${GATEWAY_URL}/${path}${searchParams ? `?${searchParams}` : ""}`;

  const accessToken = await getAccessToken();

  const headers: HeadersInit = {
    "Content-Type": request.headers.get("Content-Type") || "application/json",
    "X-Correlation-Id": crypto.randomUUID(),
  };

  if (accessToken) {
    headers["Authorization"] = `Bearer ${accessToken}`;
  }

  const acceptHeader = request.headers.get("Accept");
  if (acceptHeader) {
    headers["Accept"] = acceptHeader;
  }

  try {
    let body: BodyInit | null = null;
    
    if (method !== "GET" && method !== "DELETE") {
      const contentType = request.headers.get("Content-Type") || "";
      
      if (contentType.includes("multipart/form-data")) {
        body = await request.formData();
        delete (headers as Record<string, string>)["Content-Type"];
      } else if (contentType.includes("application/json")) {
        const text = await request.text();
        if (text) {
          body = text;
        }
      } else {
        body = await request.text();
      }
    }

    const response = await fetch(targetUrl, {
      method,
      headers,
      body,
    });

    const responseHeaders = new Headers();
    
    const contentType = response.headers.get("Content-Type");
    if (contentType) {
      responseHeaders.set("Content-Type", contentType);
    }

    const contentDisposition = response.headers.get("Content-Disposition");
    if (contentDisposition) {
      responseHeaders.set("Content-Disposition", contentDisposition);
    }

    if (!response.ok) {
      if (response.status === 401) {
        return NextResponse.json(
          { error: "Unauthorized", message: "Please sign in to continue" },
          { status: 401, headers: responseHeaders }
        );
      }

      const errorBody = await response.text();
      let errorJson;
      try {
        errorJson = JSON.parse(errorBody);
      } catch {
        errorJson = { error: errorBody };
      }

      return NextResponse.json(errorJson, {
        status: response.status,
        headers: responseHeaders,
      });
    }

    if (contentType?.includes("application/json")) {
      const data = await response.json();
      return NextResponse.json(data, {
        status: response.status,
        headers: responseHeaders,
      });
    }

    const blob = await response.blob();
    return new NextResponse(blob, {
      status: response.status,
      headers: responseHeaders,
    });
  } catch (error) {
    console.error("Proxy error:", error);
    return NextResponse.json(
      { error: "Internal server error", message: "Failed to connect to backend service" },
      { status: 502 }
    );
  }
}
