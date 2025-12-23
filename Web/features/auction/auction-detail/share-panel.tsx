"use client";

import { useState, useCallback, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faShare,
    faCopy,
    faCheck,
    faEnvelope,
    faLink,
    faQrcode,
    faCode,
} from "@fortawesome/free-solid-svg-icons";
import {
    faFacebook,
    faXTwitter,
    faWhatsapp,
    faTelegram,
    faLinkedin,
    faPinterest,
    faReddit,
} from "@fortawesome/free-brands-svg-icons";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { showSuccessToast } from "@/utils";
import QRCode from "qrcode";

interface ShareOption {
    id: string;
    name: string;
    icon: typeof faFacebook;
    color: string;
    bgColor: string;
    action: (url: string, title: string, description?: string) => void;
}

interface SharePanelProps {
    title: string;
    description?: string;
    url?: string;
    price?: number;
    className?: string;
    variant?: "button" | "icon" | "compact";
}

const shareOptions: ShareOption[] = [
    {
        id: "facebook",
        name: "Facebook",
        icon: faFacebook,
        color: "text-white",
        bgColor: "bg-[#1877F2] hover:bg-[#0d65d9]",
        action: (url) => {
            window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`, "_blank", "width=600,height=400");
        },
    },
    {
        id: "twitter",
        name: "X (Twitter)",
        icon: faXTwitter,
        color: "text-white",
        bgColor: "bg-black hover:bg-gray-800",
        action: (url, title) => {
            const text = `Check out this auction: ${title}`;
            window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`, "_blank", "width=600,height=400");
        },
    },
    {
        id: "whatsapp",
        name: "WhatsApp",
        icon: faWhatsapp,
        color: "text-white",
        bgColor: "bg-[#25D366] hover:bg-[#1da851]",
        action: (url, title) => {
            const text = `Check out this auction: ${title} ${url}`;
            window.open(`https://wa.me/?text=${encodeURIComponent(text)}`, "_blank");
        },
    },
    {
        id: "telegram",
        name: "Telegram",
        icon: faTelegram,
        color: "text-white",
        bgColor: "bg-[#0088cc] hover:bg-[#006da3]",
        action: (url, title) => {
            window.open(`https://t.me/share/url?url=${encodeURIComponent(url)}&text=${encodeURIComponent(title)}`, "_blank");
        },
    },
    {
        id: "linkedin",
        name: "LinkedIn",
        icon: faLinkedin,
        color: "text-white",
        bgColor: "bg-[#0A66C2] hover:bg-[#084d94]",
        action: (url) => {
            window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`, "_blank", "width=600,height=400");
        },
    },
    {
        id: "pinterest",
        name: "Pinterest",
        icon: faPinterest,
        color: "text-white",
        bgColor: "bg-[#E60023] hover:bg-[#c4001e]",
        action: (url, title, description) => {
            window.open(`https://pinterest.com/pin/create/button/?url=${encodeURIComponent(url)}&description=${encodeURIComponent(description || title)}`, "_blank", "width=600,height=400");
        },
    },
    {
        id: "reddit",
        name: "Reddit",
        icon: faReddit,
        color: "text-white",
        bgColor: "bg-[#FF4500] hover:bg-[#e03d00]",
        action: (url, title) => {
            window.open(`https://www.reddit.com/submit?url=${encodeURIComponent(url)}&title=${encodeURIComponent(title)}`, "_blank");
        },
    },
    {
        id: "email",
        name: "Email",
        icon: faEnvelope,
        color: "text-white",
        bgColor: "bg-slate-600 hover:bg-slate-700",
        action: (url, title, description) => {
            const subject = `Check out this auction: ${title}`;
            const body = `${description || title}\n\n${url}`;
            window.location.href = `mailto:?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
        },
    },
];

export function SharePanel({
    title,
    description,
    url,
    price,
    className,
    variant = "button",
}: SharePanelProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [copied, setCopied] = useState(false);
    const [qrCode, setQrCode] = useState<string | null>(null);
    const [showQR, setShowQR] = useState(false);
    const supportsNativeShare = typeof navigator !== "undefined" && !!navigator.share;

    const shareUrl = url || (typeof window !== "undefined" ? window.location.href : "");

    useEffect(() => {
        if (showQR && shareUrl) {
            QRCode.toDataURL(shareUrl, {
                width: 200,
                margin: 2,
                color: { dark: "#1e293b", light: "#ffffff" },
            })
                .then(setQrCode)
                .catch(() => setQrCode(null));
        }
    }, [showQR, shareUrl]);

    const copyLink = useCallback(async () => {
        try {
            await navigator.clipboard.writeText(shareUrl);
            setCopied(true);
            showSuccessToast("Link copied to clipboard!");
            setTimeout(() => setCopied(false), 2000);
        } catch {
            const textArea = document.createElement("textarea");
            textArea.value = shareUrl;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand("copy");
            document.body.removeChild(textArea);
            setCopied(true);
            showSuccessToast("Link copied to clipboard!");
            setTimeout(() => setCopied(false), 2000);
        }
    }, [shareUrl]);

    const handleNativeShare = useCallback(async () => {
        if (!navigator.share) return;

        try {
            await navigator.share({
                title: title,
                text: description || `Check out this auction: ${title}`,
                url: shareUrl,
            });
        } catch (err) {
            if ((err as Error).name !== "AbortError") {
                setIsOpen(true);
            }
        }
    }, [title, description, shareUrl]);

    const handleShare = useCallback((option: ShareOption) => {
        option.action(shareUrl, title, description);
    }, [shareUrl, title, description]);

    const copyEmbedCode = useCallback(() => {
        const embedCode = `<iframe src="${shareUrl}?embed=true" width="400" height="300" frameborder="0"></iframe>`;
        navigator.clipboard.writeText(embedCode);
        showSuccessToast("Embed code copied!");
    }, [shareUrl]);

    const triggerButton = (() => {
        if (variant === "icon") {
            return (
                <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    className={cn(
                        "w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center",
                        "text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 transition-all",
                        className
                    )}
                >
                    <FontAwesomeIcon icon={faShare} className="w-5 h-5" />
                </motion.button>
            );
        }

        if (variant === "compact") {
            return (
                <Button variant="ghost" size="sm" className={cn("gap-2", className)}>
                    <FontAwesomeIcon icon={faShare} className="w-4 h-4" />
                    Share
                </Button>
            );
        }

        return (
            <Button variant="outline" className={cn("gap-2", className)}>
                <FontAwesomeIcon icon={faShare} className="w-4 h-4" />
                Share Auction
            </Button>
        );
    })();

    if (supportsNativeShare && variant === "icon") {
        return (
            <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={handleNativeShare}
                className={cn(
                    "w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center",
                    "text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 transition-all",
                    className
                )}
            >
                <FontAwesomeIcon icon={faShare} className="w-5 h-5" />
            </motion.button>
        );
    }

    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>
                <div onClick={supportsNativeShare ? handleNativeShare : undefined}>
                    {triggerButton}
                </div>
            </DialogTrigger>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <FontAwesomeIcon icon={faShare} className="w-5 h-5 text-purple-500" />
                        Share this Auction
                    </DialogTitle>
                </DialogHeader>

                <div className="space-y-6 py-4">
                    <div className="p-4 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                        <p className="font-medium text-slate-900 dark:text-white line-clamp-2">{title}</p>
                        {price && (
                            <p className="text-sm text-purple-600 dark:text-purple-400 mt-1">
                                Current bid: ${price.toLocaleString()}
                            </p>
                        )}
                    </div>

                    <div>
                        <p className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-3">
                            Share via
                        </p>
                        <div className="grid grid-cols-4 gap-3">
                            {shareOptions.slice(0, 8).map((option) => (
                                <motion.button
                                    key={option.id}
                                    whileHover={{ scale: 1.05 }}
                                    whileTap={{ scale: 0.95 }}
                                    onClick={() => handleShare(option)}
                                    className={cn(
                                        "flex flex-col items-center gap-1.5 p-3 rounded-xl transition-all",
                                        option.bgColor,
                                        option.color
                                    )}
                                >
                                    <FontAwesomeIcon icon={option.icon} className="w-5 h-5" />
                                    <span className="text-xs font-medium">{option.name.split(" ")[0]}</span>
                                </motion.button>
                            ))}
                        </div>
                    </div>

                    <div>
                        <p className="text-sm font-medium text-slate-700 dark:text-slate-300 mb-3">
                            Or copy link
                        </p>
                        <div className="flex gap-2">
                            <div className="relative flex-1">
                                <FontAwesomeIcon
                                    icon={faLink}
                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400"
                                />
                                <Input
                                    value={shareUrl}
                                    readOnly
                                    className="pl-10 pr-4 bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700 text-sm"
                                />
                            </div>
                            <Button
                                onClick={copyLink}
                                variant={copied ? "default" : "outline"}
                                className={cn(
                                    "min-w-[100px] transition-all",
                                    copied && "bg-green-500 hover:bg-green-600 text-white border-green-500"
                                )}
                            >
                                <FontAwesomeIcon
                                    icon={copied ? faCheck : faCopy}
                                    className="w-4 h-4 mr-2"
                                />
                                {copied ? "Copied!" : "Copy"}
                            </Button>
                        </div>
                    </div>

                    <div className="flex gap-2">
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setShowQR(!showQR)}
                            className="flex-1"
                        >
                            <FontAwesomeIcon icon={faQrcode} className="w-4 h-4 mr-2" />
                            {showQR ? "Hide QR" : "Show QR Code"}
                        </Button>
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={copyEmbedCode}
                            className="flex-1"
                        >
                            <FontAwesomeIcon icon={faCode} className="w-4 h-4 mr-2" />
                            Embed Code
                        </Button>
                    </div>

                    <AnimatePresence>
                        {showQR && qrCode && (
                            <motion.div
                                initial={{ opacity: 0, height: 0 }}
                                animate={{ opacity: 1, height: "auto" }}
                                exit={{ opacity: 0, height: 0 }}
                                className="flex justify-center overflow-hidden"
                            >
                                <div className="p-4 bg-white rounded-xl shadow-inner">
                                    {/* eslint-disable-next-line @next/next/no-img-element */}
                                    <img src={qrCode} alt="QR Code" className="w-48 h-48" />
                                </div>
                            </motion.div>
                        )}
                    </AnimatePresence>
                </div>
            </DialogContent>
        </Dialog>
    );
}

export function QuickShareButtons({
    title,
    url,
    className,
}: {
    title: string;
    url?: string;
    className?: string;
}) {
    const shareUrl = url || (typeof window !== "undefined" ? window.location.href : "");
    const [copied, setCopied] = useState(false);

    const copyLink = useCallback(async () => {
        await navigator.clipboard.writeText(shareUrl);
        setCopied(true);
        showSuccessToast("Link copied!");
        setTimeout(() => setCopied(false), 2000);
    }, [shareUrl]);

    const shareToTwitter = useCallback(() => {
        const text = `Check out this auction: ${title}`;
        window.open(
            `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(shareUrl)}`,
            "_blank",
            "width=600,height=400"
        );
    }, [title, shareUrl]);

    const shareToFacebook = useCallback(() => {
        window.open(
            `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl)}`,
            "_blank",
            "width=600,height=400"
        );
    }, [shareUrl]);

    return (
        <div className={cn("flex items-center gap-2", className)}>
            <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={shareToFacebook}
                className="w-10 h-10 rounded-lg bg-[#1877F2] text-white flex items-center justify-center hover:bg-[#0d65d9] transition-colors"
            >
                <FontAwesomeIcon icon={faFacebook} className="w-5 h-5" />
            </motion.button>
            <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={shareToTwitter}
                className="w-10 h-10 rounded-lg bg-black text-white flex items-center justify-center hover:bg-gray-800 transition-colors"
            >
                <FontAwesomeIcon icon={faXTwitter} className="w-5 h-5" />
            </motion.button>
            <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={copyLink}
                className={cn(
                    "w-10 h-10 rounded-lg flex items-center justify-center transition-colors",
                    copied
                        ? "bg-green-500 text-white"
                        : "bg-slate-100 dark:bg-slate-800 text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30"
                )}
            >
                <FontAwesomeIcon icon={copied ? faCheck : faCopy} className="w-4 h-4" />
            </motion.button>
        </div>
    );
}
