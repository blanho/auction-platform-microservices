"use client";

import { useState, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGavel } from "@fortawesome/free-solid-svg-icons";
import { getTimeRemaining } from "@/utils";
import { MESSAGES, TIME } from "@/constants";

interface TimeLeft {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

interface CountdownTimerProps {
    endDate: string;
    isUrgent?: boolean;
}

export function CountdownTimer({ endDate, isUrgent }: CountdownTimerProps) {
    const [timeLeft, setTimeLeft] = useState<TimeLeft | null>(() => getTimeRemaining(endDate));

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(getTimeRemaining(endDate));
        }, TIME.COUNTDOWN_INTERVAL);
        return () => clearInterval(timer);
    }, [endDate]);

    if (!timeLeft) {
        return (
            <div className="flex items-center gap-2 text-red-500 font-semibold">
                <FontAwesomeIcon icon={faGavel} className="w-5 h-5" />
                <span>{MESSAGES.LABELS.AUCTION_ENDED}</span>
            </div>
        );
    }

    const showDays = timeLeft.days > 0;
    const formatUnit = (value: number) => String(value).padStart(2, "0");
    const urgentClass = isUrgent ? "animate-pulse" : "";

    return (
        <div className="flex items-center gap-3">
            <div className={`flex gap-2 font-mono ${urgentClass}`}>
                {showDays && (
                    <div className="flex flex-col items-center">
                        <span className="text-3xl font-bold bg-linear-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                            {timeLeft.days}
                        </span>
                        <span className="text-[10px] uppercase tracking-wider text-slate-500">Days</span>
                    </div>
                )}
                {showDays && (
                    <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>
                )}
                <div className="flex flex-col items-center">
                    <span className="text-3xl font-bold bg-linear-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                        {formatUnit(timeLeft.hours)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Hours</span>
                </div>
                <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>
                <div className="flex flex-col items-center">
                    <span className="text-3xl font-bold bg-linear-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                        {formatUnit(timeLeft.minutes)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Mins</span>
                </div>
                <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>
                <div className="flex flex-col items-center">
                    <span
                        className={`text-3xl font-bold ${
                            isUrgent
                                ? "text-red-500"
                                : "bg-linear-to-br from-purple-600 to-blue-600 bg-clip-text text-transparent"
                        }`}
                    >
                        {formatUnit(timeLeft.seconds)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Secs</span>
                </div>
            </div>
        </div>
    );
}
