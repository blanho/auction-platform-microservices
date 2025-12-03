import { NextAuthOptions } from "next-auth";
import { JWT } from "next-auth/jwt";
import CredentialsProvider from "next-auth/providers/credentials";

export interface ExtendedUser {
  id: string;
  name?: string | null;
  email?: string | null;
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

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
    CredentialsProvider({
      id: "id-server",
      name: "Identity Server",
      credentials: {
        username: { label: "Username", type: "text" },
        password: { label: "Password", type: "password" }
      },
      async authorize(credentials) {
        if (!credentials?.username || !credentials?.password) {
          return null;
        }

        try {
          const response = await fetch(`${identityServerUrl}/connect/token`, {
            method: "POST",
            headers: {
              "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
              grant_type: "password",
              username: credentials.username,
              password: credentials.password,
              client_id: clientId,
              client_secret: clientSecret,
              scope: "openid profile auction"
            })
          });

          if (!response.ok) {
            return null;
          }

          const tokens = await response.json();

          const userInfoResponse = await fetch(
            `${identityServerUrl}/connect/userinfo`,
            {
              headers: {
                Authorization: `Bearer ${tokens.access_token}`
              }
            }
          );

          if (!userInfoResponse.ok) {
            return null;
          }

          const userInfo = await userInfoResponse.json();

          return {
            id: userInfo.sub,
            name: userInfo.name || credentials.username,
            email: userInfo.email,
            accessToken: tokens.access_token,
            refreshToken: tokens.refresh_token,
            expiresAt: Date.now() + tokens.expires_in * 1000
          };
        } catch (error) {
          console.error("Authentication error:", error);
          return null;
        }
      }
    })
  ],
  callbacks: {
    async jwt({ token, user, account }) {
      if (user) {
        const extendedUser = user as ExtendedUser;
        return {
          ...token,
          access_token: extendedUser.accessToken,
          refresh_token: extendedUser.refreshToken,
          expires_at: extendedUser.expiresAt,
          id: user.id
        } as ExtendedToken;
      }

      if (account) {
        return {
          ...token,
          access_token: account.access_token,
          refresh_token: account.refresh_token,
          expires_at: account.expires_at! * 1000,
          id: token.sub
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
