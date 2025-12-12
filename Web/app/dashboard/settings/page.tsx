"use client";

import { useState } from "react";
import { useSession } from "next-auth/react";
import {
    Bell,
    Camera,
    Eye,
    EyeOff,
    Lock,
    Mail,
    Phone,
    Save,
    Shield,
    User,
} from "lucide-react";
import { DashboardLayout } from "@/components/layout/dashboard-layout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { toast } from "sonner";

export default function SettingsPage() {
    const { data: session } = useSession();
    const [showPassword, setShowPassword] = useState(false);
    const [showNewPassword, setShowNewPassword] = useState(false);

    const [profile, setProfile] = useState({
        fullName: session?.user?.name || "John Doe",
        email: session?.user?.email || "john@example.com",
        phone: "+1 (555) 123-4567",
        bio: "Car enthusiast and collector",
        location: "New York, NY",
    });

    const [notifications, setNotifications] = useState({
        emailBidUpdates: true,
        emailOutbid: true,
        emailAuctionEnd: true,
        emailNewsletter: false,
        pushBidUpdates: true,
        pushOutbid: true,
        pushAuctionEnd: true,
        smsBidUpdates: false,
        smsOutbid: true,
        smsAuctionEnd: false,
    });

    const [security, setSecurity] = useState({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
        twoFactor: false,
    });

    const handleProfileSave = () => {
        toast.success("Profile updated successfully");
    };

    const handleNotificationsSave = () => {
        toast.success("Notification preferences saved");
    };

    const handleSecuritySave = () => {
        if (security.newPassword !== security.confirmPassword) {
            toast.error("Passwords do not match");
            return;
        }
        toast.success("Security settings updated");
        setSecurity({ ...security, currentPassword: "", newPassword: "", confirmPassword: "" });
    };

    return (
        <DashboardLayout
            title="Settings"
            description="Manage your account settings and preferences"
        >
            <Tabs defaultValue="profile" className="space-y-6">
                <TabsList>
                    <TabsTrigger value="profile">
                        <User className="h-4 w-4 mr-2" />
                        Profile
                    </TabsTrigger>
                    <TabsTrigger value="notifications">
                        <Bell className="h-4 w-4 mr-2" />
                        Notifications
                    </TabsTrigger>
                    <TabsTrigger value="security">
                        <Shield className="h-4 w-4 mr-2" />
                        Security
                    </TabsTrigger>
                </TabsList>

                {/* Profile Tab */}
                <TabsContent value="profile">
                    <Card>
                        <CardHeader>
                            <CardTitle>Profile Information</CardTitle>
                            <CardDescription>
                                Update your personal information
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {/* Avatar */}
                            <div className="flex items-center gap-6">
                                <div className="relative">
                                    <Avatar className="h-24 w-24">
                                        <AvatarImage src={session?.user?.image || ""} />
                                        <AvatarFallback className="text-2xl bg-amber-500 text-white">
                                            {profile.fullName.charAt(0)}
                                        </AvatarFallback>
                                    </Avatar>
                                    <button className="absolute bottom-0 right-0 p-2 bg-white dark:bg-zinc-800 rounded-full shadow-lg border">
                                        <Camera className="h-4 w-4" />
                                    </button>
                                </div>
                                <div>
                                    <h3 className="font-medium">{profile.fullName}</h3>
                                    <p className="text-sm text-zinc-500">{profile.email}</p>
                                    <Button variant="outline" size="sm" className="mt-2">
                                        Change Photo
                                    </Button>
                                </div>
                            </div>

                            <Separator />

                            {/* Profile Form */}
                            <div className="grid gap-4 md:grid-cols-2">
                                <div className="space-y-2">
                                    <Label htmlFor="fullName">Full Name</Label>
                                    <div className="relative">
                                        <User className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                        <Input
                                            id="fullName"
                                            value={profile.fullName}
                                            onChange={(e) =>
                                                setProfile({ ...profile, fullName: e.target.value })
                                            }
                                            className="pl-10"
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <Label htmlFor="email">Email</Label>
                                    <div className="relative">
                                        <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                        <Input
                                            id="email"
                                            type="email"
                                            value={profile.email}
                                            onChange={(e) =>
                                                setProfile({ ...profile, email: e.target.value })
                                            }
                                            className="pl-10"
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <Label htmlFor="phone">Phone</Label>
                                    <div className="relative">
                                        <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                        <Input
                                            id="phone"
                                            value={profile.phone}
                                            onChange={(e) =>
                                                setProfile({ ...profile, phone: e.target.value })
                                            }
                                            className="pl-10"
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <Label htmlFor="location">Location</Label>
                                    <Input
                                        id="location"
                                        value={profile.location}
                                        onChange={(e) =>
                                            setProfile({ ...profile, location: e.target.value })
                                        }
                                    />
                                </div>
                                <div className="space-y-2 md:col-span-2">
                                    <Label htmlFor="bio">Bio</Label>
                                    <Input
                                        id="bio"
                                        value={profile.bio}
                                        onChange={(e) =>
                                            setProfile({ ...profile, bio: e.target.value })
                                        }
                                        placeholder="Tell us about yourself"
                                    />
                                </div>
                            </div>

                            <div className="flex justify-end">
                                <Button onClick={handleProfileSave} className="bg-amber-500 hover:bg-amber-600">
                                    <Save className="h-4 w-4 mr-2" />
                                    Save Changes
                                </Button>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                {/* Notifications Tab */}
                <TabsContent value="notifications">
                    <Card>
                        <CardHeader>
                            <CardTitle>Notification Preferences</CardTitle>
                            <CardDescription>
                                Choose how you want to be notified
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {/* Email Notifications */}
                            <div className="space-y-4">
                                <h4 className="font-medium flex items-center gap-2">
                                    <Mail className="h-4 w-4" />
                                    Email Notifications
                                </h4>
                                <div className="space-y-3 ml-6">
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <Label>Bid Updates</Label>
                                            <p className="text-sm text-zinc-500">
                                                Get notified when someone bids on your auctions
                                            </p>
                                        </div>
                                        <Switch
                                            checked={notifications.emailBidUpdates}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    emailBidUpdates: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <Label>Outbid Alerts</Label>
                                            <p className="text-sm text-zinc-500">
                                                Get notified when you&apos;ve been outbid
                                            </p>
                                        </div>
                                        <Switch
                                            checked={notifications.emailOutbid}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    emailOutbid: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <Label>Auction End</Label>
                                            <p className="text-sm text-zinc-500">
                                                Get notified when auctions you&apos;re watching end
                                            </p>
                                        </div>
                                        <Switch
                                            checked={notifications.emailAuctionEnd}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    emailAuctionEnd: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <Label>Newsletter</Label>
                                            <p className="text-sm text-zinc-500">
                                                Receive weekly highlights and featured auctions
                                            </p>
                                        </div>
                                        <Switch
                                            checked={notifications.emailNewsletter}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    emailNewsletter: checked,
                                                })
                                            }
                                        />
                                    </div>
                                </div>
                            </div>

                            <Separator />

                            {/* Push Notifications */}
                            <div className="space-y-4">
                                <h4 className="font-medium flex items-center gap-2">
                                    <Bell className="h-4 w-4" />
                                    Push Notifications
                                </h4>
                                <div className="space-y-3 ml-6">
                                    <div className="flex items-center justify-between">
                                        <Label>Bid Updates</Label>
                                        <Switch
                                            checked={notifications.pushBidUpdates}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    pushBidUpdates: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <Label>Outbid Alerts</Label>
                                        <Switch
                                            checked={notifications.pushOutbid}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    pushOutbid: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <Label>Auction End</Label>
                                        <Switch
                                            checked={notifications.pushAuctionEnd}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    pushAuctionEnd: checked,
                                                })
                                            }
                                        />
                                    </div>
                                </div>
                            </div>

                            <Separator />

                            {/* SMS Notifications */}
                            <div className="space-y-4">
                                <h4 className="font-medium flex items-center gap-2">
                                    <Phone className="h-4 w-4" />
                                    SMS Notifications
                                </h4>
                                <div className="space-y-3 ml-6">
                                    <div className="flex items-center justify-between">
                                        <Label>Bid Updates</Label>
                                        <Switch
                                            checked={notifications.smsBidUpdates}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    smsBidUpdates: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <Label>Outbid Alerts</Label>
                                        <Switch
                                            checked={notifications.smsOutbid}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    smsOutbid: checked,
                                                })
                                            }
                                        />
                                    </div>
                                    <div className="flex items-center justify-between">
                                        <Label>Auction End</Label>
                                        <Switch
                                            checked={notifications.smsAuctionEnd}
                                            onCheckedChange={(checked) =>
                                                setNotifications({
                                                    ...notifications,
                                                    smsAuctionEnd: checked,
                                                })
                                            }
                                        />
                                    </div>
                                </div>
                            </div>

                            <div className="flex justify-end">
                                <Button onClick={handleNotificationsSave} className="bg-amber-500 hover:bg-amber-600">
                                    <Save className="h-4 w-4 mr-2" />
                                    Save Preferences
                                </Button>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                {/* Security Tab */}
                <TabsContent value="security">
                    <div className="space-y-6">
                        {/* Change Password */}
                        <Card>
                            <CardHeader>
                                <CardTitle>Change Password</CardTitle>
                                <CardDescription>
                                    Update your password to keep your account secure
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                <div className="space-y-2">
                                    <Label htmlFor="currentPassword">Current Password</Label>
                                    <div className="relative">
                                        <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                        <Input
                                            id="currentPassword"
                                            type={showPassword ? "text" : "password"}
                                            value={security.currentPassword}
                                            onChange={(e) =>
                                                setSecurity({
                                                    ...security,
                                                    currentPassword: e.target.value,
                                                })
                                            }
                                            className="pl-10 pr-10"
                                        />
                                        <button
                                            type="button"
                                            className="absolute right-3 top-1/2 -translate-y-1/2"
                                            onClick={() => setShowPassword(!showPassword)}
                                        >
                                            {showPassword ? (
                                                <EyeOff className="h-4 w-4 text-zinc-400" />
                                            ) : (
                                                <Eye className="h-4 w-4 text-zinc-400" />
                                            )}
                                        </button>
                                    </div>
                                </div>
                                <div className="grid gap-4 md:grid-cols-2">
                                    <div className="space-y-2">
                                        <Label htmlFor="newPassword">New Password</Label>
                                        <div className="relative">
                                            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                            <Input
                                                id="newPassword"
                                                type={showNewPassword ? "text" : "password"}
                                                value={security.newPassword}
                                                onChange={(e) =>
                                                    setSecurity({
                                                        ...security,
                                                        newPassword: e.target.value,
                                                    })
                                                }
                                                className="pl-10 pr-10"
                                            />
                                            <button
                                                type="button"
                                                className="absolute right-3 top-1/2 -translate-y-1/2"
                                                onClick={() => setShowNewPassword(!showNewPassword)}
                                            >
                                                {showNewPassword ? (
                                                    <EyeOff className="h-4 w-4 text-zinc-400" />
                                                ) : (
                                                    <Eye className="h-4 w-4 text-zinc-400" />
                                                )}
                                            </button>
                                        </div>
                                    </div>
                                    <div className="space-y-2">
                                        <Label htmlFor="confirmPassword">Confirm New Password</Label>
                                        <div className="relative">
                                            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                            <Input
                                                id="confirmPassword"
                                                type={showNewPassword ? "text" : "password"}
                                                value={security.confirmPassword}
                                                onChange={(e) =>
                                                    setSecurity({
                                                        ...security,
                                                        confirmPassword: e.target.value,
                                                    })
                                                }
                                                className="pl-10"
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="flex justify-end">
                                    <Button onClick={handleSecuritySave}>
                                        Update Password
                                    </Button>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Two-Factor Authentication */}
                        <Card>
                            <CardHeader>
                                <CardTitle>Two-Factor Authentication</CardTitle>
                                <CardDescription>
                                    Add an extra layer of security to your account
                                </CardDescription>
                            </CardHeader>
                            <CardContent>
                                <div className="flex items-center justify-between">
                                    <div className="space-y-1">
                                        <p className="font-medium">Enable 2FA</p>
                                        <p className="text-sm text-zinc-500">
                                            Use an authenticator app to generate verification codes
                                        </p>
                                    </div>
                                    <Switch
                                        checked={security.twoFactor}
                                        onCheckedChange={(checked) =>
                                            setSecurity({ ...security, twoFactor: checked })
                                        }
                                    />
                                </div>
                            </CardContent>
                        </Card>

                        {/* Active Sessions */}
                        <Card>
                            <CardHeader>
                                <CardTitle>Active Sessions</CardTitle>
                                <CardDescription>
                                    Manage your active sessions across devices
                                </CardDescription>
                            </CardHeader>
                            <CardContent>
                                <div className="space-y-4">
                                    <div className="flex items-center justify-between p-4 border rounded-lg">
                                        <div>
                                            <p className="font-medium">Current Session</p>
                                            <p className="text-sm text-zinc-500">
                                                Windows • Chrome • New York, US
                                            </p>
                                            <p className="text-xs text-zinc-400">
                                                Last active: Just now
                                            </p>
                                        </div>
                                        <Badge className="bg-green-500">Current</Badge>
                                    </div>
                                    <div className="flex items-center justify-between p-4 border rounded-lg">
                                        <div>
                                            <p className="font-medium">iPhone 14 Pro</p>
                                            <p className="text-sm text-zinc-500">
                                                iOS • Safari • New York, US
                                            </p>
                                            <p className="text-xs text-zinc-400">
                                                Last active: 2 hours ago
                                            </p>
                                        </div>
                                        <Button variant="ghost" size="sm" className="text-red-500">
                                            Revoke
                                        </Button>
                                    </div>
                                </div>
                                <Button variant="outline" className="mt-4 w-full text-red-500">
                                    Sign Out All Other Sessions
                                </Button>
                            </CardContent>
                        </Card>
                    </div>
                </TabsContent>
            </Tabs>
        </DashboardLayout>
    );
}
