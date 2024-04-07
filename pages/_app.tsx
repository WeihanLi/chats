import { Toaster } from 'react-hot-toast';
import type { AppProps } from 'next/app';
import './globals.css';
import { QueryClient, QueryClientProvider } from 'react-query';
import { appWithTranslation } from 'next-i18next';
import { useRouter } from 'next/router';
import AdminLayout from './admin/layout/layout';
import { ThemeProvider } from '@/components/theme-provider';
import { SessionProvider } from 'next-auth/react';

function App({ Component, pageProps }: AppProps<{}> | any) {
  const route = useRouter();
  const queryClient = new QueryClient();
  return (
    <ThemeProvider
      attribute='class'
      defaultTheme='light'
      enableSystem
      disableTransitionOnChange
    >
      <Toaster />
      <QueryClientProvider client={queryClient}>
        {route.pathname.includes('/admin') ? (
          <AdminLayout>
            <Component {...pageProps} />
          </AdminLayout>
        ) : route.pathname.includes('/authorizing') ? (
          <SessionProvider>
            <Component {...pageProps} />
          </SessionProvider>
        ) : (
          <Component {...pageProps} />
        )}
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default appWithTranslation(App);
