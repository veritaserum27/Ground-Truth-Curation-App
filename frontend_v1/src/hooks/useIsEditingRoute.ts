import { useLocation } from 'react-router';

export function useIsEditingRoute() {
  const { pathname } = useLocation();
  return pathname.endsWith('/edit');
}
