/**
 * Role-based navigation config for the unified app.
 * Menu items shown based on logged-in user role.
 */

export interface NavItem {
  path: string;
  label: string;
  icon: string;
  /** Roles that can see this item. Empty = all authenticated. */
  roles: string[];
}

export const NAV_ITEMS: NavItem[] = [
  // Shared (all roles)
  { path: '/dashboard', label: 'Dashboard', icon: '📊', roles: [] },
  { path: '/notifications', label: 'Notifications', icon: '🔔', roles: [] },

  // Admin + role-specific
  { path: '/inventory', label: 'Inventory', icon: '📦', roles: ['Admin', 'FarmManager'] },
  { path: '/orders', label: 'Orders', icon: '📋', roles: ['Admin', 'ShopOwner'] },
  { path: '/distributions', label: 'Logistics', icon: '🚚', roles: ['Admin', 'Driver'] },
  { path: '/deliveries', label: 'Deliveries', icon: '📬', roles: ['Admin', 'Driver', 'ShopOwner'] },
  { path: '/sales', label: 'Sales', icon: '💰', roles: ['Admin', 'ShopOwner'] },
  { path: '/reports', label: 'Reports', icon: '📈', roles: ['Admin'] },
  { path: '/suppliers', label: 'Suppliers', icon: '🏭', roles: ['Admin', 'FarmManager'] },
  { path: '/farms', label: 'Farms', icon: '🏡', roles: ['Admin', 'FarmManager'] },
  { path: '/chickens', label: 'Chickens', icon: '🐔', roles: ['Admin', 'FarmManager'] },
  { path: '/vehicles', label: 'Vehicles', icon: '🚛', roles: ['Admin'] },
  { path: '/expenses', label: 'Financials', icon: '💵', roles: ['Admin'] },
  { path: '/stock-movements', label: 'Stock Movements', icon: '📤', roles: ['Admin', 'FarmManager'] },

  // Shop / Customer
  { path: '/payments', label: 'Payments', icon: '💳', roles: ['ShopOwner'] },
  { path: '/profile', label: 'Profile', icon: '👤', roles: ['ShopOwner', 'Driver', 'FarmManager'] },
];

/** Get nav items visible for the given role. */
export function getNavItemsForRole(role: string): NavItem[] {
  return NAV_ITEMS.filter((item) => {
    if (item.roles.length === 0) return true;
    return item.roles.includes(role);
  });
}
