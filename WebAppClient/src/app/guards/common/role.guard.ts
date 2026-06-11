import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserAuthService } from '../../services/common/models/user-auth.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { SpinnerType } from '../../base/base.component';
import { CustomToastrService, ToastrMessageType, ToastrPosition } from '../../services/ui/custom-toastr.service';
import { AuthorizationEndpointService } from '../../services/common/models/authorization-endpoint.service';

export const roleGuard: CanActivateFn = async (route, state) => {
  const userAuthService = inject(UserAuthService);
  const authorizationEndpointService = inject(AuthorizationEndpointService);
  const router = inject(Router);
  const spinner: NgxSpinnerService = inject(NgxSpinnerService);
  const toastrService: CustomToastrService = inject(CustomToastrService);

  spinner.show(SpinnerType.BallAtom);

  const url = state.url;
  const menuName = route.data?.['menuName'] as string;

  try {
    let isAdmin: boolean = null;

    await userAuthService.identityCheck(result => {
      isAdmin = result?.isAdmin;
    });

    // Check whether the requested route belongs to the admin panel.
    if (url.startsWith('/admin')) {
      // If this is an admin panel route, verify that the user has admin access.
      if (!isAdmin) {
        spinner.hide(SpinnerType.BallAtom);
        toastrService.message("You are not authorized to view this page2.", "Unauthorized Access!", {
          messageType: ToastrMessageType.Warning,
          position: ToastrPosition.TopRight
        });
        return router.parseUrl('/');
      }

      // No specific menu name is defined. This is likely a general admin page.
      if (!menuName) {
        spinner.hide(SpinnerType.BallAtom); // Prevent spinner errors if the page is created but empty
        return true;
      }

      // Check endpoint-role access for the admin panel route.
      const hasAccess = await authorizationEndpointService.hasAccessToMenu(menuName);
      if (hasAccess) {
        spinner.hide(SpinnerType.BallAtom);
        return true;
      }
      else {
        spinner.hide(SpinnerType.BallAtom);
        toastrService.message("You are not authorized to view this page3.", "Unauthorized Access!", {
          messageType: ToastrMessageType.Warning,
          position: ToastrPosition.TopRight
        });
        return router.parseUrl('/admin');
      }
    }

    // No specific menu name is defined. This is likely a general UI page.
    if (!menuName) {
      spinner.hide(SpinnerType.BallAtom); // Prevent spinner errors if the page is created but empty
      return true;
    }

    // Check endpoint-role access for non-admin UI routes.
    const hasAccess = await authorizationEndpointService.hasAccessToMenu(menuName);
    if (hasAccess) {
      spinner.hide(SpinnerType.BallAtom);
      return true;
    } else {
      spinner.hide(SpinnerType.BallAtom);
      toastrService.message("You are not authorized to view this page.", "Unauthorized Access!", {
        messageType: ToastrMessageType.Warning,
        position: ToastrPosition.TopRight
      });
      return router.parseUrl('/');
    }
  } catch (err) {
    console.error("RoleGuard error:", err);
    spinner.hide(SpinnerType.BallAtom);
    return router.parseUrl('/');
  }
};
