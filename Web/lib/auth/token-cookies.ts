import { cookies } from "next/headers";
import { SignJWT, jwtVerify } from "jose";

const TOKEN_COOKIE_NAME = "auth-tokens";
const COOKIE_MAX_AGE = 60 * 60 * 24 * 7; // 7 days

const getSecretKey = () => {
  const secret = process.env.TOKEN_ENCRYPTION_SECRET || process.env.NEXTAUTH_SECRET;
  if (!secret) {
    throw new Error("TOKEN_ENCRYPTION_SECRET or NEXTAUTH_SECRET must be set");
  }
  return new TextEncoder().encode(secret);
};

export interface StoredTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export async function encryptTokens(tokens: StoredTokens): Promise<string> {
  const jwt = await new SignJWT({ ...tokens })
    .setProtectedHeader({ alg: "HS256" })
    .setIssuedAt()
    .setExpirationTime("7d")
    .sign(getSecretKey());
  return jwt;
}

export async function decryptTokens(encrypted: string): Promise<StoredTokens | null> {
  try {
    const { payload } = await jwtVerify(encrypted, getSecretKey());
    return {
      accessToken: payload.accessToken as string,
      refreshToken: payload.refreshToken as string,
      expiresAt: payload.expiresAt as number,
    };
  } catch {
    return null;
  }
}

export async function setTokenCookie(tokens: StoredTokens): Promise<void> {
  const cookieStore = await cookies();
  const encrypted = await encryptTokens(tokens);
  
  cookieStore.set(TOKEN_COOKIE_NAME, encrypted, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    path: "/",
    maxAge: COOKIE_MAX_AGE,
  });
}

export async function getTokensFromCookie(): Promise<StoredTokens | null> {
  const cookieStore = await cookies();
  const cookie = cookieStore.get(TOKEN_COOKIE_NAME);
  
  if (!cookie?.value) {
    return null;
  }
  
  return decryptTokens(cookie.value);
}

export async function clearTokenCookie(): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.delete(TOKEN_COOKIE_NAME);
}

export async function getAccessToken(): Promise<string | null> {
  const tokens = await getTokensFromCookie();
  
  if (!tokens) {
    return null;
  }
  
  if (Date.now() >= tokens.expiresAt - 60000) {
    const refreshed = await refreshTokens(tokens.refreshToken);
    if (refreshed) {
      return refreshed.accessToken;
    }
    return null;
  }
  
  return tokens.accessToken;
}

async function refreshTokens(refreshToken: string): Promise<StoredTokens | null> {
  const identityServerUrl = process.env.IDENTITY_SERVER_URL || "http://localhost:5001";
  const clientId = process.env.IDENTITY_SERVER_CLIENT_ID || "nextApp";
  const clientSecret = process.env.IDENTITY_SERVER_CLIENT_SECRET || "secret";

  try {
    const response = await fetch(`${identityServerUrl}/connect/token`, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: new URLSearchParams({
        client_id: clientId,
        client_secret: clientSecret,
        grant_type: "refresh_token",
        refresh_token: refreshToken,
      }),
    });

    if (!response.ok) {
      await clearTokenCookie();
      return null;
    }

    const data = await response.json();
    const tokens: StoredTokens = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token || refreshToken,
      expiresAt: Date.now() + data.expires_in * 1000,
    };

    await setTokenCookie(tokens);
    return tokens;
  } catch {
    await clearTokenCookie();
    return null;
  }
}
