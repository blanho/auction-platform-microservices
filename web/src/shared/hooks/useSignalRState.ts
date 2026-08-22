import { signalRService } from '@/services/signalr'
import type { HubConnectionState } from '@microsoft/signalr'
import { useSyncExternalStore } from 'react'

const subscribe = (listener: () => void) => signalRService.subscribeToState(listener)
const getSnapshot = () => signalRService.state
const getServerSnapshot = (): null => null

export function useSignalRState(): HubConnectionState | null {
  return useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot)
}
