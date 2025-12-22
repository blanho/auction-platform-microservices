"use client";

import { useState, useEffect, useCallback } from "react";
import {
    Save,
    Globe,
    DollarSign,
    Bell,
    Shield,
    Mail,
    Loader2,
    RefreshCw,
} from "lucide-react";

import { MESSAGES } from "@/constants";

import { AdminLayout } from "@/components/layout/admin-layout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";

import { settingsService, PlatformSetting } from "@/services/settings.service";

interface SettingsState {
    platformName: string;
    platformFee: string;
    currency: string;
    timezone: string;
    minBidIncrement: string;
    maxAuctionDuration: string;
    autoExtendTime: string;
    enableAutoBid: boolean;
    enableBuyNow: boolean;
    emailNotifications: boolean;
    pushNotifications: boolean;
    smsNotifications: boolean;
    notifyOnNewBid: boolean;
    notifyOnOutbid: boolean;
    notifyOnAuctionEnd: boolean;
    requireEmailVerification: boolean;
    requirePhoneVerification: boolean;
    twoFactorAuth: boolean;
    maxLoginAttempts: string;
    smtpHost: string;
    smtpPort: string;
    fromEmail: string;
    fromName: string;
}

const defaultSettings: SettingsState = {
    platformName: "Auction Platform",
    platformFee: "5",
    currency: "USD",
    timezone: "America/New_York",
    minBidIncrement: "50",
    maxAuctionDuration: "30",
    autoExtendTime: "5",
    enableAutoBid: true,
    enableBuyNow: true,
    emailNotifications: true,
    pushNotifications: true,
    smsNotifications: false,
    notifyOnNewBid: true,
    notifyOnOutbid: true,
    notifyOnAuctionEnd: true,
    requireEmailVerification: true,
    requirePhoneVerification: false,
    twoFactorAuth: false,
    maxLoginAttempts: "5",
    smtpHost: "",
    smtpPort: "587",
    fromEmail: "",
    fromName: "Auction Platform",
};

function settingsArrayToState(settingsArray: PlatformSetting[]): Partial<SettingsState> {
    const result: Record<string, string | boolean> = {};
    
    settingsArray.forEach((setting) => {
        const key = setting.key as keyof SettingsState;
        if (key in defaultSettings) {
            const defaultValue = defaultSettings[key];
            if (typeof defaultValue === "boolean") {
                result[key] = setting.value === "true";
            } else {
                result[key] = setting.value;
            }
        }
    });
    
    return result as Partial<SettingsState>;
}

function stateToSettingsArray(state: SettingsState): { key: string; value: string }[] {
    return Object.entries(state).map(([key, value]) => ({
        key,
        value: String(value),
    }));
}

function SettingsSkeleton() {
    return (
        <Card>
            <CardHeader>
                <Skeleton className="h-6 w-48" />
                <Skeleton className="h-4 w-64 mt-2" />
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                    {[1, 2, 3, 4].map((i) => (
                        <div key={i} className="space-y-2">
                            <Skeleton className="h-4 w-24" />
                            <Skeleton className="h-10 w-full" />
                        </div>
                    ))}
                </div>
            </CardContent>
        </Card>
    );
}

