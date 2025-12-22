"use client";

import { useLocale, useTranslations } from "next-intl";
import { usePathname, useRouter } from "@/i18n/navigation";
import { locales, type Locale } from "@/i18n/routing";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGlobe, faCheck } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";
import { useTransition } from "react";

const localeConfig: Record<Locale, { name: string; flag: string }> = {
  en: { name: "English", flag: "🇺🇸" },
  ja: { name: "日本語", flag: "🇯🇵" }
};

interface LanguageSwitcherProps {
  variant?: "default" | "compact";
  className?: string;
}

export function LanguageSwitcher({
  variant = "default",
  className
}: LanguageSwitcherProps) {
  const t = useTranslations("language");
  const locale = useLocale() as Locale;
  const router = useRouter();
  const pathname = usePathname();
  const [isPending, startTransition] = useTransition();

  const handleLocaleChange = (newLocale: Locale) => {
    startTransition(() => {
      router.replace(pathname, { locale: newLocale });
    });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size={variant === "compact" ? "icon" : "default"}
          className={cn(
            "gap-2",
            isPending && "opacity-50 pointer-events-none",
            className
          )}
          disabled={isPending}
        >
          <FontAwesomeIcon icon={faGlobe} className="h-4 w-4" />
          {variant === "default" && (
            <span className="hidden sm:inline">
              {localeConfig[locale].flag} {localeConfig[locale].name}
            </span>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-40">
        {locales.map((loc) => (
          <DropdownMenuItem
            key={loc}
            onClick={() => handleLocaleChange(loc)}
            className={cn(
              "flex items-center gap-2 cursor-pointer",
              locale === loc && "bg-slate-100 dark:bg-slate-800"
            )}
          >
            <span className="text-lg">{localeConfig[loc].flag}</span>
            <span className="flex-1">{localeConfig[loc].name}</span>
            {locale === loc && (
              <FontAwesomeIcon
                icon={faCheck}
                className="h-3 w-3 text-green-500"
              />
            )}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
