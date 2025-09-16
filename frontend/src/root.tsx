import {
  isRouteErrorResponse,
  Links,
  Meta, Outlet, Scripts, ScrollRestoration
} from 'react-router';
import type { Route } from "./+types/root";
import { AuthProvider } from './contexts/AuthContext';
import { DataProvider } from './contexts/DataContext';
import { Header } from './components/Header';

export function Layout({ children }: { children?: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Ground Truth Q&A Application</title>
        <Meta />
        <Links />
      </head>

      <body>
        {children}
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  )
}



export default function Root() {
  return (
    <AuthProvider>
      <DataProvider>
        <div className="min-h-screen bg-background">
          <Header />
          <main className="container mx-auto px-4 py-8">
            <Outlet />
          </main>
        </div>
      </DataProvider>
    </AuthProvider>
  )
}

export function ErrorBoundary({ error }: Route.ErrorBoundaryProps) {
  let message = "Oops!";
  let details = "An unexpected error occurred.";
  let stack: string | undefined;

  if (isRouteErrorResponse(error)) {
    message = error.status === 404 ? "404" : "Error";
    details =
      error.status === 404
        ? "The requested page could not be found."
        : error.statusText || details;
  } else if (import.meta.env.DEV && error && error instanceof Error) {
    details = error.message;
    stack = error.stack;
  }

  return (
    <main className="pt-16 p-4 container mx-auto">
      <h1>{message}</h1>
      <p>{details}</p>
      {stack && (
        <pre className="w-full p-4 overflow-x-auto">
          <code>{stack}</code>
        </pre>
      )}
    </main>
  );
}
