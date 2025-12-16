'use client';

import { useState, useEffect } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSpinner, faShield, faShieldHalved, faShieldXmark, faCopy, faCheck, faMobile, faKey } from '@fortawesome/free-solid-svg-icons';
import { toast } from 'sonner';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import {
    Alert,
    AlertDescription,
    AlertTitle,
} from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';

interface TwoFactorStatus {
    isEnabled: boolean;
    hasAuthenticator: boolean;
    recoveryCodesLeft: number;
    isMachineRemembered: boolean;
}

interface TwoFactorSetup {
    sharedKey: string;
    authenticatorUri: string;
}

interface TwoFactorSettingsProps {
    onStatusChange?: (enabled: boolean) => void;
}

export function TwoFactorSettings({ onStatusChange }: TwoFactorSettingsProps) {
    const [status, setStatus] = useState<TwoFactorStatus | null>(null);
    const [setupData, setSetupData] = useState<TwoFactorSetup | null>(null);
    const [recoveryCodes, setRecoveryCodes] = useState<string[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isSetupDialogOpen, setIsSetupDialogOpen] = useState(false);
    const [isDisableDialogOpen, setIsDisableDialogOpen] = useState(false);
    const [isRecoveryCodesDialogOpen, setIsRecoveryCodesDialogOpen] = useState(false);
    const [verificationCode, setVerificationCode] = useState('');
    const [disablePassword, setDisablePassword] = useState('');
    const [isVerifying, setIsVerifying] = useState(false);
    const [copiedCode, setCopiedCode] = useState<string | null>(null);

    useEffect(() => {
        fetchStatus();
    }, []);

    const fetchStatus = async () => {
        try {
            const response = await fetch('/api/account/2fa/status');
            if (response.ok) {
                const data = await response.json();
                setStatus(data.data);
            }
        } catch {
            toast.error('Failed to fetch 2FA status');
        } finally {
            setIsLoading(false);
        }
    };

    const initiateSetup = async () => {
        try {
            const response = await fetch('/api/account/2fa/setup', {
                method: 'POST',
            });
            if (response.ok) {
                const data = await response.json();
                setSetupData(data.data);
                setIsSetupDialogOpen(true);
            } else {
                toast.error('Failed to initiate 2FA setup');
            }
        } catch {
            toast.error('Failed to initiate 2FA setup');
        }
    };

    const enableTwoFactor = async () => {
        if (!verificationCode || verificationCode.length < 6) {
            toast.error('Please enter a valid 6-digit code');
            return;
        }

        setIsVerifying(true);
        try {
            const response = await fetch('/api/account/2fa/enable', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ code: verificationCode }),
            });

            if (response.ok) {
                const data = await response.json();
                setRecoveryCodes(data.data.recoveryCodes);
                setIsSetupDialogOpen(false);
                setIsRecoveryCodesDialogOpen(true);
                setVerificationCode('');
                await fetchStatus();
                onStatusChange?.(true);
                toast.success('Two-factor authentication enabled successfully');
            } else {
                const error = await response.json();
                toast.error(error.message || 'Invalid verification code');
            }
        } catch {
            toast.error('Failed to enable 2FA');
        } finally {
            setIsVerifying(false);
        }
    };

    const disableTwoFactor = async () => {
        if (!disablePassword) {
            toast.error('Please enter your password');
            return;
        }

        setIsVerifying(true);
        try {
            const response = await fetch('/api/account/2fa/disable', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ password: disablePassword }),
            });

            if (response.ok) {
                setIsDisableDialogOpen(false);
                setDisablePassword('');
                await fetchStatus();
                onStatusChange?.(false);
                toast.success('Two-factor authentication disabled');
            } else {
                const error = await response.json();
                toast.error(error.message || 'Invalid password');
            }
        } catch {
            toast.error('Failed to disable 2FA');
        } finally {
            setIsVerifying(false);
        }
    };

    const generateNewRecoveryCodes = async () => {
        try {
            const response = await fetch('/api/account/2fa/generate-codes', {
                method: 'POST',
            });

            if (response.ok) {
                const data = await response.json();
                setRecoveryCodes(data.data.recoveryCodes);
                setIsRecoveryCodesDialogOpen(true);
                await fetchStatus();
                toast.success('New recovery codes generated');
            } else {
                toast.error('Failed to generate recovery codes');
            }
        } catch {
            toast.error('Failed to generate recovery codes');
        }
    };

    const copyToClipboard = (text: string, label: string) => {
        navigator.clipboard.writeText(text);
        setCopiedCode(label);
        setTimeout(() => setCopiedCode(null), 2000);
        toast.success('Copied to clipboard');
    };

    if (isLoading) {
        return (
            <Card>
                <CardContent className="flex items-center justify-center py-8">
                    <FontAwesomeIcon icon={faSpinner} className="h-6 w-6 animate-spin" />
                </CardContent>
            </Card>
        );
    }

    return (
        <>
            <Card>
                <CardHeader>
                    <div className="flex items-center justify-between">
                        <div>
                            <CardTitle className="flex items-center gap-2">
                                <FontAwesomeIcon icon={faShield} className="h-5 w-5" />
                                Two-Factor Authentication
                            </CardTitle>
                            <CardDescription>
                                Add an extra layer of security to your account
                            </CardDescription>
                        </div>
                        {status?.isEnabled ? (
                            <Badge className="bg-green-500">Enabled</Badge>
                        ) : (
                            <Badge variant="outline">Disabled</Badge>
                        )}
                    </div>
                </CardHeader>
                <CardContent className="space-y-4">
                    {status?.isEnabled ? (
                        <>
                            <Alert>
                                <FontAwesomeIcon icon={faShieldHalved} className="h-4 w-4" />
                                <AlertTitle>2FA is Active</AlertTitle>
                                <AlertDescription>
                                    Your account is protected with two-factor authentication.
                                    {status.recoveryCodesLeft > 0 && (
                                        <span className="block mt-1">
                                            You have {status.recoveryCodesLeft} recovery codes remaining.
                                        </span>
                                    )}
                                </AlertDescription>
                            </Alert>

                            <div className="flex gap-2">
                                <Button
                                    variant="outline"
                                    onClick={generateNewRecoveryCodes}
                                >
                                    <FontAwesomeIcon icon={faKey} className="h-4 w-4 mr-2" />
                                    Generate New Codes
                                </Button>
                                <Button
                                    variant="destructive"
                                    onClick={() => setIsDisableDialogOpen(true)}
                                >
                                    <FontAwesomeIcon icon={faShieldXmark} className="h-4 w-4 mr-2" />
                                    Disable 2FA
                                </Button>
                            </div>
                        </>
                    ) : (
                        <>
                            <Alert>
                                <FontAwesomeIcon icon={faShieldXmark} className="h-4 w-4" />
                                <AlertTitle>2FA is Not Enabled</AlertTitle>
                                <AlertDescription>
                                    Protect your account by enabling two-factor authentication using an authenticator app.
                                </AlertDescription>
                            </Alert>

                            <Button onClick={initiateSetup} className="bg-amber-500 hover:bg-amber-600">
                                <FontAwesomeIcon icon={faMobile} className="h-4 w-4 mr-2" />
                                Set Up Two-Factor Authentication
                            </Button>
                        </>
                    )}
                </CardContent>
            </Card>

            <Dialog open={isSetupDialogOpen} onOpenChange={setIsSetupDialogOpen}>
                <DialogContent className="sm:max-w-md">
                    <DialogHeader>
                        <DialogTitle>Set Up Two-Factor Authentication</DialogTitle>
                        <DialogDescription>
                            Scan the QR code or enter the key in your authenticator app
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4">
                        {setupData && (
                            <>
                                <div className="flex flex-col items-center space-y-4">
                                    <div className="p-4 bg-white rounded-lg">
                                        <img
                                            src={`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(setupData.authenticatorUri)}`}
                                            alt="QR Code"
                                            className="w-48 h-48"
                                        />
                                    </div>
                                    <p className="text-sm text-muted-foreground text-center">
                                        Scan this QR code with your authenticator app
                                    </p>
                                </div>

                                <div className="space-y-2">
                                    <Label>Or enter this key manually:</Label>
                                    <div className="flex gap-2">
                                        <Input
                                            value={setupData.sharedKey}
                                            readOnly
                                            className="font-mono text-sm"
                                        />
                                        <Button
                                            variant="outline"
                                            size="icon"
                                            onClick={() => copyToClipboard(setupData.sharedKey, 'key')}
                                        >
                                            {copiedCode === 'key' ? (
                                                <FontAwesomeIcon icon={faCheck} className="h-4 w-4" />
                                            ) : (
                                                <FontAwesomeIcon icon={faCopy} className="h-4 w-4" />
                                            )}
                                        </Button>
                                    </div>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="verificationCode">Verification Code</Label>
                                    <Input
                                        id="verificationCode"
                                        placeholder="Enter 6-digit code"
                                        value={verificationCode}
                                        onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                                        maxLength={6}
                                    />
                                </div>
                            </>
                        )}
                    </div>

                    <DialogFooter>
                        <Button variant="outline" onClick={() => setIsSetupDialogOpen(false)}>
                            Cancel
                        </Button>
                        <Button
                            onClick={enableTwoFactor}
                            disabled={isVerifying || verificationCode.length !== 6}
                            className="bg-amber-500 hover:bg-amber-600"
                        >
                            {isVerifying && <FontAwesomeIcon icon={faSpinner} className="h-4 w-4 mr-2 animate-spin" />}
                            Verify & Enable
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            <Dialog open={isDisableDialogOpen} onOpenChange={setIsDisableDialogOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Disable Two-Factor Authentication</DialogTitle>
                        <DialogDescription>
                            Enter your password to disable 2FA. This will make your account less secure.
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-2">
                        <Label htmlFor="disablePassword">Password</Label>
                        <Input
                            id="disablePassword"
                            type="password"
                            value={disablePassword}
                            onChange={(e) => setDisablePassword(e.target.value)}
                            placeholder="Enter your password"
                        />
                    </div>

                    <DialogFooter>
                        <Button variant="outline" onClick={() => setIsDisableDialogOpen(false)}>
                            Cancel
                        </Button>
                        <Button
                            variant="destructive"
                            onClick={disableTwoFactor}
                            disabled={isVerifying || !disablePassword}
                        >
                            {isVerifying && <FontAwesomeIcon icon={faSpinner} className="h-4 w-4 mr-2 animate-spin" />}
                            Disable 2FA
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            <Dialog open={isRecoveryCodesDialogOpen} onOpenChange={setIsRecoveryCodesDialogOpen}>
                <DialogContent className="sm:max-w-lg">
                    <DialogHeader>
                        <DialogTitle>Recovery Codes</DialogTitle>
                        <DialogDescription>
                            Save these recovery codes in a safe place. You can use them to access your account if you lose your authenticator.
                        </DialogDescription>
                    </DialogHeader>

                    <Alert variant="destructive">
                        <AlertDescription>
                            Each code can only be used once. Generate new codes if you run out.
                        </AlertDescription>
                    </Alert>

                    <div className="grid grid-cols-2 gap-2 p-4 bg-muted rounded-lg font-mono text-sm">
                        {recoveryCodes.map((code, index) => (
                            <div key={index} className="p-2 bg-background rounded">
                                {code}
                            </div>
                        ))}
                    </div>

                    <DialogFooter>
                        <Button
                            variant="outline"
                            onClick={() => copyToClipboard(recoveryCodes.join('\n'), 'codes')}
                        >
                            {copiedCode === 'codes' ? (
                                <FontAwesomeIcon icon={faCheck} className="h-4 w-4 mr-2" />
                            ) : (
                                <FontAwesomeIcon icon={faCopy} className="h-4 w-4 mr-2" />
                            )}
                            Copy All
                        </Button>
                        <Button onClick={() => setIsRecoveryCodesDialogOpen(false)}>
                            Done
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </>
    );
}