export default function AdminSettingsPage() {
    const [settings, setSettings] = useState<SettingsState>(defaultSettings);
    const [originalSettings, setOriginalSettings] = useState<PlatformSetting[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);

    const fetchSettings = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await settingsService.getSettings();
            setOriginalSettings(data);
            const mappedSettings = settingsArrayToState(data);
            setSettings((prev) => ({ ...prev, ...mappedSettings }));
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchSettings();
    }, [fetchSettings]);

    const handleSave = async () => {
        setIsSaving(true);
        try {
            const changedSettings = stateToSettingsArray(settings).filter(({ key, value }) => {
                const original = originalSettings.find((s) => s.key === key);
                return !original || original.value !== value;
            });

            if (changedSettings.length > 0) {
                await settingsService.bulkUpdateSettings(changedSettings);
            }
            
            toast.success(MESSAGES.SUCCESS.SETTINGS_SAVED);
            fetchSettings();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsSaving(false);
        }
    };

    if (isLoading) {
        return (
            <AdminLayout>
                <div className="p-6 lg:p-8 space-y-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold tracking-tight">Settings</h1>
                            <p className="text-muted-foreground">Configure platform settings</p>
                        </div>
                    </div>
                    <SettingsSkeleton />
                    <SettingsSkeleton />
                    <SettingsSkeleton />
                </div>
            </AdminLayout>
        );
    }

    return (
        <AdminLayout>
            <div className="p-6 lg:p-8 space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold tracking-tight">Settings</h1>
                        <p className="text-muted-foreground">Configure platform settings</p>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={fetchSettings}
                        disabled={isLoading}
                    >
                        <RefreshCw className={`h-4 w-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
                        Refresh
                    </Button>
                </div>
                {/* Platform Settings */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <Globe className="h-5 w-5 text-blue-500" />
                            <CardTitle>Platform Settings</CardTitle>
                        </div>
                        <CardDescription>
                            General platform configuration
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-2">
                                <Label htmlFor="platformName">Platform Name</Label>
                                <Input
                                    id="platformName"
                                    value={settings.platformName}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            platformName: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="platformFee">Platform Fee (%)</Label>
                                <Input
                                    id="platformFee"
                                    type="number"
                                    value={settings.platformFee}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            platformFee: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="currency">Default Currency</Label>
                                <Select
                                    value={settings.currency}
                                    onValueChange={(value) =>
                                        setSettings({ ...settings, currency: value })
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="USD">USD ($)</SelectItem>
                                        <SelectItem value="EUR">EUR (€)</SelectItem>
                                        <SelectItem value="GBP">GBP (£)</SelectItem>
                                        <SelectItem value="JPY">JPY (¥)</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="timezone">Timezone</Label>
                                <Select
                                    value={settings.timezone}
                                    onValueChange={(value) =>
                                        setSettings({ ...settings, timezone: value })
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="America/New_York">
                                            Eastern Time (ET)
                                        </SelectItem>
                                        <SelectItem value="America/Los_Angeles">
                                            Pacific Time (PT)
                                        </SelectItem>
                                        <SelectItem value="Europe/London">
                                            GMT (London)
                                        </SelectItem>
                                        <SelectItem value="Asia/Tokyo">
                                            Japan Standard Time
                                        </SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Auction Settings */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <DollarSign className="h-5 w-5 text-green-500" />
                            <CardTitle>Auction Settings</CardTitle>
                        </div>
                        <CardDescription>
                            Configure auction behavior and rules
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="grid gap-4 md:grid-cols-3">
                            <div className="space-y-2">
                                <Label htmlFor="minBidIncrement">
                                    Min Bid Increment ($)
                                </Label>
                                <Input
                                    id="minBidIncrement"
                                    type="number"
                                    value={settings.minBidIncrement}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            minBidIncrement: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="maxAuctionDuration">
                                    Max Auction Duration (days)
                                </Label>
                                <Input
                                    id="maxAuctionDuration"
                                    type="number"
                                    value={settings.maxAuctionDuration}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            maxAuctionDuration: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="autoExtendTime">
                                    Auto-Extend Time (minutes)
                                </Label>
                                <Input
                                    id="autoExtendTime"
                                    type="number"
                                    value={settings.autoExtendTime}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            autoExtendTime: e.target.value,
                                        })
                                    }
                                />
                            </div>
                        </div>
                        <Separator />
                        <div className="space-y-4">
                            <div className="flex items-center justify-between">
                                <div>
                                    <Label>Enable Auto-Bid</Label>
                                    <p className="text-sm text-zinc-500">
                                        Allow users to set automatic bidding
                                    </p>
                                </div>
                                <Switch
                                    checked={settings.enableAutoBid}
                                    onCheckedChange={(checked) =>
                                        setSettings({ ...settings, enableAutoBid: checked })
                                    }
                                />
                            </div>
                            <div className="flex items-center justify-between">
                                <div>
                                    <Label>Enable Buy Now</Label>
                                    <p className="text-sm text-zinc-500">
                                        Allow sellers to set a buy now price
                                    </p>
                                </div>
                                <Switch
                                    checked={settings.enableBuyNow}
                                    onCheckedChange={(checked) =>
                                        setSettings({ ...settings, enableBuyNow: checked })
                                    }
                                />
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Notification Settings */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <Bell className="h-5 w-5 text-amber-500" />
                            <CardTitle>Notification Settings</CardTitle>
                        </div>
                        <CardDescription>
                            Configure notification preferences
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="grid gap-4 md:grid-cols-3">
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>Email Notifications</Label>
                                    <p className="text-xs text-zinc-500">Send via email</p>
                                </div>
                                <Switch
                                    checked={settings.emailNotifications}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            emailNotifications: checked,
                                        })
                                    }
                                />
                            </div>
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>Push Notifications</Label>
                                    <p className="text-xs text-zinc-500">Browser push</p>
                                </div>
                                <Switch
                                    checked={settings.pushNotifications}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            pushNotifications: checked,
                                        })
                                    }
                                />
                            </div>
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>SMS Notifications</Label>
                                    <p className="text-xs text-zinc-500">Send via SMS</p>
                                </div>
                                <Switch
                                    checked={settings.smsNotifications}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            smsNotifications: checked,
                                        })
                                    }
                                />
                            </div>
                        </div>
                        <Separator />
                        <div className="space-y-4">
                            <h4 className="text-sm font-medium">Notification Events</h4>
                            <div className="grid gap-4 md:grid-cols-3">
                                <div className="flex items-center justify-between">
                                    <Label>New Bid Placed</Label>
                                    <Switch
                                        checked={settings.notifyOnNewBid}
                                        onCheckedChange={(checked) =>
                                            setSettings({
                                                ...settings,
                                                notifyOnNewBid: checked,
                                            })
                                        }
                                    />
                                </div>
                                <div className="flex items-center justify-between">
                                    <Label>Outbid Alert</Label>
                                    <Switch
                                        checked={settings.notifyOnOutbid}
                                        onCheckedChange={(checked) =>
                                            setSettings({
                                                ...settings,
                                                notifyOnOutbid: checked,
                                            })
                                        }
                                    />
                                </div>
                                <div className="flex items-center justify-between">
                                    <Label>Auction Ended</Label>
                                    <Switch
                                        checked={settings.notifyOnAuctionEnd}
                                        onCheckedChange={(checked) =>
                                            setSettings({
                                                ...settings,
                                                notifyOnAuctionEnd: checked,
                                            })
                                        }
                                    />
                                </div>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Security Settings */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <Shield className="h-5 w-5 text-red-500" />
                            <CardTitle>Security Settings</CardTitle>
                        </div>
                        <CardDescription>
                            Configure security and authentication
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>Email Verification</Label>
                                    <p className="text-xs text-zinc-500">
                                        Require email verification
                                    </p>
                                </div>
                                <Switch
                                    checked={settings.requireEmailVerification}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            requireEmailVerification: checked,
                                        })
                                    }
                                />
                            </div>
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>Phone Verification</Label>
                                    <p className="text-xs text-zinc-500">
                                        Require phone verification
                                    </p>
                                </div>
                                <Switch
                                    checked={settings.requirePhoneVerification}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            requirePhoneVerification: checked,
                                        })
                                    }
                                />
                            </div>
                            <div className="flex items-center justify-between p-4 border rounded-lg">
                                <div>
                                    <Label>Two-Factor Auth</Label>
                                    <p className="text-xs text-zinc-500">
                                        Enforce 2FA for all users
                                    </p>
                                </div>
                                <Switch
                                    checked={settings.twoFactorAuth}
                                    onCheckedChange={(checked) =>
                                        setSettings({
                                            ...settings,
                                            twoFactorAuth: checked,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2 p-4 border rounded-lg">
                                <Label htmlFor="maxLoginAttempts">
                                    Max Login Attempts
                                </Label>
                                <Input
                                    id="maxLoginAttempts"
                                    type="number"
                                    value={settings.maxLoginAttempts}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            maxLoginAttempts: e.target.value,
                                        })
                                    }
                                />
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Email Settings */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center gap-2">
                            <Mail className="h-5 w-5 text-purple-500" />
                            <CardTitle>Email Settings</CardTitle>
                        </div>
                        <CardDescription>
                            Configure email server settings
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-2">
                                <Label htmlFor="smtpHost">SMTP Host</Label>
                                <Input
                                    id="smtpHost"
                                    value={settings.smtpHost}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            smtpHost: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="smtpPort">SMTP Port</Label>
                                <Input
                                    id="smtpPort"
                                    value={settings.smtpPort}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            smtpPort: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="fromEmail">From Email</Label>
                                <Input
                                    id="fromEmail"
                                    type="email"
                                    value={settings.fromEmail}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            fromEmail: e.target.value,
                                        })
                                    }
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="fromName">From Name</Label>
                                <Input
                                    id="fromName"
                                    value={settings.fromName}
                                    onChange={(e) =>
                                        setSettings({
                                            ...settings,
                                            fromName: e.target.value,
                                        })
                                    }
                                />
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <div className="flex justify-end">
                    <Button
                        onClick={handleSave}
                        className="bg-amber-500 hover:bg-amber-600"
                        disabled={isSaving}
                    >
                        {isSaving ? (
                            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        ) : (
                            <Save className="h-4 w-4 mr-2" />
                        )}
                        {isSaving ? "Saving..." : "Save Settings"}
                    </Button>
                </div>
            </div>
        </AdminLayout>
    );
}
