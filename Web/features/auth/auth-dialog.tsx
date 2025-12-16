"use client";

import { useState, useEffect } from "react";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faUser,
    faEnvelope,
    faLock,
    faEye,
    faEyeSlash,
    faRightToBracket,
    faUserPlus,
    faCheckCircle,
    faSpinner,
} from "@fortawesome/free-solid-svg-icons";
import {
    faGoogle,
    faFacebook,
} from "@fortawesome/free-brands-svg-icons";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Separator } from "@/components/ui/separator";

type AuthMode = "signin" | "register";

interface AuthDialogProps {
    trigger?: React.ReactNode;
    defaultMode?: AuthMode;
    onSuccess?: () => void;
    redirectUrl?: string;
}

interface RegisterFormData {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
}

export function AuthDialog({
    trigger,
    defaultMode = "signin",
    onSuccess,
    redirectUrl,
}: AuthDialogProps) {
    const router = useRouter();
    const [isOpen, setIsOpen] = useState(false);
    const [mode, setMode] = useState<AuthMode>(defaultMode);
    const [loading, setLoading] = useState(false);
    const [oauthLoading, setOauthLoading] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [externalProviders, setExternalProviders] = useState<string[]>([]);

    const [signInData, setSignInData] = useState({ username: "", password: "" });
    const [registerData, setRegisterData] = useState<RegisterFormData>({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
    });

    const identityServerUrl = process.env.NEXT_PUBLIC_IDENTITY_SERVER_URL || "http://localhost:5001";
    const callbackUrl = redirectUrl || window.location.href;

    useEffect(() => {
        const fetchProviders = async () => {
            try {
                const response = await fetch(`${identityServerUrl}/api/account/external-providers`);
                if (response.ok) {
                    const result = await response.json();
                    setExternalProviders(result.data || []);
                }
            } catch {
            }
        };
        fetchProviders();
    }, [identityServerUrl]);

    const resetForm = () => {
        setSignInData({ username: "", password: "" });
        setRegisterData({ username: "", email: "", password: "", confirmPassword: "" });
        setError(null);
        setSuccess(false);
        setShowPassword(false);
        setShowConfirmPassword(false);
    };

    const handleOpenChange = (open: boolean) => {
        setIsOpen(open);
        if (!open) {
            resetForm();
            setMode(defaultMode);
        }
    };

    const switchMode = (newMode: AuthMode) => {
        setMode(newMode);
        setError(null);
        setSuccess(false);
    };

    const handleSignIn = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!signInData.username || !signInData.password) {
            setError("Please enter both username and password");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const result = await signIn("id-server", {
                username: signInData.username,
                password: signInData.password,
                redirect: false,
                callbackUrl,
            });

            if (result?.error) {
                setError("Invalid username or password");
            } else if (result?.ok) {
                setIsOpen(false);
                onSuccess?.();
                router.refresh();
            }
        } catch {
            setError("An error occurred during sign in");
        } finally {
            setLoading(false);
        }
    };

    const validateRegisterForm = (): string | null => {
        if (!registerData.username || !registerData.email || !registerData.password || !registerData.confirmPassword) {
            return "All fields are required";
        }
        if (registerData.username.length < 3) {
            return "Username must be at least 3 characters";
        }
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(registerData.email)) {
            return "Please enter a valid email address";
        }
        if (registerData.password.length < 6) {
            return "Password must be at least 6 characters";
        }
        if (registerData.password !== registerData.confirmPassword) {
            return "Passwords do not match";
        }
        return null;
    };

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();

        const validationError = validateRegisterForm();
        if (validationError) {
            setError(validationError);
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const response = await fetch(`${identityServerUrl}/api/account/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    username: registerData.username,
                    email: registerData.email,
                    password: registerData.password,
                }),
            });

            const result = await response.json();

            if (!response.ok) {
                const errorMessage = result.message || "Registration failed";
                const errors = result.errors ? result.errors.join(", ") : "";
                throw new Error(errors || errorMessage);
            }

            setSuccess(true);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Registration failed. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const handleOAuthLogin = (provider: string) => {
        setOauthLoading(provider);
        window.location.href = `${identityServerUrl}/api/account/external-login?provider=${provider}&returnUrl=${encodeURIComponent(callbackUrl)}`;
    };

    return (
        <Dialog open={isOpen} onOpenChange={handleOpenChange}>
            <DialogTrigger asChild>
                {trigger || (
                    <Button className="h-12 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
                        <FontAwesomeIcon icon={faRightToBracket} className="mr-2 w-4 h-4" />
                        Sign In
                    </Button>
                )}
            </DialogTrigger>
            <DialogContent className="sm:max-w-[440px] p-0 overflow-hidden border-0 shadow-2xl bg-white dark:bg-slate-900">
                <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-purple-600 to-blue-600" />
                
                <AnimatePresence mode="wait">
                    {success ? (
                        <motion.div
                            key="success"
                            initial={{ opacity: 0, scale: 0.95 }}
                            animate={{ opacity: 1, scale: 1 }}
                            exit={{ opacity: 0, scale: 0.95 }}
                            className="p-8 text-center"
                        >
                            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-gradient-to-br from-emerald-500 to-green-600 flex items-center justify-center shadow-lg shadow-emerald-500/25">
                                <FontAwesomeIcon icon={faCheckCircle} className="w-8 h-8 text-white" />
                            </div>
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">
                                Check Your Email
                            </h3>
                            <p className="text-slate-500 dark:text-slate-400 mb-6">
                                We&apos;ve sent a confirmation link to <strong className="text-slate-700 dark:text-slate-300">{registerData.email}</strong>.
                                Please check your inbox to activate your account.
                            </p>
                            <Button
                                onClick={() => switchMode("signin")}
                                className="w-full h-11 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
                            >
                                <FontAwesomeIcon icon={faRightToBracket} className="mr-2 w-4 h-4" />
                                Go to Sign In
                            </Button>
                        </motion.div>
                    ) : (
                        <motion.div
                            key={mode}
                            initial={{ opacity: 0, x: mode === "signin" ? -20 : 20 }}
                            animate={{ opacity: 1, x: 0 }}
                            exit={{ opacity: 0, x: mode === "signin" ? 20 : -20 }}
                            transition={{ duration: 0.2 }}
                            className="p-6"
                        >
                            <DialogHeader className="mb-6">
                                <div className="flex items-center gap-3 mb-2">
                                    <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-600 to-blue-600 flex items-center justify-center shadow-lg shadow-purple-500/25">
                                        <FontAwesomeIcon 
                                            icon={mode === "signin" ? faRightToBracket : faUserPlus} 
                                            className="w-5 h-5 text-white" 
                                        />
                                    </div>
                                    <DialogTitle className="text-xl font-bold text-slate-900 dark:text-white">
                                        {mode === "signin" ? "Welcome Back" : "Create Account"}
                                    </DialogTitle>
                                </div>
                                <DialogDescription className="text-slate-500 dark:text-slate-400">
                                    {mode === "signin"
                                        ? "Enter your credentials to access your account"
                                        : "Fill in your details to create a new account"}
                                </DialogDescription>
                            </DialogHeader>

                            {error && (
                                <Alert variant="destructive" className="mb-4 border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/30">
                                    <AlertDescription className="text-red-600 dark:text-red-400">
                                        {error}
                                    </AlertDescription>
                                </Alert>
                            )}

                            <form onSubmit={mode === "signin" ? handleSignIn : handleRegister} className="space-y-4">
                                {mode === "signin" ? (
                                    <>
                                        <div className="space-y-2">
                                            <Label htmlFor="signin-username" className="text-slate-700 dark:text-slate-300">
                                                Username
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faUser} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="signin-username"
                                                    type="text"
                                                    placeholder="Enter your username"
                                                    value={signInData.username}
                                                    onChange={(e) => setSignInData(prev => ({ ...prev, username: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                            </div>
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor="signin-password" className="text-slate-700 dark:text-slate-300">
                                                Password
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faLock} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="signin-password"
                                                    type={showPassword ? "text" : "password"}
                                                    placeholder="Enter your password"
                                                    value={signInData.password}
                                                    onChange={(e) => setSignInData(prev => ({ ...prev, password: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 pr-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                                <button
                                                    type="button"
                                                    onClick={() => setShowPassword(!showPassword)}
                                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                                                >
                                                    <FontAwesomeIcon icon={showPassword ? faEyeSlash : faEye} className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </div>
                                    </>
                                ) : (
                                    <>
                                        <div className="space-y-2">
                                            <Label htmlFor="register-username" className="text-slate-700 dark:text-slate-300">
                                                Username
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faUser} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="register-username"
                                                    type="text"
                                                    placeholder="Choose a username"
                                                    value={registerData.username}
                                                    onChange={(e) => setRegisterData(prev => ({ ...prev, username: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                            </div>
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor="register-email" className="text-slate-700 dark:text-slate-300">
                                                Email
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faEnvelope} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="register-email"
                                                    type="email"
                                                    placeholder="Enter your email"
                                                    value={registerData.email}
                                                    onChange={(e) => setRegisterData(prev => ({ ...prev, email: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                            </div>
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor="register-password" className="text-slate-700 dark:text-slate-300">
                                                Password
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faLock} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="register-password"
                                                    type={showPassword ? "text" : "password"}
                                                    placeholder="Create a password"
                                                    value={registerData.password}
                                                    onChange={(e) => setRegisterData(prev => ({ ...prev, password: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 pr-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                                <button
                                                    type="button"
                                                    onClick={() => setShowPassword(!showPassword)}
                                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                                                >
                                                    <FontAwesomeIcon icon={showPassword ? faEyeSlash : faEye} className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </div>

                                        <div className="space-y-2">
                                            <Label htmlFor="register-confirm" className="text-slate-700 dark:text-slate-300">
                                                Confirm Password
                                            </Label>
                                            <div className="relative">
                                                <FontAwesomeIcon 
                                                    icon={faLock} 
                                                    className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" 
                                                />
                                                <Input
                                                    id="register-confirm"
                                                    type={showConfirmPassword ? "text" : "password"}
                                                    placeholder="Confirm your password"
                                                    value={registerData.confirmPassword}
                                                    onChange={(e) => setRegisterData(prev => ({ ...prev, confirmPassword: e.target.value }))}
                                                    disabled={loading}
                                                    className="pl-10 pr-10 h-11 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                                    required
                                                />
                                                <button
                                                    type="button"
                                                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                                                >
                                                    <FontAwesomeIcon icon={showConfirmPassword ? faEyeSlash : faEye} className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </div>
                                    </>
                                )}

                                <Button
                                    type="submit"
                                    disabled={loading}
                                    className="w-full h-11 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 rounded-xl font-semibold"
                                >
                                    {loading ? (
                                        <>
                                            <FontAwesomeIcon icon={faSpinner} className="mr-2 w-4 h-4 animate-spin" />
                                            {mode === "signin" ? "Signing in..." : "Creating account..."}
                                        </>
                                    ) : (
                                        <>
                                            <FontAwesomeIcon 
                                                icon={mode === "signin" ? faRightToBracket : faUserPlus} 
                                                className="mr-2 w-4 h-4" 
                                            />
                                            {mode === "signin" ? "Sign In" : "Create Account"}
                                        </>
                                    )}
                                </Button>
                            </form>

                            {mode === "signin" && externalProviders.length > 0 && (
                                <>
                                    <div className="relative my-6">
                                        <Separator className="bg-slate-200 dark:bg-slate-700" />
                                        <span className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-white dark:bg-slate-900 px-3 text-xs text-slate-500 uppercase tracking-wider">
                                            Or continue with
                                        </span>
                                    </div>

                                    <div className="grid grid-cols-2 gap-3">
                                        {externalProviders.includes("Google") && (
                                            <Button
                                                type="button"
                                                variant="outline"
                                                onClick={() => handleOAuthLogin("Google")}
                                                disabled={!!oauthLoading}
                                                className="h-11 rounded-xl border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800"
                                            >
                                                {oauthLoading === "Google" ? (
                                                    <FontAwesomeIcon icon={faSpinner} className="mr-2 w-4 h-4 animate-spin" />
                                                ) : (
                                                    <FontAwesomeIcon icon={faGoogle} className="mr-2 w-4 h-4 text-red-500" />
                                                )}
                                                Google
                                            </Button>
                                        )}
                                        {externalProviders.includes("Facebook") && (
                                            <Button
                                                type="button"
                                                variant="outline"
                                                onClick={() => handleOAuthLogin("Facebook")}
                                                disabled={!!oauthLoading}
                                                className="h-11 rounded-xl border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800"
                                            >
                                                {oauthLoading === "Facebook" ? (
                                                    <FontAwesomeIcon icon={faSpinner} className="mr-2 w-4 h-4 animate-spin" />
                                                ) : (
                                                    <FontAwesomeIcon icon={faFacebook} className="mr-2 w-4 h-4 text-blue-600" />
                                                )}
                                                Facebook
                                            </Button>
                                        )}
                                    </div>
                                </>
                            )}

                            <div className="mt-6 text-center text-sm text-slate-500 dark:text-slate-400">
                                {mode === "signin" ? (
                                    <>
                                        Don&apos;t have an account?{" "}
                                        <button
                                            type="button"
                                            onClick={() => switchMode("register")}
                                            className="font-semibold text-purple-600 hover:text-purple-700 dark:text-purple-400 dark:hover:text-purple-300"
                                        >
                                            Register
                                        </button>
                                    </>
                                ) : (
                                    <>
                                        Already have an account?{" "}
                                        <button
                                            type="button"
                                            onClick={() => switchMode("signin")}
                                            className="font-semibold text-purple-600 hover:text-purple-700 dark:text-purple-400 dark:hover:text-purple-300"
                                        >
                                            Sign In
                                        </button>
                                    </>
                                )}
                            </div>
                        </motion.div>
                    )}
                </AnimatePresence>
            </DialogContent>
        </Dialog>
    );
}
