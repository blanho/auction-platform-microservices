import { NextResponse } from "next/server";
import { clearTokenCookie } from "@/lib/auth/token-cookies";

export async function DELETE() {
  await clearTokenCookie();
  
  return NextResponse.json({ success: true });
}

export async function POST() {
  await clearTokenCookie();
  
  return NextResponse.json({ success: true });
}
