import { NextRouter } from 'next/router';

export const isMobile = () => {
  const userAgent =
    typeof window.navigator === 'undefined' ? '' : navigator.userAgent;
  const mobileRegex =
    /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i;
  return mobileRegex.test(userAgent);
};

export function formatNumberAsMoney(amount: number, maximumFractionDigits = 5) {
  return new Intl.NumberFormat('zh-CN', { maximumFractionDigits }).format(
    amount,
  );
}

export function termDateString() {
  return new Date(
    new Date().getTime() + 10 * 365 * 24 * 60 * 60 * 1000,
  ).toISOString();
}

export const PhoneRegExp = /^[1][3,4,5,6,7,8,9][0-9]{9}$/;
export const SmsExpirationSeconds = 300;

export const getApiUrl = () =>
  typeof window !== 'undefined'
    ? (window as any)['API_URL'] || ''
    : process.env.API_URL;

export const getQueryId = (router: NextRouter): string => {
  const { id } = router.query;
  if (id) {
    if (Array.isArray(id)) {
      return id[0];
    } else {
      return id;
    }
  }
  const asPath = router.asPath.split('?')[0];
  const pathSegments = asPath.split('/');
  return pathSegments[pathSegments.length - 1] || '';
};
