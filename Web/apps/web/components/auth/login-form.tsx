"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { Button, Input, Label, Card, CardContent, CardFooter, Spinner } from "@repo/ui";
import { loginSchema, type LoginInput } from "@repo/validators";
import { useAuth } from "@repo/hooks";

export function LoginForm() {
  const router = useRouter();
  const { loginAsync, isLoggingIn, loginError } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  async function onSubmit(data: LoginInput) {
    try {
      await loginAsync(data);
      router.push("/dashboard");
    } catch {
      // Error handled by mutation
    }
  }

  return (
    <Card>
      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-4 pt-6">
          {loginError && (
            <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {loginError.message || "Invalid email or password"}
            </div>
          )}
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="you@example.com"
              {...register("email")}
              aria-invalid={!!errors.email}
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              {...register("password")}
              aria-invalid={!!errors.password}
            />
            {errors.password && (
              <p className="text-sm text-destructive">{errors.password.message}</p>
            )}
          </div>
        </CardContent>
        <CardFooter>
          <Button type="submit" className="w-full" disabled={isLoggingIn}>
            {isLoggingIn ? <Spinner size="sm" /> : "Sign In"}
          </Button>
        </CardFooter>
      </form>
    </Card>
  );
}
