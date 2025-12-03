import { NextAuthOptions } from "next-auth";
import { JWT } from "next-auth/jwt";
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6";

export interface ExtendedToken extends JWT {
  access_token?: string;
  refresh_token?: string;
  expires_at?: number;
  error?: string;
}

export interface ExtendedSession {
  user: {
    id: string;
    name?: string | null;
    email?: string | null;
    image?: string | null;
  };
  accessToken?: string;
  error?: string;
  expires: string;
}

const identityServerUrl =
  process.env.IDENTITY_SERVER_URL || "http://localhost:5001";
const clientId = process.env.IDENTITY_SERVER_CLIENT_ID || "nextApp";
const clientSecret = process.env.IDENTITY_SERVER_CLIENT_SECRET || "secret";

async function refreshAccessToken(
  token: ExtendedToken
): Promise<ExtendedToken> {
  try {
    const url = `${identityServerUrl}/connect/token`;

    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded"
      },
      body: new URLSearchParams({
        client_id: clientId,
        client_secret: clientSecret,
        grant_type: "refresh_token",
        refresh_token: token.refresh_token!
      })
    });

    const refreshedTokens = await response.json();

    if (!response.ok) {
      throw refreshedTokens;
    }

    return {
      ...token,
      access_token: refreshedTokens.access_token,
      expires_at: Date.now() + refreshedTokens.expires_in * 1000,
      refresh_token: refreshedTokens.refresh_token ?? token.refresh_token
    };
  } catch (error) {
    console.error("Error refreshing access token:", error);
    return {
      ...token,
      error: "RefreshAccessTokenError"
    };
  }
}

export const authOptions: NextAuthOptions = {
  providers: [
    DuendeIDS6Provider({
      id: "id-server",
      name: "Identity Server",
      clientId,
      clientSecret,
      issuer: identityServerUrl,
      authorization: {
        params: {
          scope: "openid profile auction"
        }
      }
    })
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account && profile) {
        return {
          ...token,
          access_token: account.access_token,
          refresh_token: account.refresh_token,
          expires_at: account.expires_at! * 1000,
          id: profile.sub
        } as ExtendedToken;
      }

      const extendedToken = token as ExtendedToken;
      if (extendedToken.expires_at && Date.now() < extendedToken.expires_at) {
        return token;
      }

      if (extendedToken.refresh_token) {
        return refreshAccessToken(extendedToken);
      }

      return token;
    },
    async session({ session, token }) {
      const extendedToken = token as ExtendedToken;

      return {
        ...session,
        user: {
          ...session.user,
          id: extendedToken.id || extendedToken.sub
        },
        accessToken: extendedToken.access_token,
        error: extendedToken.error,
        expires: session.expires
      } as ExtendedSession;
    }
  },
  pages: {
    signIn: "/auth/signin",
    signOut: "/auth/signout",
    error: "/auth/error"
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60
  },
  debug: process.env.NODE_ENV === "development"
};
