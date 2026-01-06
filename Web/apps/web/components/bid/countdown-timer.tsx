"use client";

import { useCountdown } from "@repo/hooks";
import { Badge } from "@repo/ui";

interface CountdownTimerProps {
  endTime: string;
}

export function CountdownTimer({ endTime }: CountdownTimerProps) {
  const { days, hours, minutes, seconds, isExpired } = useCountdown(endTime);

  if (isExpired) {
    return (
      <Badge variant="destructive" className="text-lg">
        Ended
      </Badge>
    );
  }

  return (
    <div className="flex items-center justify-center gap-1 font-mono text-2xl font-bold">
      {days > 0 && (
        <>
          <TimeUnit value={days} label="d" />
          <span className="text-muted-foreground">:</span>
        </>
      )}
      <TimeUnit value={hours} label="h" />
      <span className="text-muted-foreground">:</span>
      <TimeUnit value={minutes} label="m" />
      <span className="text-muted-foreground">:</span>
      <TimeUnit value={seconds} label="s" />
    </div>
  );
}

function TimeUnit({ value, label }: { value: number; label: string }) {
  return (
    <div className="flex items-baseline">
      <span className="tabular-nums">{String(value).padStart(2, "0")}</span>
      <span className="text-xs text-muted-foreground">{label}</span>
    </div>
  );
}
