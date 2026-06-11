import { Component, EventEmitter, Output, signal } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Router } from '@angular/router';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { UserAuthService } from '../../../../services/common/models/user-auth.service';
import { AuthorizeDefinitionConstants } from '../../../../constants/authorize-definition.constants';
import { AuthorizationEndpointService } from '../../../../services/common/models/authorization-endpoint.service';

export interface MenuItem {
  icon: string;
  iconClass: string;
  label: string;
  route: string;
  exact: boolean;
  menuName?: string;
  subItems?: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  animations: [
    trigger('slideVertical', [
      transition(':enter', [
        style({ height: '0', opacity: 0 }),
        animate('200ms ease-out', style({ height: '*', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ height: '0', opacity: 0 }))
      ])
    ]),
    trigger('rotateIcon', [
      state('true', style({ transform: 'rotate(90deg)' })),
      state('false', style({ transform: 'rotate(0deg)' })),
      transition('true <=> false', animate('200ms ease-in-out'))
    ])
  ]
})
export class SidebarComponent {

  collapsed = signal(false);
  private layoutSubscription: any; // Track screen size with BreakpointObserver

  constructor(
    private breakpointObserver: BreakpointObserver,
    private router: Router,
    private authorizationEndpointService: AuthorizationEndpointService,
    private userAuthService: UserAuthService
  ) { }

  ngOnInit() {
    this.buildFilteredMenu();

    this.layoutSubscription = this.breakpointObserver.observe([Breakpoints.XSmall, Breakpoints.Small])
      .subscribe(result => {
        if (result.matches) {
          // Collapse the sidenav when screen width is small
          this.collapsed.set(true);
        } else {
          // Expand the sidenav for larger screens
          this.collapsed.set(false);
        }
      });
  }

  ngOnDestroy() {
    if (this.layoutSubscription) {
      this.layoutSubscription.unsubscribe(); // Cleanup subscription when component is destroyed
    }
  }

  // We use EventEmitter to change the collapsed state.
  @Output() collapsedChange = new EventEmitter<boolean>();

  // We send the value to the parent with collapsedChange.
  emitCollapseChange() {
    this.collapsedChange.emit(this.collapsed());
  }

  get collapsedClass() {
    return this.collapsed() ? 'collapsed' : 'expanded';
  }

  // Build menu items based on the user's accessible menus.
  filteredMenuItems = signal<MenuItem[]>([]);
  buildFilteredMenu() {
    const accessibleMenuNames = this.userAuthService.accessibleMenus;
    const allItems = this.menuItems();
    const visibleItems: MenuItem[] = [];

    for (const item of allItems) {

      if (item.subItems) {
        const visibleSubItems = item.subItems.filter(subItem => {
          if (!subItem.menuName) return true;
          return accessibleMenuNames.includes(subItem.menuName);
        });
        // Show the parent menu when at least one child item is visible.
        if (visibleSubItems.length > 0) {
          visibleItems.push({ ...item, subItems: visibleSubItems });
        }
        continue;
      }
      // Show standalone items that do not require a menu permission.
      if (!item.menuName) {
        visibleItems.push(item);
        continue;
      }
      // Show items with a menu permission only when the user has access.
      if (accessibleMenuNames.includes(item.menuName)) {
        visibleItems.push(item);
      }
    }
    // Update the filtered menu signal.
    this.filteredMenuItems.set(visibleItems);
  }

  menuItems = signal<MenuItem[]>([
    {
      icon: 'dashboard',
      iconClass: '',
      label: 'Dashboard',
      route: '/admin',
      exact: true
    },
    {
      icon: 'group',
      iconClass: '',
      label: 'Customers',
      route: '/admin/customers',
      exact: true
    },
    {
      icon: 'inventory_2',
      iconClass: '',
      label: 'Products',
      route: '/admin/products',
      exact: true,
      menuName: AuthorizeDefinitionConstants.Products
    },
    {
      icon: 'inventory',
      iconClass: 'material-icons-outlined',
      label: 'Orders',
      route: '/admin/orders',
      exact: false,
      menuName: AuthorizeDefinitionConstants.Orders
    },
    {
      icon: 'settings',
      iconClass: '',
      label: 'Administration',
      route: '',
      exact: true,
      subItems: [
        {
          icon: 'security',
          iconClass: '',
          label: 'Roles',
          route: '/admin/administration/roles',
          exact: true,
          menuName: AuthorizeDefinitionConstants.Roles
        },
        {
          icon: 'lock',
          iconClass: '',
          label: 'Role Access',
          route: '/admin/administration/role-access',
          exact: true,
          menuName: AuthorizeDefinitionConstants.Endpoints
        },
        {
          icon: 'person',
          iconClass: '',
          label: 'Users',
          route: '/admin/administration/users',
          exact: true,
          menuName: AuthorizeDefinitionConstants.Users
        }
      ]
    },
    {
      icon: 'domain',
      iconClass: '',
      label: 'Show the Main Site',
      route: '',
      exact: true
    }
  ]);

  isOrdersRouteActive(): boolean {
    return this.router.url.startsWith('/admin/orders/order-detail');
  }

  isAdministrationRouteActive(): boolean {
    return this.router.url.startsWith('/admin/administration/');
  }

  nestedMenuOpen = signal(this.isAdministrationRouteActive());
  toggleNested(item: any) {
    if (!item.subItems)
      return;
    this.nestedMenuOpen.set(!this.nestedMenuOpen());
  }
}
