import type { Metadata } from "next";
import { ThemeProvider } from "next-themes";
import { AuthProvider } from "@/lib/auth-context";
import { Toaster } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import "./globals.css";

export const metadata: Metadata = {
  title: "BIMSS",
  description: "Buklod Integrated Membership and Services System",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full antialiased" suppressHydrationWarning>
      <body className="min-h-full flex flex-col">
        <ThemeProvider attribute="class" defaultTheme="light" enableSystem={false}>
          <AuthProvider>
            <TooltipProvider>{children}</TooltipProvider>
            <Toaster />
          </AuthProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
