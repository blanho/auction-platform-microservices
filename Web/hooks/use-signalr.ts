"use client";

import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { Notification } from "@/types/notification";
import { API_ENDPOINTS } from "@/constants/api";
import { useAuthSession } from "@/hooks/use-auth-session";

const GATEWAY_URL =
  process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:6001";

const RECONNECT_TIMEOUT_MS = 60000;

interface UseSignalROptions {
  onNotification?: (notification: Notification) => void;
  onConnected?: () => void;
  onDisconnected?: () => void;
  onReconnecting?: () => void;
  onReconnected?: () => void;
}

interface UseSignalRReturn {
  connectionState: signalR.HubConnectionState;
  isConnected: boolean;
  isConnecting: boolean;
  isReconnecting: boolean;
  error: Error | null;
}

export function useSignalR(options: UseSignalROptions = {}): UseSignalRReturn {
  const { accessToken, isAuthenticated, isLoading } = useAuthSession();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [connectionState, setConnectionState] =
    useState<signalR.HubConnectionState>(
      signalR.HubConnectionState.Disconnected
    );
  const [error, setError] = useState<Error | null>(null);
  const optionsRef = useRef(options);

  useEffect(() => {
    optionsRef.current = options;
  }, [options]);

  useEffect(() => {
    if (isLoading || !isAuthenticated || !accessToken) {
      if (connectionRef.current) {
        connectionRef.current.stop().then(() => {
          connectionRef.current = null;
          setConnectionState(signalR.HubConnectionState.Disconnected);
        });
      }
      return;
    }

    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    let isMounted = true;

    const createConnection = async () => {
      try {
        const connection = new signalR.HubConnectionBuilder()
          .withUrl(`${GATEWAY_URL}${API_ENDPOINTS.NOTIFICATIONS_HUB}`, {
            accessTokenFactory: () => accessToken
          })
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: (retryContext) => {
              if (retryContext.elapsedMilliseconds < RECONNECT_TIMEOUT_MS) {
                return Math.random() * 10000;
              }
              return null;
            }
          })
          .configureLogging(signalR.LogLevel.Information)
          .build();

        connection.onclose((err) => {
          if (isMounted) {
            setConnectionState(signalR.HubConnectionState.Disconnected);
            setError(err || null);
            optionsRef.current.onDisconnected?.();
          }
        });

        connection.onreconnecting((err) => {
          if (isMounted) {
            setConnectionState(signalR.HubConnectionState.Reconnecting);
            setError(err || null);
            optionsRef.current.onReconnecting?.();
          }
        });

        connection.onreconnected(() => {
          if (isMounted) {
            setConnectionState(signalR.HubConnectionState.Connected);
            setError(null);
            optionsRef.current.onReconnected?.();
          }
        });

        connection.on("ReceiveNotification", (notification: Notification) => {
          optionsRef.current.onNotification?.(notification);
        });

        setConnectionState(signalR.HubConnectionState.Connecting);
        connectionRef.current = connection;
        await connection.start();

        if (isMounted) {
          setConnectionState(signalR.HubConnectionState.Connected);
          setError(null);
          optionsRef.current.onConnected?.();
        }
      } catch (err) {
        if (isMounted) {
          setConnectionState(signalR.HubConnectionState.Disconnected);
          setError(err as Error);
        }
      }
    };

    createConnection();

    return () => {
      isMounted = false;
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [isLoading, isAuthenticated, accessToken]);

  return {
    connectionState,
    isConnected: connectionState === signalR.HubConnectionState.Connected,
    isConnecting: connectionState === signalR.HubConnectionState.Connecting,
    isReconnecting: connectionState === signalR.HubConnectionState.Reconnecting,
    error
  };
}
