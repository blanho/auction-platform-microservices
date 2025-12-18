"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faShare,
    faCopy,
} from "@fortawesome/free-solid-svg-icons";
import { faFacebook, faXTwitter } from "@fortawesome/free-brands-svg-icons";
import { showSuccessToast } from "@/utils";
import { motion } from "framer-motion";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface ShareMenuProps {
    title: string;
    url?: string;
}

export function ShareMenu({ title, url }: ShareMenuProps) {
    const shareUrl = url || (typeof window !== "undefined" ? window.location.href : "");

    const copyLink = () => {
        navigator.clipboard.writeText(shareUrl);
        showSuccessToast("Link copied to clipboard");
    };

    const shareToTwitter = () => {
        const text = `Check out this auction: ${title}`;
        window.open(
            `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(shareUrl)}`,
            "_blank"
        );
    };

    const shareToFacebook = () => {
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl)}`, "_blank");
    };

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    className="w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 transition-all"
                >
                    <FontAwesomeIcon icon={faShare} className="w-5 h-5" />
                </motion.button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="rounded-xl">
                <DropdownMenuItem onClick={shareToFacebook} className="cursor-pointer">
                    <FontAwesomeIcon icon={faFacebook} className="w-4 h-4 mr-2 text-blue-600" />
                    Facebook
                </DropdownMenuItem>
                <DropdownMenuItem onClick={shareToTwitter} className="cursor-pointer">
                    <FontAwesomeIcon icon={faXTwitter} className="w-4 h-4 mr-2" />
                    X (Twitter)
                </DropdownMenuItem>
                <DropdownMenuItem onClick={copyLink} className="cursor-pointer">
                    <FontAwesomeIcon icon={faCopy} className="w-4 h-4 mr-2 text-slate-500" />
                    Copy Link
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
